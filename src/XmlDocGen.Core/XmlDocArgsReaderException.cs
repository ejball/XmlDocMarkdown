namespace XmlDocGen.Core;

/// <summary>An exception thrown for invalid command-line arguments.</summary>
public sealed class XmlDocArgsReaderException(string message) : Exception(message);
