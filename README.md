# Deprecated

> **This library is retired.** It is obsolete, and nothing in .NET replaces it directly.
>
> - It scanned types across `AppDomain`s, private-bin folders and assemblies, which .NET Core and later do not have. Use `AssemblyLoadContext` and `Assembly.GetTypes()` where you need to inspect assemblies, and a scanning library such as Scrutor to register types with `Microsoft.Extensions.DependencyInjection`.
>
> It has not changed since 2015.
>
> The NuGet packages are deprecated (`NCode.Scanners`, `NCode.Scanners.Cecil` and `NCode.Scanners.Reflection`) and this repository is archived. The code stays available as it is.

# NCode.Scanners
This library provides a fluent API to search, filter, transform, and cache .NET types by probing applications (using private-bin folders), directories, files, and AppDomains, and assemblies.

## NCode.Scanners.Cecil
This library provides additional scanners for 'NCode.Scanners' that uses 'Cecil' to inspect assemblies and types without loading them into the current AppDomain.

## NCode.Scanners.Reflection
This library provides additional scanners for 'NCode.Scanners' that uses 'Reflection' to inspect assemblies and types which causes them to the loaded into the current AppDomain.

## Documentation
See the [wiki][wiki] for usage and reference documentation.

## Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][issues].

[wiki]: https://github.com/NCodeGroup/NCode.Scanners/wiki
[issues]:https://github.com/NCodeGroup/NCode.Scanners/issues 
