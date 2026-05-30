# ExampleRecord

A C# 9 generic record.

```csharp
public ExampleRecord(string Name, int Age, HashSet<DayOfWeek> Days, T GenericType, Action<T> GenericLambda)
```

| parameter | description |
| --- | --- |
| T | Some generic type. |
| Name | A string. |
| Age | An integer. |
| Days | A hash set. |
| GenericType | A generic type parameter. |
| GenericLambda | An lambda parameter. |
