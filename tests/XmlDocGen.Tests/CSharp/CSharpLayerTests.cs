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
		var signature = CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!).Text;

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

		Assert.That(CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(overloaded))!).Text, Does.Contain("where T : class where U : struct"));
		Assert.That(CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(defaultParameters))!).Text, Does.Contain("double @double = double.NaN"));
		Assert.That(CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(defaultParameters))!).Text, Does.Contain("ExampleFlagsEnum flags = ExampleFlagsEnum.Second | ExampleFlagsEnum.Third"));
		Assert.That(CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(tryGetValue))!).Text, Does.Contain("out object value"));
	}

	[Test]
	public void SignaturesExposeLinkTargetsOnTypeTokens()
	{
		var tree = TestSupport.CreateExampleTree();
		var method = typeof(ExampleClass).GetMethod(nameof(ExampleClass.Create), Type.EmptyTypes)!;
		var signature = CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(method))!);

		Assert.That(signature.Tokens, Has.Some.Matches<CSharpToken>(x => x.Kind == CSharpTokenKind.TypeName && x.LinkTarget == typeof(ExampleClass).GetTypeInfo()));
	}
}
