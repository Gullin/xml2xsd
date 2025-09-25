using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        string mode = args[0].ToLowerInvariant();

        try
        {
            if (mode == "--xsd")
            {
                RunXsdGeneration(args.Skip(1).ToArray());
            }
            else if (mode == "--xmltemplate")
            {
                RunXmlTemplateGeneration(args.Skip(1).ToArray());
            }
            else
            {
                Console.WriteLine($"❌ Okänd växel: {mode}");
                PrintUsage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Fel: " + ex.Message);
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Användning:");
        Console.WriteLine("  dotnet run --xsd <fil1.xml> <katalog> ...   -> Generera XSD från XML");
        Console.WriteLine("  dotnet run --xmltemplate <fil.xsd>          -> Generera XML-instansmall från XSD");
        Console.WriteLine();
    }

    // --- LÄGE 1: Generera XSD från XML ---
    static void RunXsdGeneration(string[] inputs)
    {
        var xmlFiles = ResolveXmlFiles(inputs);

        if (!xmlFiles.Any())
        {
            Console.WriteLine("Inga XML-filer hittades.");
            return;
        }

        Console.WriteLine("Genererar XSD från följande XML-filer:");
        foreach (var f in xmlFiles) Console.WriteLine(" - " + f);

        XmlSchemaInference inference = new XmlSchemaInference();
        XmlSchemaSet schemaSet = null;

        foreach (string xmlFile in xmlFiles)
        {
            using var reader = XmlReader.Create(xmlFile);
            schemaSet = schemaSet == null
                ? inference.InferSchema(reader)
                : inference.InferSchema(reader, schemaSet);
        }

        string outputFile = Path.Combine(Environment.CurrentDirectory, "combined_schema.xsd");

        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            using var writer = XmlWriter.Create(outputFile, new XmlWriterSettings { Indent = true });
            schema.Write(writer);
        }

        Console.WriteLine($"\n✅ XSD-schema genererat: {outputFile}");
    }

    // --- LÄGE 2: Generera XML-mall från XSD ---
    static void RunXmlTemplateGeneration(string[] inputs)
    {
        if (inputs.Length != 1)
        {
            Console.WriteLine("❌ Ange exakt en XSD-fil för att skapa XML-mall.");
            return;
        }

        string xsdPath = Path.GetFullPath(inputs[0]);
        if (!File.Exists(xsdPath))
        {
            Console.WriteLine($"❌ Hittade inte XSD-fil: {xsdPath}");
            return;
        }

        string outputXml = Path.Combine(Environment.CurrentDirectory,
            Path.GetFileNameWithoutExtension(xsdPath) + "_template.xml");

        using var reader = XmlReader.Create(xsdPath);
        var schema = XmlSchema.Read(reader, (s, e) => Console.WriteLine(e.Message));
        schema.Compile(null);

        using var writer = XmlWriter.Create(outputXml, new XmlWriterSettings { Indent = true });
        writer.WriteStartDocument();

        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            WriteElement(writer, element, schema, indent: 0);
        }

        writer.WriteEndDocument();
        Console.WriteLine($"\n✅ XML-mall genererad: {outputXml}");
    }

    // Hjälpmetod för att hitta XML-filer från filer/kataloger
    static IEnumerable<string> ResolveXmlFiles(string[] inputs)
    {
        List<string> files = new();

        foreach (var input in inputs)
        {
            string path = Path.GetFullPath(input);

            if (File.Exists(path) && path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(path);
            }
            else if (Directory.Exists(path))
            {
                files.AddRange(Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories));
            }
            else
            {
                Console.WriteLine($"⚠️ Hoppade över: {input} (ingen giltig fil eller katalog)");
            }
        }

        return files.Distinct().OrderBy(f => f).ToList();
    }

    // Skriver ut element med kommentarer
    static void WriteElement(XmlWriter writer, XmlSchemaElement element, XmlSchema schema, int indent)
    {
        string occursInfo = GetOccursInfo(element);

        writer.WriteWhitespace(Environment.NewLine + new string(' ', indent));
        writer.WriteComment($" Element: <{element.Name}> {occursInfo} ");
        writer.WriteWhitespace(Environment.NewLine + new string(' ', indent));
        writer.WriteStartElement(element.Name);

        if (element.ElementSchemaType is XmlSchemaComplexType complexType)
        {
            // Attribut
            if (complexType.AttributeUses.Count > 0)
            {
                foreach (XmlSchemaAttribute attr in complexType.AttributeUses.Values)
                {
                    string attrInfo = attr.Use == XmlSchemaUse.Required ? "obligatoriskt" : "valfritt";
                    writer.WriteWhitespace(Environment.NewLine + new string(' ', indent + 2));
                    writer.WriteComment($" Attribut: {attr.Name} ({attrInfo}) ");
                    writer.WriteAttributeString(attr.Name, "");
                }
            }

            // Barn
            if (complexType.ContentTypeParticle is XmlSchemaSequence sequence)
            {
                foreach (XmlSchemaObject item in sequence.Items)
                {
                    if (item is XmlSchemaElement child)
                    {
                        WriteElement(writer, child, schema, indent + 2);
                    }
                }
            }
        }
        else
        {
            writer.WriteString("");
        }

        writer.WriteEndElement();
    }

    static string GetOccursInfo(XmlSchemaElement element)
    {
        string required = element.MinOccurs == 0 ? "valfritt" : "obligatoriskt";
        string multiple = element.MaxOccursString == "unbounded"
            ? "kan förekomma obegränsat antal gånger"
            : element.MaxOccurs > 1 ? $"kan förekomma upp till {element.MaxOccurs} gånger"
            : "förekommer max en gång";

        return $"({required}, {multiple})";
    }
}
