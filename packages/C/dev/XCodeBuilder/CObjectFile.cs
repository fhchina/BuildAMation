// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            var sourceFile = moduleToBuild.SourceFileLocation;

            var project = this.Workspace.GetProject(node);

            PBXFileReference.EType fileType = PBXFileReference.EType.CSourceFile;
            if (moduleToBuild is C.ObjCxx.ObjectFile)
            {
                fileType = PBXFileReference.EType.ObjCxxSourceFile;
            }
            else if (moduleToBuild is C.ObjC.ObjectFile)
            {
                fileType = PBXFileReference.EType.ObjCSourceFile;
            }
            else if (moduleToBuild is C.Cxx.ObjectFile)
            {
                fileType = PBXFileReference.EType.CxxSourceFile;
            }
            var fileRef = project.FileReferences.Get(moduleName, fileType, sourceFile, project.RootUri);

            var sourcesBuildPhase = project.SourceBuildPhases.Get("Sources", moduleName);
            var data = project.BuildFiles.Get(moduleName, fileRef, sourcesBuildPhase);
            if (null == data)
            {
                throw new Opus.Core.Exception("Build file not available");
            }

            Opus.Core.BaseOptionCollection complementOptionCollection = null;
            if (node.EncapsulatingNode.Module is Opus.Core.ICommonOptionCollection)
            {
                var commonOptions = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection;
                if (commonOptions is C.ICCompilerOptions)
                {
                    complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                }
            }

            if ((complementOptionCollection != null) && !complementOptionCollection.Empty)
            {
                // there is an option delta to write for this file
                Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
                CommandLineProcessor.ToCommandLine.Execute(complementOptionCollection, commandLineBuilder, target, null);

                if (commandLineBuilder.Count > 0)
                {
                    data.Settings["COMPILER_FLAGS"].AddRangeUnique(commandLineBuilder);
                }
            }
            else
            {
                // fill out the build configuration for the singular file
                var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
                XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, data, buildConfiguration, target);

                var basePath = Opus.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
#if true
                var outputDirLoc = moduleToBuild.Locations[C.ObjectFile.OutputDir];
                var relPath = Opus.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
#else
                var options = moduleToBuild.Options as C.CompilerOptionCollection;
                var relPath = Opus.Core.RelativePathUtilities.GetPath(options.OutputDirectoryPath, basePath);
#endif
                buildConfiguration.Options["CONFIGURATION_TEMP_DIR"].AddUnique("$SYMROOT/" + relPath);
                buildConfiguration.Options["TARGET_TEMP_DIR"].AddUnique("$CONFIGURATION_TEMP_DIR");

                // TODO: these should really be options in their own rights
#if true
                buildConfiguration.Options["ONLY_ACTIVE_ARCH"].AddUnique("YES");
                buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"].AddUnique("10.8");
                buildConfiguration.Options["SDKROOT"].AddUnique("macosx");

                if (target.HasToolsetType(typeof(LLVMGcc.Toolset)))
                {
                    if (target.Toolset.Version(baseTarget).StartsWith("4.2"))
                    {
                        buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvmgcc42");
                    }
                    else
                    {
                        throw new Opus.Core.Exception("Not supporting LLVM Gcc version {0}", target.Toolset.Version(baseTarget));
                    }
                }
                else if (target.HasToolsetType(typeof(Clang.Toolset)))
                {
                    buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvm.clang.1_0");
                }
                else
                {
                    throw new Opus.Core.Exception("Cannot identify toolchain {0}", target.ToolsetName('='));
                }
#endif
            }

            // add the source file to the configuration
            var config = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            config.SourceFiles.AddUnique(data);

            success = true;
            return data;
        }
    }
}
