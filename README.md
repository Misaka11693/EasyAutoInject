
# EasyAutoInject

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/Misaka11693/EasyAutoInject/workflows/build) [![NuGet Version](https://img.shields.io/nuget/v/EasyAutoInject.svg)](https://www.nuget.org/packages/EasyAutoInject/) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

## Introduction

EasyAutoInject is a lightweight library designed to simplify dependency injection (DI) configuration in .NET applications. It provides an attribute-based method that can automatically register services into the DI container based on attributes applied to classes.

### Key Features

- **Automatic Registration**:
  - Automatically registers classes into the DI container without explicitly calling methods like `AddTransient`, `AddScoped`, or `AddSingleton`.

- **Customizable Lifetimes**:
  - Supports specifying service lifetimes including transient (`Transient`), scoped (`Scoped`), and singleton (`Singleton`).

- **Interface Registration**:
  - Optionally registers the class itself if it implements interfaces.

- **Specific Type Registration**:
  - Can specify particular types for registration, such as registering the class itself or other implemented interfaces.

- **Assembly-Specific Injection**:
  - Can specify particular assemblies for dependency injection to avoid unnecessary global scanning.

- **Optional Self-Registration**:
  - Can choose whether to register the class itself in the DI container even if it does not implement any interfaces. If a class implements interfaces, by default only the interfaces are registered unless `RegisterSelfIfImpl` is set to `true`.

## Use Cases

EasyAutoInject is suitable for the following scenarios:

- Need to quickly set up dependency injection within a project.
- Want to reduce the manual configuration work for the DI container.
- Require flexible control over service lifetimes and registration methods.

## Installation

### Installing via NuGet Package Manager:

```powershell
Install-Package EasyAutoInject
```

### Adding Package Reference:

```xml
<ItemGroup>
    <PackageReference Include="EasyAutoInject" Version="*" />
</ItemGroup>
```

### Installing via .NET CLI:

```bash
dotnet add package EasyAutoInject
```

## Usage

### Using `AutoInject()` in `Program.cs`

```csharp
var services = new ServiceCollection();
services.AutoInject(); // Registers all services marked with AutoInject
services.AutoInject("MyService.dll"); // Registers all services marked with AutoInject in the specified DLL
services.AutoInject("MyService.dll", "AnotherService.dll"); // Registers all services marked with AutoInject in multiple DLLs
```

### Specifying Interface Suffix Filtering

```csharp
services.AutoInject(interfaceSuffix: "Service"); // Registers all services marked with AutoInject whose interfaces have the suffix "Service"
```

### Example Code

#### Example 1: Interface Definitions

```csharp
namespace MyProject;

// Interface definitions
public interface IEmailService { }
public interface IUserManager { }
```

#### Example 2: Implementation Class

```csharp
namespace MyProject;

[AutoInject(Lifetime = ServiceLifetime.Scoped)]
public class MyService : IEmailService, IUserManager { }
```

#### Example 3: Registering Services in `Program.cs`

In your application's startup file, you can use the `AutoInject` method and specify a particular interface suffix to register these services.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using MyProject; // Reference your project namespace

public class Program
{
    public static void Main(string[] args)
    {
       ...
       Build.Services.AutoInject(interfaceSuffix: "Service");
    }
}
```

### Service Registration Effect

Using `interfaceSuffix: "Service"` will result in the registration of all services whose interface names end with "Service", specifically:

```csharp
services.AddScoped<IEmailService, MyService>();
```

Note that the `IUserManager` interface will not be registered because it does not match the specified suffix "Service".

### Additional Examples

#### Example 4: For Non-Interface Implementing Classes

```csharp
namespace MyProject;

[AutoInject]
public class Foo { }
```

This configuration will cause the `Foo` class to be automatically registered as a transient lifetime service, as shown below:

```csharp
services.AddTransient<Foo>();
```

#### Example 5: For Classes Implementing Interfaces

```csharp
namespace MyProject;

[AutoInject(Lifetime = ServiceLifetime.Scoped, RegisterSelfIfImpl = true)]
public class Foo : IFoo { }
```

This configuration will cause the `Foo` class to be automatically registered as a scoped lifetime service, and the `IFoo` interface will also be registered to point to the implementation of `Foo`, as shown below:

```csharp
services.AddScoped<IFoo, Foo>();
services.AddScoped<Foo>();
```

If `RegisterSelfIfImpl` is not specified, it defaults to `false`:

```csharp
namespace MyProject;

[AutoInject(Lifetime = ServiceLifetime.Scoped)]
public class Foo : IFoo { }
```

Then only the `IFoo` interface will be registered:

```csharp
services.AddScoped<IFoo, Foo>();
```

#### Example 6: For Classes Implementing Multiple Interfaces

```csharp
namespace MyProject;

[AutoInject(Lifetime = ServiceLifetime.Scoped, RegisterSelfIfImpl = true)]
public class Foo : IBar, IFoo { }
```

This configuration will cause the `Foo` class to be automatically registered as a scoped lifetime service, and both the `IBar` and `IFoo` interfaces will be registered to point to the implementation of `Foo`, as shown below:

```csharp
services.AddScoped<IBar, Foo>();
services.AddScoped<IFoo, Foo>();
services.AddScoped<Foo>();
```

If `RegisterSelfIfImpl` is not specified, it defaults to `false`:

```csharp
namespace MyProject;

[AutoInject(Lifetime = ServiceLifetime.Scoped)]
public class Foo : IBar, IFoo { }
```

Then only the `IBar` and `IFoo` interfaces will be registered:

```csharp
services.AddScoped<IBar, Foo>();
services.AddScoped<IFoo, Foo>();
```

#### Example 7: For Specifying Registration Types

```csharp
namespace MyProject;

[AutoInject(typeof(IBaz), RegisterSelfIfImpl = true)]
public class Bar : IBaz { }
```

This configuration will cause the `Bar` class to be registered as an implementation of the `IBaz` interface, and if `RegisterSelfIfImpl` is set to `true`, it will also register the `Bar` class itself, as shown below:

```csharp
services.AddTransient<IBaz, Bar>();
services.AddTransient<Bar>();
```

If `RegisterSelfIfImpl` is not specified, it defaults to `false`:

```csharp
namespace MyProject;

[AutoInject(typeof(IBaz))]
public class Bar : IBaz { }
```

Then only the `IBaz` interface will be registered:

```csharp
services.AddTransient<IBaz, Bar>();
```

### Example: Mismatch Between `typeof` and Implemented Interfaces

Assume we have a class implementing multiple interfaces, but in the `AutoInjectAttribute` only some interfaces are specified, or some non-implemented interfaces are specified.

#### Example 8: Multiple Interface Implementations, Partially Specified

```csharp
namespace MyProject;

public interface IFoo { }
public interface IBar { }
public interface IBaz { }

[AutoInject(typeof(IFoo), typeof(IBar))]
public class FooBarBaz : IFoo, IBar, IBaz { }
```

This configuration will cause the `FooBarBaz` class to be automatically registered as a transient lifetime service, and the `IFoo` and `IBar` interfaces will be registered to point to the implementation of `FooBarBaz`, as shown below:

```csharp
services.AddTransient<IFoo, FooBarBaz>();
services.AddTransient<IBar, FooBarBaz>();
```

#### Example 9: Multiple Interface Implementations, Specifying a Non-Implemented Interface

```csharp
namespace MyProject;

public interface IFoo { }
public interface IBar { }
public interface IBaz { }

[AutoInject(typeof(IFoo), typeof(IBaz))]
public class FooBar : IFoo, IBar { }
```

Since `FooBar` does not implement `IBaz`, this will result in a runtime exception:

```csharp
InvalidOperationException: In the AutoInjectAttribute of type MyProject.FooBar, the specified RegisterType(IBaz) must be the type itself or an implemented interface.
```

## Community Contributions

We welcome community members to submit Pull Requests or Issues suggesting improvements or reporting bugs for EasyAutoInject. Your feedback and support will help in the continuous improvement and development of EasyAutoInject. If you have any questions or need further assistance, please feel free to contact the project maintainers.

Thank you for using EasyAutoInject!
