# ExampleTriGenericClass

A generic class with three generic type parameters.

```csharp
public class ExampleTriGenericClass<TOne, TTwo, TThree> where TTwo : struct, IEnumerable<string> where TThree : class, TOne, IEnumerable<TTwo?>
```

## Members
| name | kind | summary |
| --- | --- | --- |
| [ExampleTriGenericClass](./ExampleTriGenericClass/ExampleTriGenericClass.md) | constructor |  |
