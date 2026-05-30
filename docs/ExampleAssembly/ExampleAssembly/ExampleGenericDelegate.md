# ExampleGenericDelegate

A generic delegate.

```csharp
public sealed delegate ExampleGenericDelegate<in T1, in T2, out TResult> : ICloneable, ISerializable where T1 : class, new() where T2 : struct
```

## Members
| name | kind | summary |
| --- | --- | --- |
| [BeginInvoke](./ExampleGenericDelegate/BeginInvoke.md) | method |  |
| [EndInvoke](./ExampleGenericDelegate/EndInvoke.md) | method |  |
| [ExampleGenericDelegate](./ExampleGenericDelegate/ExampleGenericDelegate.md) | constructor |  |
| [Invoke](./ExampleGenericDelegate/Invoke.md) | method |  |
