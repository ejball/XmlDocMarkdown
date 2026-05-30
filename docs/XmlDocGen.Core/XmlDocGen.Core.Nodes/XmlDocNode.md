# XmlDocNode

Base class for an assembly, namespace, type, or member documentation node.

```csharp
public abstract class XmlDocNode
```

## Members
| name | kind | summary |
| --- | --- | --- |
| [Assembly](./XmlDocNode/Assembly.md) | property | Gets the owning assembly node. |
| [Children](./XmlDocNode/Children.md) | property | Gets child nodes. |
| [DescendantsAndSelf](./XmlDocNode/DescendantsAndSelf.md) | method | Enumerates this node and every descendant. |
| [DescendantsAndSelf](./XmlDocNode/DescendantsAndSelf-XmlDocNodeVisibility.md) | method | Enumerates this node and visible descendants. |
| [GetChildren](./XmlDocNode/GetChildren.md) | method | Gets visible immediate children. |
| [IsBrowsable](./XmlDocNode/IsBrowsable.md) | property | Gets a value indicating whether this node is browsable. |
| [IsCompilerGenerated](./XmlDocNode/IsCompilerGenerated.md) | property | Gets a value indicating whether this node is compiler-generated. |
| [IsObsolete](./XmlDocNode/IsObsolete.md) | property | Gets a value indicating whether this node is obsolete. |
| [MemberInfo](./XmlDocNode/MemberInfo.md) | property | Gets the reflected member associated with this node. |
| [Name](./XmlDocNode/Name.md) | property | Gets the simple display name. |
| [Parent](./XmlDocNode/Parent.md) | property | Gets the parent node, or null for an assembly root. |
| [Ref](./XmlDocNode/Ref.md) | property | Gets this node&amp;#39;s XML documentation reference. |
| [TryGetAttribute](./XmlDocNode/TryGetAttribute.md) | method | Surfaces an attribute applied to this node, if present. |
| [Visibility](./XmlDocNode/Visibility.md) | property | Gets the exact visibility of this node. |
| [XmlMember](./XmlDocNode/XmlMember.md) | property | Gets the associated XML documentation, if any. |
