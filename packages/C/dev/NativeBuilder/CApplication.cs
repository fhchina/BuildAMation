// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.Application application, out bool success)
        {
            Opus.Core.BaseModule applicationModule = application as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = applicationModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection applicationOptions = applicationModule.Options;
            C.ILinkerOptions linkerOptions = applicationOptions as C.ILinkerOptions;

            C.OutputFileFlags objectFileFlags = C.OutputFileFlags.ObjectFile;
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                objectFileFlags |= C.OutputFileFlags.Win32CompiledResource;
            }

            // find dependent object files
            Opus.Core.StringArray dependentObjectFiles = new Opus.Core.StringArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputPaths(objectFileFlags, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputPaths(objectFileFlags, dependentObjectFiles);
            }
            if (0 == dependentObjectFiles.Count)
            {
                Opus.Core.Log.Detail("There were no object files to link for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            // find dependent library files
            Opus.Core.StringArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.StaticLibrary | C.OutputFileFlags.StaticImportLibrary, dependentLibraryFiles);
            }

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.AddRange(dependentObjectFiles);
                if (null != dependentLibraryFiles)
                {
                    inputFiles.AddRange(dependentLibraryFiles);
                }
                Opus.Core.StringArray outputFiles = applicationOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.ToString(' '));

            // then libraries
            C.ILinkerTool linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            C.LinkerUtilities.AppendLibrariesToCommandLine(commandLineBuilder, linkerTool, linkerOptions, dependentLibraryFiles);

            int exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}