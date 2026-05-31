using System.Reflection;
using ExampleAssembly;
using NUnit.Framework;
using XmlDocGen.Core.CSharp;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Tests.CSharp;

internal sealed class CSharpLayerTests
{
	[Test]
	public void FullTypeSignatureRendersBaseInterfacesWithoutExtraSpaces()
	{
		var tree = TestSupport.CreateExampleTree();
		var signature = CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!).Text;

		Assert.That(signature, Does.StartWith("public class ExampleClass : IExampleContravariantInterface<ExampleClass>, IExampleCovariantInterface<string>"));
		Assert.That(signature, Does.Not.Contain("public  class"));
	}

	[Test]
	public void FullMethodSignatureRendersConstraintsDefaultsAndRefKinds()
	{
		var tree = TestSupport.CreateExampleTree();
		var overloaded = typeof(ExampleClass).GetMethods().Single(x => x.Name == nameof(ExampleClass.Overloaded) && x.GetGenericArguments().Length == 2);
		var defaultParameters = typeof(ExampleClass).GetMethod(nameof(ExampleClass.DefaultParameters))!;
		var tryGetValue = typeof(ExampleClass).GetMethods().Single(x => x.Name == nameof(ExampleClass.TryGetValue) && !x.IsGenericMethod);

		Assert.That(CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForMember(overloaded))!).Text, Does.Contain("where T : class where U : struct"));
		Assert.That(CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForMember(defaultParameters))!).Text, Does.Contain("double @double = double.NaN"));
		Assert.That(CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForMember(defaultParameters))!).Text, Does.Contain("ExampleFlagsEnum flags = ExampleFlagsEnum.Second | ExampleFlagsEnum.Third"));
		Assert.That(CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForMember(tryGetValue))!).Text, Does.Contain("out object value"));
	}

	[Test]
	public void SignaturesExposeLinkTargetsOnTypeTokens()
	{
		var tree = TestSupport.CreateExampleTree();
		var method = typeof(ExampleClass).GetMethod(nameof(ExampleClass.Create), Type.EmptyTypes)!;
		var signature = CSharpSignature.CreateFull(tree.FindNode(XmlDocRef.ForMember(method))!);

		Assert.That(signature.Tokens, Has.Some.Matches<CSharpToken>(x => x.Kind == CSharpTokenKind.TypeName && x.LinkTarget == typeof(ExampleClass).GetTypeInfo()));
	}

	[Test]
	public void FullSignaturesRenderModernCSharpSyntax()
	{
		var tree = TestSupport.CreateExampleTree();
		var modernType = typeof(ExampleModernSyntax);

		Assert.That(GetSignature(tree, typeof(ExampleReadOnlyRefStruct)), Is.EqualTo("public readonly ref struct ExampleReadOnlyRefStruct"));
		Assert.That(GetSignature(tree, typeof(IExampleStaticAbstractInterface<>).GetMethod("Create")!), Is.EqualTo("public static abstract TSelf Create()"));
		Assert.That(GetSignature(tree, typeof(IExampleStaticAbstractInterface<>).GetMethod("Identity")!), Is.EqualTo("public static virtual TSelf Identity(TSelf value)"));
		Assert.That(GetSignature(tree, modernType.GetProperty(nameof(ExampleModernSyntax.NullableText))!), Is.EqualTo("public string? NullableText { get; set; }"));
		Assert.That(GetSignature(tree, modernType.GetProperty(nameof(ExampleModernSyntax.RefReadonlyValue))!), Is.EqualTo("public ref readonly int RefReadonlyValue { get; }"));
		Assert.That(GetSignature(tree, modernType.GetMethod(nameof(ExampleModernSyntax.GetFunctionPointer))!), Is.EqualTo("public delegate*<int, int> GetFunctionPointer(delegate*<int, int> callback)"));
		Assert.That(GetSignature(tree, modernType.GetMethod(nameof(ExampleModernSyntax.UpdateScoped))!), Is.EqualTo("public void UpdateScoped(scoped ref int value)"));
		Assert.That(GetSignature(tree, modernType.GetMethod(nameof(ExampleModernSyntax.ReadRefReadonly))!), Is.EqualTo("public void ReadRefReadonly(ref readonly int value)"));
		Assert.That(GetSignature(tree, modernType.GetMethod(nameof(ExampleModernSyntax.TupleNames))!), Is.EqualTo("public (int Count, string? Name) TupleNames((nint Index, nuint Length) input)"));
		Assert.That(GetSignature(tree, modernType.GetMethod(nameof(ExampleModernSyntax.Constrained))!), Does.Contain("where TNotNull : notnull where TUnmanaged : unmanaged where TClass : class?"));
		Assert.That(GetSignature(tree, modernType.GetMethods().Single(x => x.Name == "op_CheckedAddition")), Does.Contain("operator checked +"));
		Assert.That(GetSignature(tree, modernType.GetMethods().Single(x => x.Name == "op_UnsignedRightShift")), Does.Contain("operator >>>"));
	}

	private static string GetSignature(Core.Nodes.XmlDocTree tree, MemberInfo member) => CSharpSignature.CreateFull(tree.FindNode(member)!).Text;
}
