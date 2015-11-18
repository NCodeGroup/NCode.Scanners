# NCode.Scanners
This library provides a fluent API to search, filter, transform, and cache .NET types by probing applications (using private-bin folders), directories, files, and AppDomains, and assemblies.

* [Scanner Interfaces](#Scanner Interfaces)
* [Scanner Factory](#Scanner Factory)
* [Scanner Context](#Scanner Context)
* [Aggregate Scanner](#Aggregate Scanner)
* [Cache Scanner](#Cache Scanner)

## Scanner Interfaces
Provides a fluent interface that is used to scan for items of a certain type.

```cs
public interface IScanner<out T> : ISupportFactory, INotifyPropertyChanged, INotifyCollectionChanged, IFluentInterface
{
  IEnumerable<T> Scan(IScanContext context);
}
```

An abstract <code>Scanner&lt;T&gt;</code> class is also available that provides some default implementation of the event handlers and the <code>ISupportFactory</code> interface.

## Scanner Factory
The <code>IScannerFactory</code> interface itself only contains 2 members. Its purpose is to allow others to provide their own implementations of scanners by using extension methods on the <code>IScannerFactory</code> interface with various factory methods.

```cs
public interface IScannerFactory : ISupportFactory
{
  event ContextCreatedHandler ContextCreated;
  IScanContext CreateContext();
}

public interface ISupportFactory
{
  IScannerFactory Factory { get; }
}

public static partial class ScannerFactory
{
  public static IScannerFactory Create() { /* ... */ }
  public static IScanner<T> Empty<T>(this IScannerFactory factory) { /* ... */ }
  // ...
}
```

## Scan Context

Provides the ability to specify additional options to the <code>Scan</code> method which various <code>IScanner&lt;T&gt;</code> implementations can use to alter their behavior at runtime.

```cs
public interface IScanOption
{
  // nothing
}
public interface IScanContext : ISupportFactory
{
  KeyedByTypeCollection<IScanOption> Options { get; }
}
```

### Example with Context and Option
The following is a crued example of an <code>IScanOption</code> that stores a <code>TypeName</code> which is used by a custom scanner to load and return a <code>Type</code> by name.

```cs
public class ExampleOption : IScanOption
{
  string TypeName { get; set; }
}
public class ExampleScanner : Scanner<Type>
{
  public IEnumerable<T> Scan(IScanContext context)
  {
    var typeName = context.Options.Find<ExampleOption>().TypeName
    // ...
    return Type.GetType(typeName);
  }
}
public static class Program
{
  private static void Main()
  {
    var factory = ScannerFactory.Create();
    var context = factory.CreateContext();

    var option = new ExampleOption { TypeName = typeof(Program).AssemblyQualifiedName };
    context.Options.Add(option);

    var scanner = new ExampleScanner();
    var types = scanner.Scan(context);
    // ...
  }
}
```

## Aggregate Scanner
Provides an <code>IScanner&lt;T&gt;</code> that combines a collection of <code>IScanner&lt;T&gt;</code> objects. This scanner will propagate any events from it's collection of scanners.

```cs
public interface IAggregateScanner<T> : IScanner<T>
{
  ICollection<IScanner<T>> Scanners { get; }
}

public static partial class ScannerFactory
{
  public static IAggregateScanner<T> Aggregate<T>(this IScannerFactory factory, params IScanner<T>[] source) { /* ... */ }
  public static IAggregateScanner<T> Aggregate<T>(this IScannerFactory factory, IEnumerable<IScanner<T>> source) { /* ... */ }
}

public static partial class ScannerExtensions
{
  public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, params IScanner<T>[] scanners) { /* ... */ }
  public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, IEnumerable<IScanner<T>> scanners) { /* ... */ }
}
```

## Cache Scanner
Provides an <code>IScanner&lt;T&gt;</code> that automatically caches items from another scanner. In addition to propagating the events from it's parent, this scanner will also cache the items from it's parent and continue to return those items until the cache is invalidated. The cache of items will be invalidated automatically when the parent raises it's <code>CollectionChanged</code> event or when the <code>Invalidate</code> method is called.

```cs
public interface ICacheScanner<out T> : IScanner<T>, IUseParentScanner<T>
{
  void Invalidate();
}

public static partial class ScannerExtensions
{
  public static ICacheScanner<T> Cache<T>(this IScanner<T> source) { /* ... */ }
}
```

# NCode.Scanners.Cecil
This library provides additional scanners for 'NCode.Scanners' that uses 'Cecil' to inspect assemblies and types without loading them into the current AppDomain.

# NCode.Scanners.Reflection
This library provides additional scanners for 'NCode.Scanners' that uses 'Reflection' to inspect assemblies and types which causes them to the loaded into the current AppDomain.

## Examples
  private static void UsingCecil()
  {
    var factory = ScannerFactory.Create();

    var items = factory
      .FilesInLocalPath()   // returns IScanner<FileInfo>
      .ReadAssembly()       // returns IScanner<AssemblyDefinition>
      .GetDefinedTypes()    // returns IScanner<TypeDefiniton>
      .IsDefined((GuidAttribute attr) => attr.Value == "{00000000-0000-0000-C000-000000000046}")
      .Scan();              // returns IEnumerable<TypeDefiniton>

    // ...
    DisplayItems(items);
  }

  private static void UsingReflection()
  {
    var factory = ScannerFactory.Create();

    var items = factory
      .FilesInLocalPath()   // returns IScanner<FileInfo>
      .GetAssemblyName()    // returns IScanner<AssemblyName>
      .LoadAssembly()       // returns IScanner<Assembly>
      .GetDefinedTypes()    // returns IScanner<TypeInfo>
      .IsDefined((GuidAttribute attr) => attr.Value == "{00000000-0000-0000-C000-000000000046}", false)
      .Scan();              // returns IEnumerable<TypeInfo>

    // ...
    DisplayItems(items);
  }

  private static void SystemFilesFromGac()
  {
    var factory = ScannerFactory.Create();

    var dirs = new[] { @"C:\Windows\assembly\GAC_MSIL", @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL" };
    var items = factory
      .FilesInDirectory(dirs, SearchOption.AllDirectories)
      .Include(file => file.Name.StartsWith("System."))
      .Scan();

    // ...
    DisplayItems(items);
  }

  private static void DisplayItems(IEnumerable<object> items)
  {
    var counter = 0;
    foreach (var item in items)
    {
      Console.WriteLine("[{0}] {1}", ++counter, item);
    }
  }

## Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][1].

[1]: https://github.com/NCodeGroup/NCode.Scanners/issues
