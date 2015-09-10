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
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.DynamicLibrary moduleToBuild,
            out bool success)
        {
            var dynamicLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;

            // dependents
            var inputVariables = new MakeFileVariableDictionary();
            var dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var childNode in node.Children)
                {
                    if (null == childNode.Data)
                    {
                        continue;
                    }
                    var data = childNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(keysToFilter));
                    dataArray.Add(data);
                }
            }
            if (null != node.ExternalDependents)
            {
                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey);
                if (target.HasPlatform(Bam.Core.EPlatform.Unix))
                {
                    // TODO: why is the symlink not present?
                    //libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(libraryKeysToFilter));
                    dataArray.Add(data);
                }
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var dynamicLibraryOptions = dynamicLibraryModule.Options;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (dynamicLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = dynamicLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    // libraries are manually added later
                    var excludedOptions = new Bam.Core.StringArray("Libraries", "StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);
                }
                else
                {
                    var excludedOptions = new Bam.Core.StringArray();
                    excludedOptions.Add("RPath"); // $ORIGIN is handled differently
                    excludedOptions.Add("Libraries");
                    excludedOptions.Add("StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);

                    // handle RPath separately
                    // what needs to happen is that the $ must be doubled, and the entire path quoted
                    // http://stackoverflow.com/questions/230364/how-to-get-rpath-with-origin-to-work-on-codeblocks-gcc
                    var rPathCommandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray();
                    optionNames.Add("RPath");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(dynamicLibraryOptions, rPathCommandLine, target, optionNames);
                    foreach (var rpath in rPathCommandLine)
                    {
                        var linkerCommand = rpath.Split(',');
                        var rpathDir = linkerCommand[linkerCommand.Length - 1];
                        rpathDir = rpathDir.Replace("$ORIGIN", "$$ORIGIN");
                        rpathDir = System.String.Format("'{0}'", rpathDir);
                        var linkerCommandArray = new Bam.Core.StringArray(linkerCommand);
                        linkerCommandArray.Remove(linkerCommand[linkerCommand.Length - 1]);
                        linkerCommandArray.Add(rpathDir);
                        var newRPathCommand = linkerCommandArray.ToString(',');
                        commandLineBuilder.Add(newRPathCommand);
                    }
                }
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            var toolset = target.Toolset;
            var linkerTool = toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var executable = linkerTool.Executable((Bam.Core.BaseTarget)target);

            var recipeBuilder = new System.Text.StringBuilder();
            if (executable.Contains(" "))
            {
                recipeBuilder.AppendFormat("\"{0}\"", executable);
            }
            else
            {
                recipeBuilder.Append(executable);
            }

            var dependentLibraryCommandLine = new Bam.Core.StringArray();
            {
                var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;

                recipeBuilder.AppendFormat(" {0} ", commandLineBuilder.ToString(' '));

                var extensionFilters = new Bam.Core.StringArray();
                extensionFilters.AddUnique(compilerTool.ObjectFileSuffix);

                if (toolset.HasTool(typeof(C.IWinResourceCompilerTool)))
                {
                    var winResourceCompilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;
                    extensionFilters.AddUnique(winResourceCompilerTool.CompiledResourceSuffix);
                }

                // TODO: don't want to access the archiver tool here really, as creating
                // an application does not require one
                // although we do need to know where static libraries are written
                // perhaps the ILinkerTool can have a duplicate of the static library suffix?
                var archiverTool = toolset.Tool(typeof(C.IArchiverTool)) as C.IArchiverTool;

                var dependentLibraries = new Bam.Core.StringArray();
                extensionFilters.AddUnique(archiverTool.StaticLibrarySuffix);
                if (linkerTool is C.IWinImportLibrary)
                {
                    extensionFilters.AddUnique((linkerTool as C.IWinImportLibrary).ImportLibrarySuffix);
                }
                else
                {
                    extensionFilters.AddUnique(linkerTool.DynamicLibrarySuffix);
                }

                foreach (var ext in extensionFilters)
                {
                    dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", ext));
                }

                C.LinkerUtilities.AppendLibrariesToCommandLine(dependentLibraryCommandLine, linkerTool, dynamicLibraryOptions as C.ILinkerOptions, dependentLibraries);
            }

            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            var primaryOutputKey = C.Application.OutputFile;
            var outputPath = moduleToBuild.Locations[primaryOutputKey].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");
            var instanceName = MakeFile.InstanceName(node);
            var toolOutputLocKeys = (linkerTool as Bam.Core.ITool).OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);

            // replace non-primary outputs with their variable names
            foreach (var outputKey in outputFileLocations)
            {
                if (outputKey == primaryOutputKey)
                {
                    continue;
                }

                var outputLoc = moduleToBuild.Locations[outputKey];
                if (!outputLoc.IsValid)
                {
                    continue;
                }

                var variableName = System.String.Format("$({0}_{1}_Variable)", instanceName, outputKey.ToString());
                var outputLocPath = outputLoc.GetSinglePath();
                recipe = recipe.Replace(outputLocPath, variableName);
            }

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.Application.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                inputVariables,
                null,
                recipes);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;
            makeFile.RuleArray.Add(rule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (linkerTool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (linkerTool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}