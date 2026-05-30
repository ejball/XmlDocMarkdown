# ExampleDerivedClass

A class that derives from [`ExampleClass`](./ExampleClass.md).

```csharp
public class ExampleDerivedClass : ExampleClass, IExampleContravariantInterface<ExampleClass>, IExampleContravariantInterface<ExampleDerivedClass>, IExampleCovariantInterface<object>, IExampleCovariantInterface<string>, IExampleInterface, IExampleInternalInterface, IEnumerable<object>, IEnumerable<string>, IEnumerable
```

## Members
| name | kind | summary |
| --- | --- | --- |
| [ExampleDerivedClass](./ExampleDerivedClass/ExampleDerivedClass.md) | constructor |  |
| [ExampleMethod](./ExampleDerivedClass/ExampleMethod.md) | method | An implicitly implemented interface method. |
| [GetEnumerator](./ExampleDerivedClass/GetEnumerator.md) | method | The enumerator. |
| [Jump](./ExampleDerivedClass/Jump.md) | method | An overridden method. |
| [SeeAlso](./ExampleDerivedClass/SeeAlso.md) | method | A method with lots of see alsos. |
