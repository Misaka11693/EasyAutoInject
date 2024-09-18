using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AutoInject;

/// <summary>
/// Provides helper methods for automatically registering services marked with the AutoInject attribute.
/// </summary>
public static class RegisterHelper
{
    /// <summary>
    /// Automatically registers services that are marked with the AutoInject attribute.
    /// 
    /// If DLL names are provided, only assemblies from those DLLs are registered; otherwise, all assemblies in the application domain are registered.
    /// Supports specifying an interface name suffix to filter interfaces by.
    /// </summary>
    /// <param name="services">The collection of services to register.</param>
    /// <param name="interfaceSuffix">An optional suffix to filter interface names. Defaults to null (no suffix).</param>
    /// <param name="dllNames">Optional array of strings containing the names of DLLs from which to load and register services. If empty, registers all assemblies.</param>
    public static void AutoInject(this IServiceCollection services, string? interfaceSuffix = null, params string[] dllNames)
    {
        // Trim trailing spaces from interfaceSuffix
        interfaceSuffix = interfaceSuffix?.TrimEnd();

        // Get assemblies from specified DLL names or all assemblies
        var assemblies = AssemblyHelper.GetAssemblies(dllNames);

        // Iterate through each assembly and register services marked with the AutoInject attribute
        foreach (var assembly in assemblies)
        {
            RegisterServices(services, assembly, interfaceSuffix);
        }
    }

    /// <summary>
    /// Registers services marked with the AutoInject attribute.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="interfaceSuffix">The optional suffix to filter interface names. If null, no suffix check is performed.</param>
    private static void RegisterServices(IServiceCollection services, Assembly assembly, string? interfaceSuffix)
    {
        // Get non-abstract class types within the assembly that have the AutoInject attribute, excluding interfaces
        var typesWithAttribute = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetCustomAttribute<AutoInjectAttribute>() != null);

        foreach (var type in typesWithAttribute)
        {
            var attribute = type.GetCustomAttribute<AutoInjectAttribute>();
            if (attribute!.AutoRegister)
            {
                // Get the service types to be registered
                var serviceTypes = GetServiceTypes(type, attribute, interfaceSuffix);

                // Register each service type into the service collection
                foreach (var serviceType in serviceTypes)
                {
                    RegisterService(services, serviceType, type, attribute.Lifetime);
                }

                // Register the type itself if RegisterSelfIfImpl is true and it's not already included
                if (attribute.RegisterSelfIfImpl && !serviceTypes.Contains(type))
                {
                    RegisterService(services, type, type, attribute.Lifetime);
                }
            }
        }
    }

    /// <summary>
    /// Gets the service types.
    /// </summary>
    /// <param name="type">The implementation type.</param>
    /// <param name="attribute">The AutoInject attribute.</param>
    /// <param name="interfaceSuffix">The optional suffix to filter interface names.</param>
    /// <returns>A set of service types.</returns>
    private static HashSet<Type> GetServiceTypes(Type type, AutoInjectAttribute attribute, string? interfaceSuffix)
    {
        var serviceTypes = new HashSet<Type>();

        if (attribute.RegisterTypes != null)
        {
            // If RegisterTypes is specified, register only these types
            foreach (var registerType in attribute.RegisterTypes)
            {
                if (registerType == type || type.GetInterfaces().Contains(registerType))
                {
                    serviceTypes.Add(registerType);
                }
                else
                {
                    throw new InvalidOperationException($"In the AutoInjectAttribute of type {type.FullName}, the specified RegisterType({registerType.FullName}) must be the type itself or an implemented interface.");
                }
            }
        }
        else
        {
            // If no RegisterTypes are specified, register all interfaces or interfaces ending with the specified suffix
            serviceTypes.UnionWith(type.GetInterfaces()
                .Where(i => interfaceSuffix == null || i.Name.EndsWith(interfaceSuffix, StringComparison.OrdinalIgnoreCase)));

            // If no interfaces were found and no suffix was specified, register the type itself
            if (!serviceTypes.Any() && string.IsNullOrEmpty(interfaceSuffix))
            {
                serviceTypes.Add(type);
            }
        }

        return serviceTypes;
    }

    /// <summary>
    /// Registers an implementation.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <param name="lifetime">The lifetime of the service.</param>
    private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            default: // Transient
                services.AddTransient(serviceType, implementationType);
                break;
        }

        // Log the registration details
        Console.WriteLine($"Registration method: {lifetime} Name: {serviceType.Name} Instance: {implementationType.Name}");
    }
}