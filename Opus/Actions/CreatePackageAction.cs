﻿// <copyright file="CreatePackageAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.CreatePackageAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class CreatePackageAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-createpackage";
            }
        }

        public string Description
        {
            get
            {
                return "Create a new package";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.PackagePath = arguments;
        }

        private string PackagePath
        {
            get;
            set;
        }

        public bool Execute()
        {
#if true
            bool isComplete;
            Core.PackageIdentifier id = Core.PackageUtilities.IsPackageDirectory(this.PackagePath,
                                                                                 out isComplete);
            if ((null != id) || isComplete)
            {
                throw new Core.Exception(System.String.Format("Package already present at '{0}'", this.PackagePath), false);
            }
#else
            Core.PackageInformation createPackage = Core.PackageInformation.FromPath(this.PackagePath, false);
            if (null == createPackage)
            {
                throw new Core.Exception(System.String.Format("Path '{0}' is not a valid package path", this.PackagePath), false);
            }

            Core.PackageInformationCollection packageInfoCollection = Core.State.PackageInfo;
            if (null == packageInfoCollection)
            {
                packageInfoCollection = new Opus.Core.PackageInformationCollection();
                Core.State.PackageInfo = packageInfoCollection;
            }

            packageInfoCollection.Add(createPackage);

            Core.Log.DebugMessage("Package is '{0}' @ '{1}'", createPackage.Identifier.ToString("-"), createPackage.Root);
            Core.Log.DebugMessage("Package roots are:");
            foreach (string packageRoot in Core.State.PackageRoots)
            {
                Core.Log.DebugMessage("\t'{0}'", packageRoot);
            }

            string PackageDirectory = createPackage.Directory;
#endif
            string PackageDirectory = this.PackagePath;
            if (System.IO.Directory.Exists(PackageDirectory))
            {
                Core.Log.Info("Package directory already exists at '{0}'", PackageDirectory);
                return false;
            }
            else
            {
                System.IO.Directory.CreateDirectory(PackageDirectory);
            }

            id = Core.PackageUtilities.IsPackageDirectory(this.PackagePath, out isComplete);

            // Xml file for dependencies
            Core.PackageDependencyXmlFile packageDefinition = new Opus.Core.PackageDependencyXmlFile(id.DefinitionPathName, true);
            if (null == packageDefinition)
            {
                throw new Core.Exception(System.String.Format("Package definition file '%s' could not be created", packageDefinition), false);
            }

            if (Core.State.PackageCreationDependents != null)
            {
                Core.Log.DebugMessage("Adding dependent packages:");
                foreach (string dependentPackage in Core.State.PackageCreationDependents)
                {
                    Core.Log.DebugMessage("\t'{0}'", dependentPackage);
                    string[] packageNameAndVersion = dependentPackage.Split('-');
                    if (packageNameAndVersion.Length != 2)
                    {
                        throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageNameAndVersion), false);
                    }

                    packageDefinition.AddRequiredPackage(packageNameAndVersion[0], packageNameAndVersion[1]);
                }
            }
            packageDefinition.Write();
            id.Definition = packageDefinition;

            // Script file
            string packageScriptPathname = id.ScriptPathName;
            using (System.IO.TextWriter scriptWriter = new System.IO.StreamWriter(packageScriptPathname))
            {
                scriptWriter.WriteLine("// Automatically generated by Opus v" + Core.State.OpusVersionString);
                scriptWriter.WriteLine("namespace " + id.Name);
                scriptWriter.WriteLine("{");
                scriptWriter.WriteLine("    // Add modules here");
                scriptWriter.WriteLine("}");
                scriptWriter.Close();
            }

            Core.Log.Info("Successfully created package '{0}' at '{1}'", id.ToString("-"), PackageDirectory);

            return true;
        }
    }
}