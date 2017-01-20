# The Atoms

## Filters

A filter is a middleware component that performs a specific function. A middleware layer is typically a collection of multiple filters, so each filter should adhere to the [single responsibility principle](http://en.wikipedia.org/wiki/Single_responsibility_principle) -- do one thing and one thing only. This fine-grained approach ensures that developers are able to opt-in to each behavior without including unnecessary or unwatched functionality.

The framework includes many filters, most of which are structural or compositional. Developers are also free to create their own filters specific to their application requirements.

To create a filter, create a class that implements `IFilter<T>`.

```csharp
public interface IFilter<T>
    where T : class, PipeContext
{
    Task Send(T context, IPipe<T> next);
}
```

## Pipes

Filters are composed into a _Pipe_ using a builder pattern.

```csharp
var pipe = Pipe.New<PipeContext>(x =>
{   
    x.UseFilter(new CustomFilter(...));
})
```

Contexts are then sent through the pipe using the `IPipe<T>` interface.

```csharp
public interface IPipe<T>
    where T : class, PipeContext
{
    Task Send(T context);
}
```

The send is invoked using the standard async/await features in the C# language.

```csharp
await pipe.Send(new PipeContext(...));
```
