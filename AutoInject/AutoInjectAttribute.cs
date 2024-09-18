using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AutoInject
{
    /// <summary>
    /// An attribute for specifying dependency injection behavior for a class.
    /// This attribute cannot be applied to multiple classes or inherited by other classes.
    /// The default service lifetime is transient (<see cref="ServiceLifetime.Transient"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoInjectAttribute : Attribute
    {
        /// <summary>
        /// The service lifetime type.
        /// Supported options include:
        /// <list type="bullet">
        /// <item><description>Transient (<see cref="ServiceLifetime.Transient"/>): Creates a new instance for each request.</description></item>
        /// <item><description>Scoped (<see cref="ServiceLifetime.Scoped"/>): Shares the same instance within the scope of a request.</description></item>
        /// <item><description>Singleton (<see cref="ServiceLifetime.Singleton"/>): Shares a single instance throughout the application's lifecycle.</description></item>
        /// </list>
        /// The default value is Transient.
        /// </summary>
        public ServiceLifetime Lifetime { get; private set; }

        /// <summary>
        /// Indicates whether the class should be automatically registered with the dependency injection container.
        /// The default value is true.
        /// </summary>
        public bool AutoRegister { get; private set; }

        /// <summary>
        /// Indicates whether the class itself should also be registered when it implements an interface.
        /// If set to <c>true</c>, the class itself will also be registered;
        /// If set to <c>false</c>, only the interfaces will be registered, not the class itself.
        /// If the class does not implement any interfaces, it will default to registering the class itself.
        /// By default, this property is <c>false</c>, meaning the class will not be registered if it implements an interface.
        /// </summary>
        public bool RegisterSelfIfImpl { get; private set; }

        /// <summary>
        /// A list of service types to register.
        /// </summary>
        public List<Type> RegisterTypes { get; private set; } = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoInjectAttribute"/> class.
        /// </summary>
        /// <param name="lifetime">The service lifetime type. Default value is Transient.</param>
        /// <param name="autoRegister">Indicates whether the class should be automatically registered with the dependency injection container. Default value is true.</param>
        /// <param name="registerSelfIfImpl">Indicates whether the class itself should also be registered when it implements an interface. Default value is false.</param>
        /// <param name="registerTypes">A list of service types.</param>
        public AutoInjectAttribute(
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            bool autoRegister = true,
            bool registerSelfIfImpl = false,
            params Type[] registerTypes)
        {
            Lifetime = lifetime;
            AutoRegister = autoRegister;
            RegisterSelfIfImpl = registerSelfIfImpl;

            // Add specified service types to the registration list
            foreach (var type in registerTypes)
            {
                RegisterTypes.Add(type);
            }
        }
    }
}