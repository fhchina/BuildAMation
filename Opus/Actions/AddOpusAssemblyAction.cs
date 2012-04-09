﻿// <copyright file="AddOpusAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddOpusAssemblyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddOpusAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addopusassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Adds an Opus assembly to the package definition (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            string[] assemblyNames = arguments.Split(System.IO.Path.PathSeparator);
            this.OpusAssemblyNameArray = new Opus.Core.StringArray(assemblyNames);
        }

        private Core.StringArray OpusAssemblyNameArray
        {
            get;
            set;
        }

        public bool Execute()
        {
            bool isComplete;
            Core.PackageIdentifier mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isComplete);
            if (null == mainPackageId)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a package", Core.State.WorkingDirectory), false);
            }
            if (!isComplete)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory), false);
            }

            Core.PackageDefinitionFile xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isComplete)
            {
                xmlFile.Read(true);
            }

            bool success = false;
            foreach (string OpusAssemblyName in this.OpusAssemblyNameArray)
            {
                if (!xmlFile.OpusAssemblies.Contains(OpusAssemblyName))
                {
                    xmlFile.OpusAssemblies.Add(OpusAssemblyName);

                    Core.Log.MessageAll("Added Opus assembly '{0}' to package '{1}'", OpusAssemblyName, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Opus assembly '{0}' already used by package '{1}'", OpusAssemblyName, mainPackageId.ToString());
                }
            }

            if (success)
            {
                xmlFile.Write();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}