﻿// <copyright file="PackageDefinitionFile.cs" company="Mark Final">
//  Opus.Core
// </copyright>
// <summary>Opus package definition XML file</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    internal class OpusXmlResolver : System.Xml.XmlResolver
    {
        internal OpusXmlResolver()
            : base()
        {
        }

        public override System.Net.ICredentials Credentials
        {
            set { throw new System.NotImplementedException(); }
        }

        public override System.Uri ResolveUri(System.Uri baseUri, string relativeUri)
        {
            if (baseUri == null)
            {
                return base.ResolveUri(baseUri, relativeUri);
            }

            if (!relativeUri.EndsWith(".xsd"))
            {
                throw new System.Xml.XmlException(System.String.Format("Don't know how to resolve URIs such as '{0}'", relativeUri));
            }

            // NEW style definition files (0.50 onward)
            if (relativeUri == State.OpusPackageDependencySchemaRelativePathNameV2)
            {
                // we've got a local match, so use the version of the Schema that is next to the Opus binary
                return new System.Uri(State.OpusPackageDependencySchemaPathNameV2);
            }
            else
            {
                // OLD style definition files (pre 0.50)
                if (System.IO.File.Exists(relativeUri))
                {
                    // absolute pathname to a schema that exists on disk!
                    return base.ResolveUri(baseUri, relativeUri);
                }
                else
                {
                    // absolute pathname to a schema that doesn't exist on disk!
                    throw new System.Xml.Schema.XmlSchemaException(System.String.Format("Schema '{0}' cannot be located. Please re-run the Opus command and force a definition file update", relativeUri));
                }
            }
        }

        public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn)
        {
            if (absoluteUri.IsFile)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(absoluteUri.LocalPath);
                return reader.BaseStream;
            }
            else
            {
                if ("http://code.google.com/p/opus" == absoluteUri.ToString())
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(State.OpusPackageDependencySchemaPathNameV2);
                    return reader.BaseStream;
                }

                throw new System.Xml.XmlException(System.String.Format("Did not understand non-file URI '{0}'", absoluteUri.ToString()));
            }
        }
    }

    public class DotNetAssemblyDescription
    {
        public DotNetAssemblyDescription(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public string RequiredTargetFramework
        {
            get;
            set;
        }
    }

    public class PackageDefinitionFile
    {
        private string xmlFilename;
        private bool validate;
        
        private static void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            if (args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
            {
                Log.DebugMessage("Warning: Matching schema not found (" + args.Exception.GetType().ToString() + "). " + args.Message);
            }
            else
            {
                // NEW style definition files
                if (args.Exception is System.Xml.Schema.XmlSchemaException)
                {
                    throw new Exception("From {0} (line {1}, position {2}):\n{3}", args.Exception.SourceUri, args.Exception.LineNumber, args.Exception.LinePosition, args.Exception.Message);
                }

                // OLD style definition files
                if (args.Exception.SourceUri.Contains(".xsd"))
                {
                    System.Uri schemaUri = new System.Uri(State.OpusPackageDependencySchemaPathNameV2);
                    if (schemaUri.AbsolutePath != args.Exception.SourceUri)
                    {
                        throw new Exception("Duplicate schemas exist! From package definition file '{0}', and from the Opus build '{1}'", args.Exception.SourceUri, schemaUri.AbsolutePath);
                    }
                }

                string message = System.String.Format("Validation error: " + args.Message);
                message += System.String.Format("\nAt '" + args.Exception.SourceUri + "', line " + args.Exception.LineNumber + ", position " + args.Exception.LinePosition);

                throw new System.Xml.XmlException(message);
            }
        }
        
        private void Validate()
        {
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
            settings.ValidationType = System.Xml.ValidationType.Schema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationCallBack);
            settings.XmlResolver = new OpusXmlResolver();

            // Create the XmlReader object.
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(this.xmlFilename, settings);
    
            // Parse the file. 
            while (reader.Read());
        }
        
        public PackageDefinitionFile(string xmlFilename, bool validate)
        {
            this.validate = validate;
            this.xmlFilename = xmlFilename;
            this.PackageIdentifiers = new PackageIdentifierCollection();
            this.OpusAssemblies = new StringArray();
            this.DotNetAssemblies = new Array<DotNetAssemblyDescription>();
            this.SupportedPlatforms = EPlatform.All;
            this.Definitions = new StringArray();
        }

        public void Write()
        {
            if (System.IO.File.Exists(this.xmlFilename))
            {
                System.IO.FileAttributes attributes = System.IO.File.GetAttributes(this.xmlFilename);
                if (0 != (attributes & System.IO.FileAttributes.ReadOnly))
                {
                    throw new Exception("File '{0}' cannot be written to as it is read only", this.xmlFilename);
                }
            }

            this.PackageIdentifiers.Sort();

            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            string targetNamespace = "Opus";
            string namespaceURI = "http://code.google.com/p/opus";
            System.Xml.XmlElement packageDefinition = document.CreateElement(targetNamespace, "PackageDefinition", namespaceURI);
            {
                string xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                System.Xml.XmlAttribute schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                schemaAttribute.Value = System.String.Format("{0} {1}", namespaceURI, State.OpusPackageDependencySchemaRelativePathNameV2);
                packageDefinition.Attributes.Append(schemaAttribute);
            }
            document.AppendChild(packageDefinition);

            if (this.PackageIdentifiers.Count > 0)
            {
                System.Xml.XmlElement requiredPackages = document.CreateElement(targetNamespace, "RequiredPackages", namespaceURI);
                foreach (PackageIdentifier package in this.PackageIdentifiers)
                {
                    System.Xml.XmlElement packageElement = null;

                    {
                        System.Xml.XmlNode node = requiredPackages.FirstChild;
                        while (node != null)
                        {
                            System.Xml.XmlAttributeCollection attributes = node.Attributes;
                            System.Xml.XmlAttribute nameAttribute = attributes["Name"];
                            if ((null != nameAttribute) && (nameAttribute.Value == package.Name))
                            {
                                packageElement = node as System.Xml.XmlElement;
                                break;
                            }

                            node = node.NextSibling;
                        }
                    }

                    if (null == packageElement)
                    {
                        packageElement = document.CreateElement(targetNamespace, "Package", namespaceURI);
                        packageElement.SetAttribute("Name", package.Name);
                        requiredPackages.AppendChild(packageElement);
                    }
                    {
                        System.Xml.XmlElement packageVersionElement = document.CreateElement(targetNamespace, "Version", namespaceURI);
                        packageVersionElement.SetAttribute("Id", package.Version);

                        EPlatform platformFilter = package.PlatformFilter;
                        if (platformFilter == EPlatform.Invalid)
                        {
                            throw new Exception("Dependent '{0}' requires at least one platform filter", package.ToString());
                        }

                        if (platformFilter != EPlatform.All)
                        {
                            string platformFilterString = Platform.ToString(platformFilter, ' ');
                            string[] split = platformFilterString.Split(' ');
                            string conditionText = null;
                            foreach (string platform in split)
                            {
                                if (null != conditionText)
                                {
                                    conditionText += " || ";
                                }
                                conditionText += System.String.Format("'$(Platform)' == '{0}'", platform);
                            }
                            
                            packageVersionElement.SetAttribute("Condition", conditionText);
                        }
                        packageElement.AppendChild(packageVersionElement);
                    }
                }
                packageDefinition.AppendChild(requiredPackages);
            }

            if (this.OpusAssemblies.Count > 0)
            {
                System.Xml.XmlElement requiredOpusAssemblies = document.CreateElement(targetNamespace, "RequiredOpusAssemblies", namespaceURI);
                foreach (string assemblyName in this.OpusAssemblies)
                {
                    System.Xml.XmlElement assemblyElement = document.CreateElement(targetNamespace, "OpusAssembly", namespaceURI);
                    assemblyElement.SetAttribute("Name", assemblyName);
                    requiredOpusAssemblies.AppendChild(assemblyElement);
                }
                packageDefinition.AppendChild(requiredOpusAssemblies);
            }

            if (this.DotNetAssemblies.Count > 0)
            {
                System.Xml.XmlElement requiredDotNetAssemblies = document.CreateElement(targetNamespace, "RequiredDotNetAssemblies", namespaceURI);
                foreach (DotNetAssemblyDescription desc in this.DotNetAssemblies)
                {
                    System.Xml.XmlElement assemblyElement = document.CreateElement(targetNamespace, "DotNetAssembly", namespaceURI);
                    assemblyElement.SetAttribute("Name", desc.Name);
                    if (null != desc.RequiredTargetFramework)
                    {
                        assemblyElement.SetAttribute("RequiredTargetFramework", desc.RequiredTargetFramework);
                    }
                    requiredDotNetAssemblies.AppendChild(assemblyElement);
                }
                packageDefinition.AppendChild(requiredDotNetAssemblies);
            }

            // supported platforms
            {
                System.Xml.XmlElement supportedPlatformsElement = document.CreateElement(targetNamespace, "SupportedPlatforms", namespaceURI);

                if (EPlatform.Windows == (this.SupportedPlatforms & EPlatform.Windows))
                {
                    System.Xml.XmlElement platformElement = document.CreateElement(targetNamespace, "Platform", namespaceURI);
                    platformElement.SetAttribute("Name", "Windows");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.Unix == (this.SupportedPlatforms & EPlatform.Unix))
                {
                    System.Xml.XmlElement platformElement = document.CreateElement(targetNamespace, "Platform", namespaceURI);
                    platformElement.SetAttribute("Name", "Unix");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.OSX == (this.SupportedPlatforms & EPlatform.OSX))
                {
                    System.Xml.XmlElement platformElement = document.CreateElement(targetNamespace, "Platform", namespaceURI);
                    platformElement.SetAttribute("Name", "OSX");
                    supportedPlatformsElement.AppendChild(platformElement);
                }

                packageDefinition.AppendChild(supportedPlatformsElement);
            }

            // definitions
            if (this.Definitions.Count > 0)
            {
                System.Xml.XmlElement definitionsElement = document.CreateElement(targetNamespace, "Definitions", namespaceURI);

                foreach (string define in this.Definitions)
                {
                    System.Xml.XmlElement defineElement = document.CreateElement(targetNamespace, "Definition", namespaceURI);
                    defineElement.SetAttribute("Name", define);
                    definitionsElement.AppendChild(defineElement);
                }

                packageDefinition.AppendChild(definitionsElement);
            }

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.NewLineChars = "\n";
            xmlWriterSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;

            using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.xmlFilename, xmlWriterSettings))
            {
                document.WriteTo(xmlWriter);
            }

            if (this.validate)
            {
                this.Validate();
            }
        }

        public void Read(bool validateSchemaLocation)
        {
            System.Xml.XmlReaderSettings xmlReaderSettings = new System.Xml.XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            if (this.validate)
            {
                xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
            }
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;
            xmlReaderSettings.XmlResolver = new OpusXmlResolver();

            // try reading the current schema version first
            if (this.ReadCurrent(xmlReaderSettings, validateSchemaLocation))
            {
                if (State.ForceDefinitionFileUpdate)
                {
                    Log.DebugMessage("Forced writing of package definition file '{0}'", this.xmlFilename);
                    this.Write();
                }

                return;
            }

            // fall back on earlier versions of the schema
            if (this.ReadV1(xmlReaderSettings))
            {
                // now write the file out using the curent schema
                this.Write();

                Log.DebugMessage("Converted package definition file '{0}' to the current schema", this.xmlFilename);

                return;
            }

            throw new Exception("An error occurred while reading a package or package definition file '{0}' does not satisfy any of the Opus schemas", this.xmlFilename);
        }

        protected EPlatform InterpretConditionValue(string condition)
        {
            // unless you apply the pattern, remove the match, apply another pattern, etc.?

            //string filter = "'$(Platform)'";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s)";
            //string pattern = @"^('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')\s([\|]{2})";
            //string pattern2 = @"^\s([\|]{2})\s";
            string[] pattern = new string[2];
            pattern[0] = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            pattern[1] = @"([\|]{2})";
            System.Text.RegularExpressions.RegexOptions expOptions =
                System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace |
                System.Text.RegularExpressions.RegexOptions.Singleline;

            EPlatform supportedPlatforms = EPlatform.Invalid;

            int position = 0;
            int patternIndex = 0;
            for (; ; )
            {
                Core.Log.DebugMessage("Searching from pos {0} with pattern index {1}", position, patternIndex);
                if (position > condition.Length)
                {
                    Core.Log.DebugMessage("End of string");
                    break;
                }

                System.Text.RegularExpressions.Regex exp = new System.Text.RegularExpressions.Regex(pattern[patternIndex], expOptions);
                System.Text.RegularExpressions.MatchCollection matches = exp.Matches(condition, position);
                if (0 == matches.Count)
                {
                    break;
                }

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Length > 0)
                    {
                        Core.Log.DebugMessage("Match '{0}'", match.Value);
                        foreach (System.Text.RegularExpressions.Group group in match.Groups)
                        {
                            Core.Log.DebugMessage("\tg '{0}' @ {1}", group.Value, group.Index);
                        }

                        if (0 == patternIndex)
                        {
                            if (match.Groups.Count != 5)
                            {
                                throw new Exception("Expected format to be '$(Platform) == '...''. Instead found '{0}'", match.Value);
                            }

                            if (match.Groups[2].Value != "Platform")
                            {
                                throw new Exception("Expected 'Platform/ in '{0}'", match.Value);
                            }

                            if (match.Groups[3].Value != "==")
                            {
                                throw new Exception("Only supporting equivalence == in '{0}'", match.Value);
                            }

                            string platform = match.Groups[4].Value;
                            EPlatform ePlatform = Platform.FromString(platform);
                            if (ePlatform == EPlatform.Invalid)
                            {
                                throw new Exception("Unrecognize platform '{0}'", platform);
                            }

                            supportedPlatforms |= ePlatform;
                        }
                        else if (1 == patternIndex)
                        {
                            if (match.Groups.Count != 2)
                            {
                                throw new Exception("Expected format to be '... || ...'. Instead found '{0}'", match.Value);
                            }

                            if (match.Groups[1].Value != "||")
                            {
                                throw new Exception("Only supporting Boolean OR || in '{0}'", match.Value);
                            }
                        }

                        position += match.Length + 1;
                        ++patternIndex;
                        patternIndex = patternIndex % 2;

                        break;
                    }
                }
            }
            Core.Log.DebugMessage("End of matches");

            return supportedPlatforms;
        }

        private bool ReadRequiredPackages(System.Xml.XmlReader xmlReader)
        {
            string requiredPackagesElementName = "Opus:RequiredPackages";
            if (requiredPackagesElementName != xmlReader.Name)
            {
                return false;
            }

            string packageElementName = "Opus:Package";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredPackagesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (xmlReader.Name == packageElementName)
                {
                    string packageNameAttribute = "Name";
                    if (!xmlReader.MoveToAttribute(packageNameAttribute))
                    {
                        throw new Exception("Required attribute 'Name' of 'Package' node missing");
                    }
                    string packageName = xmlReader.Value;

                    string packageVersionElementName = "Opus:Version";
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.Name == packageElementName) &&
                            (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                        {
                            break;
                        }

                        if (xmlReader.Name == packageVersionElementName)
                        {
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                string packageVersionIdAttribute = "Id";
                                if (!xmlReader.MoveToAttribute(packageVersionIdAttribute))
                                {
                                    throw new Exception("Required 'Id' attribute of 'Version' node missing");
                                }
                                string packageVersion = xmlReader.Value;

                                EPlatform platformFilter = EPlatform.All;
                                string packageConditionAttribute = "Condition";
                                if (xmlReader.MoveToAttribute(packageConditionAttribute))
                                {
                                    string conditionValue = xmlReader.Value;
                                    platformFilter = this.InterpretConditionValue(conditionValue);
                                }

                                PackageIdentifier id = new PackageIdentifier(packageName, packageVersion);
                                id.PlatformFilter = platformFilter;
                                this.PackageIdentifiers.Add(id);
                            }
                        }
                        else
                        {
                            throw new Exception("Unexpected element");
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'", requiredPackagesElementName);
                }
            }

            return true;
        }

        private bool ReadRequiredOpusAssemblies(System.Xml.XmlReader xmlReader)
        {
            string requiredOpusAssembliesElementName = "Opus:RequiredOpusAssemblies";
            if (requiredOpusAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            string opusAssemblyElementName = "Opus:OpusAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredOpusAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (opusAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        string assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of 'Opus:OpusAssembly' node missing");
                        }
                        string assemblyName = xmlReader.Value;

                        this.OpusAssemblies.Add(assemblyName);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'", requiredOpusAssembliesElementName);
                }
            }

            return true;
        }

        private bool ReadRequiredDotNetAssemblies(System.Xml.XmlReader xmlReader)
        {
            string requiredDotNetAssembliesElementName = "Opus:RequiredDotNetAssemblies";
            if (requiredDotNetAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            string dotNetAssemblyElementName = "Opus:DotNetAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredDotNetAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (dotNetAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        string assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of 'Opus:DotNetAssembly' node missing");
                        }
                        string assemblyName = xmlReader.Value;

                        DotNetAssemblyDescription desc = new DotNetAssemblyDescription(assemblyName);

                        string assemblyRequiredTargetFrameworkNameAttribute = "RequiredTargetFramework";
                        if (xmlReader.MoveToAttribute(assemblyRequiredTargetFrameworkNameAttribute))
                        {
                            desc.RequiredTargetFramework = xmlReader.Value;
                        }

                        this.DotNetAssemblies.Add(desc);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'", requiredDotNetAssembliesElementName);
                }
            }

            return true;
        }

        private bool ReadSupportedPlatforms(System.Xml.XmlReader xmlReader)
        {
            string supportedPlatformsElementName = "Opus:SupportedPlatforms";
            if (supportedPlatformsElementName != xmlReader.Name)
            {
                return false;
            }

            this.SupportedPlatforms = EPlatform.Invalid;
            string platformElementName = "Opus:Platform";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == supportedPlatformsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (platformElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        string platformNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(platformNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of 'Opus:Platform' node missing");
                        }

                        string platformName = xmlReader.Value;
                        if ("Windows" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Windows;
                        }
                        if ("Unix" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Unix;
                        }
                        if ("OSX" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.OSX;
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'", supportedPlatformsElementName);
                }
            }

            return true;
        }

        private bool ReadDefinitions(System.Xml.XmlReader xmlReader)
        {
            string definitionsElementName = "Opus:Definitions";
            if (definitionsElementName != xmlReader.Name)
            {
                return false;
            }

            string defineElementName = "Opus:Definition";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == definitionsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (defineElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        string defineNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(defineNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of 'Opus:Definition' node missing");
                        }

                        string definition = xmlReader.Value;

                        this.Definitions.Add(definition);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'", definitionsElementName);
                }
            }

            return true;
        }

        protected bool ReadCurrent(System.Xml.XmlReaderSettings readerSettings, bool validateSchemaLocation)
        {
            try
            {
                System.Xml.XmlReaderSettings settings = readerSettings.Clone();
                if (this.validate)
                {
                    if (validateSchemaLocation)
                    {
                        settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                    }
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                }

                using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(this.xmlFilename, settings))
                {
                    string rootElementName = "Opus:PackageDefinition";
                    if (!xmlReader.ReadToFollowing(rootElementName))
                    {
                        Log.DebugMessage("Root element '{0}' not found in '{1}'. Xml instance may be referencing an old Opus schema. This file will now be upgraded to the latest schema.", rootElementName, this.xmlFilename);
                        return false;
                    }

                    while (xmlReader.Read())
                    {
                        if (ReadRequiredPackages(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredOpusAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredDotNetAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSupportedPlatforms(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitions(xmlReader))
                        {
                            // all done
                        }
                        else if (xmlReader.Name == rootElementName)
                        {
                            // should be the end element
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                throw new Exception("Expected end of root element but found '{0}'", xmlReader.Name);
                            }
                        }
                        else
                        {
                            throw new Exception("Package definition reading code failed to recognize element with name '{0}'", xmlReader.Name);
                        }
                    }

                    if (!xmlReader.EOF)
                    {
                        throw new Exception("Failed to read all of file");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public bool ReadV1(System.Xml.XmlReaderSettings readerSettings)
        {
            try
            {
                System.Xml.XmlReaderSettings settings = readerSettings.Clone();
                settings.Schemas.Add(null, State.OpusPackageDependencySchemaPathName);

                using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(this.xmlFilename, settings))
                {
                    while (xmlReader.Read())
                    {
                        if (System.Xml.XmlNodeType.XmlDeclaration == xmlReader.NodeType)
                        {
                            // do nothing
                        }
                        else if (System.Xml.XmlNodeType.Whitespace == xmlReader.NodeType)
                        {
                            // do nothing
                        }
                        else if (System.Xml.XmlNodeType.Element == xmlReader.NodeType)
                        {
                            if ("RequiredPackages" == xmlReader.Name)
                            {
                                while (xmlReader.Read())
                                {
                                    if (System.Xml.XmlNodeType.Whitespace == xmlReader.NodeType)
                                    {
                                        // do nothing
                                    }
                                    else if (System.Xml.XmlNodeType.EndElement == xmlReader.NodeType)
                                    {
                                        if ("RequiredPackages" == xmlReader.Name)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            throw new Exception("Unexpected EndElement");
                                        }
                                    }
                                    else if (System.Xml.XmlNodeType.Element == xmlReader.NodeType)
                                    {
                                        if ("Package" == xmlReader.Name)
                                        {
                                            if (xmlReader.HasAttributes)
                                            {
                                                xmlReader.MoveToAttribute("Name");
                                                string packageName = xmlReader.Value;
                                                xmlReader.MoveToAttribute("Version");
                                                string packageVersion = xmlReader.Value;

                                                PackageIdentifier id = new PackageIdentifier(packageName, packageVersion);
                                                id.PlatformFilter = EPlatform.All;
                                                this.PackageIdentifiers.Add(id);

                                                xmlReader.MoveToElement();
                                            }
                                            else
                                            {
                                                throw new Exception("Package element has no attributes");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!xmlReader.EOF)
                    {
                        throw new System.Xml.XmlException("Failed to read all of file");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.MessageAll("Package Definition Read of Schema v1 failed: '{0}'", ex.Message);
                return false;
            }

            // add required Opus assemblies
            this.OpusAssemblies.Add("Opus.Core");

            // add required DotNet assemblies
            {
                DotNetAssemblyDescription systemDesc = new DotNetAssemblyDescription("System");
                DotNetAssemblyDescription systemXmlDesc = new DotNetAssemblyDescription("System.Xml");

                systemDesc.RequiredTargetFramework = "2.0.50727";
                systemXmlDesc.RequiredTargetFramework = "2.0.50727";

                this.DotNetAssemblies.AddRange(new DotNetAssemblyDescription[]
                    { systemDesc,
                      systemXmlDesc
                    });
            }
            
            // supported on all platforms
            this.SupportedPlatforms = EPlatform.All;

            return true;
        }

        public void AddRequiredPackage(PackageIdentifier idToAdd)
        {
            foreach (PackageIdentifier id in this.PackageIdentifiers)
            {
                if (id.Match(idToAdd, false))
                {
                    throw new Exception("Package '{0}' is already included in the dependency file", id.ToString());
                }
            }

            this.PackageIdentifiers.Add(idToAdd);
            Log.Info("Added dependency '{0}' from root '{1}'", idToAdd.ToString(), idToAdd.Root);
        }

        public bool RemovePackage(PackageIdentifier idToRemove)
        {
            PackageIdentifier idToRemoveReally = null;
            foreach (PackageIdentifier id in this.PackageIdentifiers)
            {
                if (id.Match(idToRemove, false))
                {
                    idToRemoveReally = id;
                    break;
                }
            }

            if (null != idToRemoveReally)
            {
                this.PackageIdentifiers.Remove(idToRemoveReally);
                Log.Info("Removed dependency '{0}' from root '{1}'", idToRemove.ToString(), idToRemove.Root);
                return true;
            }
            else
            {
                Log.Info("Could not find reference to package '{0}' to remove", idToRemove.ToString());
                return false;
            }
        }

        public PackageIdentifierCollection PackageIdentifiers
        {
            get;
            private set;
        }

        public StringArray OpusAssemblies
        {
            get;
            private set;
        }

        public Array<DotNetAssemblyDescription> DotNetAssemblies
        {
            get;
            private set;
        }

        public EPlatform SupportedPlatforms
        {
            get;
            set;
        }

        public StringArray Definitions
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.xmlFilename;
        }
    }
}
