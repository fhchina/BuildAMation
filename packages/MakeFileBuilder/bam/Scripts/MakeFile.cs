#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace MakeFileBuilder
{
    public sealed class MakeFile
    {
        private string
        RelativePath(
            string path)
        {
            var relativeDir = Bam.Core.RelativePathUtilities.GetPath(path, this.TopLevelMakeFilePath, "$(CURDIR)");
            return relativeDir;
        }

        public MakeFileTargetDictionary ExportedTargets
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary ExportedVariables
        {
            get;
            private set;
        }

        private Bam.Core.StringArray InputFiles
        {
            get;
            set;
        }

        private Bam.Core.StringArray InputVariables
        {
            get;
            set;
        }

        private string ModulePrefixName
        {
            get;
            set;
        }

        public string VariableName
        {
            get
            {
                if (null == this.SecondaryVariableName)
                {
                    return this.MainVariableName;
                }
                else
                {
                    return this.SecondaryVariableName;
                }
            }
        }

        private string MainVariableName
        {
            get;
            set;
        }

        public string SecondaryVariableName
        {
            get;
            private set;
        }

        public string TargetName
        {
            get;
            private set;
        }

        private string TopLevelMakeFilePath
        {
            get;
            set;
        }

        public Bam.Core.StringArray Includes
        {
            get;
            private set;
        }

        public Bam.Core.Array<MakeFileRule> RuleArray
        {
            get;
            private set;
        }

        public static string
        InstanceName(
            Bam.Core.DependencyNode node)
        {
            var instanceName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());
            return instanceName;
        }

        public
        MakeFile(
            Bam.Core.DependencyNode node,
            string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.ExportedTargets = new MakeFileTargetDictionary();
            this.ExportedVariables = new MakeFileVariableDictionary();
            this.RuleArray = new Bam.Core.Array<MakeFileRule>();
            this.ModulePrefixName = InstanceName(node);
        }

        public void
        Write(
            System.IO.TextWriter writer)
        {
            if (0 == this.RuleArray.Count)
            {
                throw new Bam.Core.Exception("MakeFile '{0}' has no rules", this.ModulePrefixName);
            }

            var ruleCount = this.RuleArray.Count;
            var ruleIndex = 0;
            foreach (var rule in this.RuleArray)
            {
                string mainVariableName;
                if (ruleCount > 1)
                {
                    mainVariableName = System.String.Format("{0}{1}", this.ModulePrefixName, ruleIndex++);
                }
                else
                {
                    mainVariableName = this.ModulePrefixName;
                }

                string outputDirectoriesVariable = null;
                if (null != rule.DirectoriesToCreate)
                {
                    writer.WriteLine("# Define directories to create");
                    string linearizedDirsToCreate = null;
                    foreach (var dir in rule.DirectoriesToCreate)
                    {
                        var dirPath = dir.GetSinglePath();
                        var relativeDir = this.RelativePath(dirPath);
                        linearizedDirsToCreate += relativeDir + " ";
                    }
                    outputDirectoriesVariable = System.String.Format("{0}_BuildDirs", mainVariableName);
                    writer.WriteLine("{0} := {1}", outputDirectoriesVariable, linearizedDirsToCreate);
                    writer.WriteLine("builddirs += $({0})", outputDirectoriesVariable);
                    writer.WriteLine("");
                }

                string mainExportVariableName = null;
                if (rule.ExportVariable)
                {
                    if (rule.ModuleToBuild is Bam.Core.IModuleCollection)
                    {
                        writer.WriteLine("# Output variable (collection)");
                        var exportVariableName = System.String.Format("{0}_{1}_CollectionVariable", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                        mainExportVariableName = exportVariableName;

                        var variableAndValue = new System.Text.StringBuilder();
                        variableAndValue.AppendFormat("{0} := ", exportVariableName);
                        foreach (var prerequisite in rule.InputVariables)
                        {
                            foreach (var pre in prerequisite.Value)
                            {
                                variableAndValue.AppendFormat("$({0}) ", pre);
                            }
                        }
                        if (null != rule.InputFiles)
                        {
                            foreach (var prerequisiteFile in rule.InputFiles)
                            {
                                var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                            }
                        }

                        writer.WriteLine(variableAndValue.ToString());
                        writer.WriteLine("");

                        var exportedVariables = new Bam.Core.StringArray();
                        exportedVariables.Add(exportVariableName);
                        this.ExportedVariables.Add(rule.PrimaryOutputLocationKey, exportedVariables);
                    }
                    else
                    {
                        if (null == rule.OutputLocationKeys)
                        {
                            throw new Bam.Core.Exception("No output keys have been assigned to Makefile rule for target '{1}'", rule.ModuleToBuild.OwningNode.UniqueModuleName, rule.Target);
                        }

                        foreach (var outputLocKey in rule.OutputLocationKeys)
                        {
                            var outputLoc = rule.ModuleToBuild.Locations[outputLocKey];
                            if (!outputLoc.IsValid)
                            {
                                continue;
                            }

                            var exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, outputLocKey.ToString());
                            if (outputLocKey == rule.PrimaryOutputLocationKey)
                            {
                                mainExportVariableName = exportVariableName;
                            }
                            writer.WriteLine("# Output variable: '{0}'", outputLocKey.ToString());

                            if (rule.ExportTarget)
                            {
                                var relativeOutputPath = this.RelativePath(outputLoc.GetSinglePath());
                                writer.WriteLine(exportVariableName + " := " + relativeOutputPath);
                                writer.WriteLine("");
                            }
                            else
                            {
                                var variableAndValue = new System.Text.StringBuilder();
                                variableAndValue.AppendFormat("{0} := ", exportVariableName);
                                foreach (var prerequisite in rule.InputVariables)
                                {
                                    foreach (var pre in prerequisite.Value)
                                    {
                                        variableAndValue.AppendFormat("$({0}) ", pre);
                                    }
                                }
                                if (null != rule.InputFiles)
                                {
                                    foreach (var prerequisiteFile in rule.InputFiles)
                                    {
                                        var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                        variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                                    }
                                }

                                writer.WriteLine(variableAndValue.ToString());
                                writer.WriteLine("");
                            }

                            var exportedVariables = new Bam.Core.StringArray();
                            exportedVariables.Add(exportVariableName);
                            this.ExportedVariables.Add(outputLocKey, exportedVariables);

                            if (null != outputDirectoriesVariable)
                            {
                                writer.WriteLine("# Order-only dependencies on directories to create");
                                writer.WriteLine("$({0}): | $({1})", exportVariableName, outputDirectoriesVariable);
                                writer.WriteLine("");
                            }
                        }
                    }
                }

                if (rule.ExportTarget && null != mainExportVariableName)
                {
                    var exportTargetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                    writer.WriteLine("# Output target");
                    if (rule.TargetIsPhony)
                    {
                        writer.WriteLine(".PHONY: {0}", exportTargetName);
                    }
                    writer.WriteLine(exportTargetName + ": $(" + mainExportVariableName + ")");
                    writer.WriteLine("");
                    this.ExportedTargets.Add(rule.PrimaryOutputLocationKey, exportTargetName);

                    if (null == rule.Recipes)
                    {
                        continue;
                    }

                    var targetAndPrerequisites = new System.Text.StringBuilder();
                    if (null != mainExportVariableName)
                    {
                        targetAndPrerequisites.AppendFormat("$({0}): ", mainExportVariableName);
                    }
                    else
                    {
                        var targetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                        if (rule.TargetIsPhony)
                        {
                            writer.WriteLine(".PHONY: {0}", targetName);
                        }
                        targetAndPrerequisites.AppendFormat("{0}: ", targetName);
                        if (rule.ExportTarget)
                        {
                            this.ExportedTargets.Add(rule.PrimaryOutputLocationKey, targetName);
                        }
                    }
                    if (null != rule.InputVariables)
                    {
                        foreach (var prerequisite in rule.InputVariables)
                        {
                            foreach (var pre in prerequisite.Value)
                            {
                                targetAndPrerequisites.AppendFormat("$({0}) ", pre);
                            }
                        }
                    }
                    if (null != rule.InputFiles)
                    {
                        foreach (var prerequisiteFile in rule.InputFiles)
                        {
                            var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                            targetAndPrerequisites.AppendFormat("{0} ", relativePrerequisiteFile);
                        }
                    }

                    writer.WriteLine("# Rule");
                    writer.WriteLine(targetAndPrerequisites.ToString());
                    foreach (var recipe in rule.Recipes)
                    {
                        writer.WriteLine("\t" + recipe);
                    }
                    writer.WriteLine("");
                }
            }
        }
    }
}