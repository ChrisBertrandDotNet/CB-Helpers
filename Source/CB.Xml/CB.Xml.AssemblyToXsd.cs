
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Extracts a XSD schema from the types an Assembly file (.exe or .dll) declares.
*/

// TODO: clean and restructure the whole file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CB.Xml
{

	/// <summary>
	/// Extracts a XSD schema from the types an Assembly file (.exe or .dll) declares.
	/// </summary>
	public class AssemblyToXsd
	{
		/// <summary>
		/// Exports the XSD schema of every assembly listed in <paramref name="assemblies"/>.
		/// A schema is a representation of all types this assembly declares.
		/// </summary>
		/// <param name="assemblies">The assemblies from which types will be extracted.</param>
		/// <param name="outputDir">Exported XSD files will be written here.</param>
		/// <param name="outputMessages"></param>
		/// <param name="typeFilter">Lets you filter the type to add to the schema. If null, all types are added.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Error loading an assembly.</exception>
		public static bool ExportTypesAsSchemas(IList<string> assemblies, string outputDir, ICollection<string> outputMessages, Func<Type, bool> typeFilter = null)
		{
			var exporter = new _AssemblyToXsd();
			return exporter.ExportTypesAsSchemas(assemblies, outputDir, outputMessages, typeFilter);
		}

		/// <summary>
		/// Extracts a XSD schema from the types an Assembly file (.exe or .dll) declares.
		/// </summary>
		class _AssemblyToXsd
		{
			const string NoTypesGenerated = "No type generated.";
			const string ErrGeneral = "Error ({0})";
			const string InfoWrittingFile = "file written: {0}.";
			const string SchemaValidationWarningDetails = "warning validating schema: {0}";
			const string SchemaValidationWarningDetailsSource = "warning validating schema on line {1}	position {2}: {0}";
			const string ErrLoadAssembly = "Error loading assemble file {0}";

			bool _schemaCompileErrors;
			ICollection<string> _outputMessages;

			/// <summary>
			/// Exports the XSD schema of every assembly listed in <paramref name="assemblies"/>.
			/// A schema is a representation of all types this assembly declares.
			/// </summary>
			/// <param name="assemblies">The assemblies from which types will be extracted.</param>
			/// <param name="outputDir">Exported XSD files will be written here.</param>
			/// <param name="outputMessages"></param>
			/// <param name="typeFilter">Lets you filter the type to add to the schema. If null, all types are added.</param>
			/// <returns></returns>
			/// <exception cref="System.InvalidOperationException">Error loading an assembly.</exception>
			public bool ExportTypesAsSchemas(IList<string> assemblies, string outputDir, ICollection<string> outputMessages, Func<Type, bool> typeFilter = null)
			{
				this._outputMessages = outputMessages;

				XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter();
				XmlSchemas xmlSchemas = new XmlSchemas();
				XmlSchemaExporter xmlSchemaExporter = new XmlSchemaExporter(xmlSchemas);
				foreach (string assemblyLocation in assemblies)
				{
					Assembly assembly = Assembly.LoadFrom(assemblyLocation);
					if (assembly == null)
					{
						throw new InvalidOperationException(_AssemblyToXsd.GetString(ErrLoadAssembly, new object[]
				{
				assemblyLocation
				}));
					}
					try
					{
						Type[] types = assembly.GetTypes();
						for (int i = 0; i < types.Length; i++)
						{
							Type type = types[i];
							if (type.IsPublic && (!type.IsAbstract || !type.IsSealed) && !type.IsInterface && !type.ContainsGenericParameters)
							{
								bool addThisType = typeFilter != null ? typeFilter(type) : true;
								if (addThisType)
								{
									XmlTypeMapping xmlTypeMapping = xmlReflectionImporter.ImportTypeMapping(type);
									xmlSchemaExporter.ExportTypeMapping(xmlTypeMapping);
								}
							}
						}
						xmlSchemas.Compile(new ValidationEventHandler(this.ValidationCallbackWithErrorCode), false);
					}
					catch (Exception ex)
					{
						if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
						{
							throw;
						}
						throw new InvalidOperationException(_AssemblyToXsd.GetString(ErrGeneral, new object[]
				{
				assemblyLocation
				}), ex);
					}
				}
				for (int j = 0; j < xmlSchemas.Count; j++)
				{
					XmlSchema xmlSchema = xmlSchemas[j];
					try
					{
						TextWriter textWriter = this.CreateOutputWriter(outputDir, "schema" + j.ToString(), ".xsd");
						xmlSchemas[j].Write(textWriter);
						textWriter.Close();
					}
					catch (Exception ex2)
					{
						if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
						{
							throw;
						}
						throw new InvalidOperationException(_AssemblyToXsd.GetString(ErrGeneral, new object[]
				{
				xmlSchema.TargetNamespace
				}), ex2);
					}
				}
				if (xmlSchemas.Count == 0)
				{
					_outputMessages.Add(_AssemblyToXsd.GetString(NoTypesGenerated));
				}
				return !this._schemaCompileErrors;
			}

			private TextWriter CreateOutputWriter(string outputdir, string fileName, string newExtension)
			{
				string path = Path.ChangeExtension(Path.GetFileName(fileName), newExtension);
				string text = Path.Combine(outputdir, path);
				_outputMessages.Add(_AssemblyToXsd.GetString(InfoWrittingFile, new object[]
		{
		text
		}));
				return new StreamWriter(text, false, new UTF8Encoding(true));
			}

			internal void ValidationCallbackWithErrorCode(object sender, ValidationEventArgs args)
			{
				string @string;
				if (args.Exception.LineNumber == 0 && args.Exception.LinePosition == 0)
				{
					@string = _AssemblyToXsd.GetString(SchemaValidationWarningDetails, new object[]
					{
					args.Message
					});
				}
				else
				{
					@string = _AssemblyToXsd.GetString(SchemaValidationWarningDetailsSource, new object[]
					{
					args.Message,
					args.Exception.LineNumber.ToString(CultureInfo.InvariantCulture),
					args.Exception.LinePosition.ToString(CultureInfo.InvariantCulture)
					});
				}
				if (args.Severity == XmlSeverityType.Error)
				{
					_outputMessages.Add(@string);
					this._schemaCompileErrors = true;
				}
			}

			static string GetString(string message, params object[] parameters)
			{
				if (parameters.Length == 0)
					return message;
				return string.Format(message, parameters);
			}
		}
	}
}