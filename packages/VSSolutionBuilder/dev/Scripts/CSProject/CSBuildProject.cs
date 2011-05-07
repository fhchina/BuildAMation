// <copyright file="CSBuildProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class CSBuildProject : ICProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid = System.Guid.NewGuid();
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private ProjectFileCollection HeaderFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();

        public CSBuildProject(string moduleName, string projectPathName, string packageDirectory)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageDirectory;

            if (packageDirectory[packageDirectory.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                this.PackageUri = new System.Uri(packageDirectory, System.UriKind.Absolute);
            }
            else
            {
                this.PackageUri = new System.Uri(packageDirectory + System.IO.Path.DirectorySeparatorChar, System.UriKind.Absolute);
            }
        }

        string IProject.Name
        {
            get
            {
                return this.ProjectName;
            }
        }

        string IProject.PathName
        {
            get
            {
                return this.PathName;
            }
        }

        System.Guid IProject.Guid
        {
            get
            {
                return this.ProjectGuid;
            }
        }

        public string PackageDirectory
        {
            get;
            private set;
        }

        System.Collections.Generic.List<string> IProject.Platforms
        {
            get
            {
                return this.PlatformList;
            }
        }

        ProjectConfigurationCollection IProject.Configurations
        {
            get
            {
                return this.ProjectConfigurations;
            }
        }

        ProjectFileCollection IProject.SourceFiles
        {
            get
            {
                return this.SourceFileCollection;
            }
        }

        ProjectFileCollection ICProject.HeaderFiles
        {
            get
            {
                return this.HeaderFileCollection;
            }
        }

        System.Collections.Generic.List<IProject> IProject.DependentProjects
        {
            get
            {
                return this.DependentProjectList;
            }
        }

        void IProject.Serialize()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                System.Uri projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by Opus v" + Opus.Core.State.OpusVersionString));

                MSBuildProject project = new MSBuildProject(xmlDocument, "3.5");

#if true
                MSBuildPropertyGroup generalGroup = project.CreatePropertyGroup();
                generalGroup.CreateProperty("ProjectGuid", this.ProjectGuid.ToString("B").ToUpper());
                // default configuration and platform
                {
                    MSBuildProperty defaultConfiguration = generalGroup.CreateProperty("Configuration", "Debug");
                    defaultConfiguration.Condition = " '$(Configuration)' == '' ";
                }
                {
                    MSBuildProperty defaultPlatform = generalGroup.CreateProperty("Platform", "AnyCPU");
                    defaultPlatform.Condition = " '$(Platform)' == '' ";
                }

                // configurations
                foreach (ProjectConfiguration configuration in this.ProjectConfigurations)
                {
                    configuration.SerializeCSBuild(project, projectLocationUri);
                }

                // source files
                if (this.SourceFileCollection.Count > 0)
                {
                    this.SourceFileCollection.SerializeMSBuild(project, "Compile", projectLocationUri, this.PackageUri);
                }

                // project dependencies
                if (this.DependentProjectList.Count > 0)
                {
                    MSBuildItemGroup dependencyItemGroup = project.CreateItemGroup();
                    foreach (IProject dependentProject in this.DependentProjectList)
                    {
                        string relativePath = Opus.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        MSBuildItem projectReference = dependencyItemGroup.CreateItem("ProjectReference", relativePath);
                        projectReference.CreateMetaData("Project", dependentProject.Guid.ToString("B").ToUpper());
                        projectReference.CreateMetaData("Name", dependentProject.Name);
                    }
                }

                // import C# project props
                project.CreateImport(@"$(MSBuildBinPath)\Microsoft.CSharp.targets");
#else
                // project globals (guid, etc)
                {
                    MSBuildPropertyGroup globalPropertyGroup = project.CreatePropertyGroup();
                    globalPropertyGroup.Label = "Globals";
                    globalPropertyGroup.CreateProperty("ProjectGuid", this.ProjectGuid.ToString("B").ToUpper());
                }

                // import default project props
                project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");

                // configurations
                this.ProjectConfigurations.SerializeMSBuild(project, projectLocationUri);

                // source files
                if (this.SourceFileCollection.Count > 0)
                {
                    this.SourceFileCollection.SerializeMSBuild(project, "ClCompile", projectLocationUri, this.PackageUri);
                }
                if (this.HeaderFileCollection.Count > 0)
                {
                    this.SourceFileCollection.SerializeMSBuild(project, "ClInclude", projectLocationUri, this.PackageUri);
                }

                // project dependencies
                // these were in the .sln file pre MSBuild
                if (this.DependentProjectList.Count > 0)
                {
                    MSBuildItemGroup dependencyItemGroup = project.CreateItemGroup();
                    foreach (IProject dependentProject in this.DependentProjectList)
                    {
                        string relativePath = Opus.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        MSBuildItem projectReference = dependencyItemGroup.CreateItem("ProjectReference", relativePath);
                        projectReference.CreateMetaData("Project", dependentProject.Guid.ToString("B"));
                        projectReference.CreateMetaData("ReferenceOutputAssembly", "false");
                    }
                }

                // import targets
                project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");
#endif
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;

            try
            {
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                }
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }
        }
    }
}
