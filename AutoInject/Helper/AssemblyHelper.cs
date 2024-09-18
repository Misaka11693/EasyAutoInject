using Microsoft.Extensions.DependencyModel;

using System.Reflection;
using System.Runtime.Loader;

namespace AutoInject;

/// <summary>
/// Provides utility methods for loading assemblies.
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// Retrieves all assemblies that belong to the project (excluding system assemblies and NuGet packages).
    /// </summary>
    /// <returns>An enumerable collection of assemblies in the project.</returns>
    public static IEnumerable<Assembly> GetAllAssemblies()
    {
        var result = new List<Assembly>();
        // Get the list of libraries used to compile the application
        var libs = DependencyContext.Default!.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package");
        foreach (var lib in libs)
        {
            try
            {
                // Load the assembly from its name
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                result.Add(assembly);
            }
            catch (Exception ex)
            {
                // Throw an exception if the assembly cannot be loaded
                throw new InvalidOperationException($"Failed to load assembly {lib.Name}.", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Loads specified assemblies into AssemblyLoadContext.Default and returns them.
    /// If no parameters are provided, returns all project assemblies.
    /// This method is primarily used to address situations where assemblies are not loaded.
    /// </summary>
    /// <param name="dllNames">One or more assembly names, such as 'Test' or 'Test.dll'.</param>
    /// <returns>A list of loaded assemblies.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified DLL file does not exist.</exception>
    /// <exception cref="BadImageFormatException">Thrown if the image is invalid.</exception>
    /// <exception cref="Exception">Thrown if there is an error loading the assembly.</exception>
    public static IEnumerable<Assembly> GetAssemblies(params string[] dllNames)
    {
        if (!dllNames.Any())
        {
            // Return all assemblies if no specific DLL names are provided
            return GetAllAssemblies();
        }

        var basePath = AppContext.BaseDirectory;
        var assemblies = new List<Assembly>();

        foreach (var dllName in dllNames)
        {
            var dllFileName = dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? dllName : dllName + ".dll";
            var dllFileFullName = Path.Combine(basePath, dllFileName);

            // Check if the file exists (case-insensitive search)
            if (!File.Exists(dllFileFullName))
            {
                throw new FileNotFoundException($"File {dllFileFullName} does not exist!");
            }

            try
            {
                // Load the assembly from the full path
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFileFullName);
                assemblies.Add(assembly);
            }
            catch (BadImageFormatException ex)
            {
                throw new BadImageFormatException($"Failed to load assembly {dllFileName}: Invalid image.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load assembly {dllFileName}.", ex);
            }
        }

        return assemblies;
    }
}