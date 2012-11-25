// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            // any source file generated by moc should not be included in the .pro file (QMake handles this)
            string sourceFilePath = objectFile.SourceFile.AbsolutePath;
            // cannot assume QtCommon is a dependency in the project, so can't use QtCommon.MocFile.Prefix
            // (TODO: there is an issue adding QtCommon as a dependency to QMakeBuilder to do with the moc version)
            if (System.IO.Path.GetFileNameWithoutExtension(sourceFilePath).StartsWith("moc_"))
            {
                success = true;
                return null;
            }

            Opus.Core.BaseModule objectFileModule = objectFile as Opus.Core.BaseModule;
            Opus.Core.Target target = objectFileModule.OwningNode.Target;
            Opus.Core.BaseOptionCollection objectFileOptions = objectFileModule.Options;

            C.CompilerOptionCollection compilerOptions = objectFileOptions as C.CompilerOptionCollection;
            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("SOURCES", sourceFilePath);
            if (objectFileOptions is C.ICxxCompilerOptions)
            {
                Opus.Core.ITool compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool));
                nodeData.AddUniqueVariable("CXXFLAGS", commandLineBuilder);
                nodeData.AddUniqueVariable("QMAKE_CXX", new Opus.Core.StringArray(compilerTool.Executable(target).Replace("\\", "/")));
            }
            else
            {
                Opus.Core.ITool compilerTool = target.Toolset.Tool(typeof(C.ICxxCompilerTool));
                nodeData.AddUniqueVariable("CFLAGS", commandLineBuilder);
                nodeData.AddUniqueVariable("QMAKE_CC", new Opus.Core.StringArray(compilerTool.Executable(target).Replace("\\", "/")));
            }
            nodeData.AddUniqueVariable("OBJECTS_DIR", new Opus.Core.StringArray(compilerOptions.OutputDirectoryPath));

            success = true;
            return nodeData;
        }
    }
}