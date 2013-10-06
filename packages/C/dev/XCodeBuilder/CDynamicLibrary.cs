// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(C.DynamicLibrary moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var outputPath = options.OutputPaths[C.OutputFileFlags.Executable];

            var fileRef = this.Project.FileReferences.Get(moduleName, PBXFileReference.EType.DynamicLibrary, outputPath, this.ProjectRootUri);
            this.Project.ProductsGroup.Children.AddUnique(fileRef);

            var data = this.Project.NativeTargets.Get(moduleName, PBXNativeTarget.EType.DynamicLibrary);
            data.ProductReference = fileRef;

            // gather up all the source files for this target
            foreach (var childNode in node.Children)
            {
                if (childNode.Module is C.ObjectFileCollectionBase)
                {
                    foreach (var objectFile in childNode.Children)
                    {
                        data.SourceFilesToBuild.AddUnique(objectFile.Data as PBXBuildFile);
                    }
                }
                else
                {
                    data.SourceFilesToBuild.AddUnique(childNode.Data as PBXBuildFile);
                }
            }

            // build configuration target overrides to the project build configuration
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            var nativeTargetConfigurationList = this.Project.ConfigurationLists.Get(data);
            nativeTargetConfigurationList.AddUnique(buildConfiguration);
            if (null == data.BuildConfigurationList)
            {
                data.BuildConfigurationList = nativeTargetConfigurationList;
            }
            else
            {
                if (data.BuildConfigurationList != nativeTargetConfigurationList)
                {
                    throw new Opus.Core.Exception("Inconsistent build configuration lists");
                }
            }

            // fill out the build configuration
            XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, this.Project, data, buildConfiguration, target);

            buildConfiguration.Options["PRODUCT_NAME"].AddUnique(options.OutputName);

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var outputPrefix = linkerTool.DynamicLibraryPrefix;
            var outputSuffix = linkerTool.DynamicLibrarySuffix;
            buildConfiguration.Options["EXECUTABLE_PREFIX"].AddUnique(outputPrefix);
            buildConfiguration.Options["EXECUTABLE_SUFFIX"].AddUnique(outputSuffix);

            buildConfiguration.Options["LD_DYLIB_INSTALL_NAME"].AddUnique("@loader_path/$(EXECUTABLE_PATH)");

            var basePath = Opus.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var relPath = Opus.Core.RelativePathUtilities.GetPath(options.OutputDirectoryPath, basePath);
            buildConfiguration.Options["CONFIGURATION_BUILD_DIR"].AddUnique("$SYMROOT/" + relPath);

            // adding the group for the target
            var group = this.Project.Groups.Get(moduleName);
            group.SourceTree = "<group>";
            group.Path = moduleName;
            foreach (var source in node.Children)
            {
                if (source.Module is Opus.Core.IModuleCollection)
                {
                    foreach (var source2 in source.Children)
                    {
                        var sourceData = source2.Data as PBXBuildFile;
                        group.Children.AddUnique(sourceData.FileReference);
                    }
                }
                else
                {
                    var sourceData = source.Data as PBXBuildFile;
                    group.Children.AddUnique(sourceData.FileReference);
                }
            }
            this.Project.MainGroup.Children.AddUnique(group);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleName);
            data.BuildPhases.AddUnique(sourcesBuildPhase);

            var copyFilesBuildPhase = this.Project.CopyFilesBuildPhases.Get("CopyFiles", moduleName);
            data.BuildPhases.AddUnique(copyFilesBuildPhase);

            var frameworksBuildPhase = this.Project.FrameworksBuildPhases.Get("Frameworks", moduleName);
            data.BuildPhases.AddUnique(frameworksBuildPhase);

            if (null != node.ExternalDependents)
            {
                foreach (var dependency in node.ExternalDependents)
                {
                    // first add a dependency so that they are built in the right order
                    var dependentData = dependency.Data as PBXNativeTarget;
                    if (null == dependentData)
                    {
                        continue;
                    }

                    var targetDependency = this.Project.TargetDependencies.Get(moduleName, dependentData);

                    var containerItemProxy = this.Project.ContainerItemProxies.Get(moduleName, dependentData, this.Project);
                    targetDependency.TargetProxy = containerItemProxy;

                    data.Dependencies.Add(targetDependency);

                    // now add a link dependency
                    var buildFile = this.Project.BuildFiles.Get(dependency.UniqueModuleName, dependentData.ProductReference);
                    buildFile.BuildPhase = frameworksBuildPhase;

                    frameworksBuildPhase.Files.AddUnique(buildFile);

                    // now add linker search paths
                    if (dependency.Module is C.DynamicLibrary)
                    {
                        buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(System.IO.Path.GetDirectoryName(dependency.Module.Options.OutputPaths[C.OutputFileFlags.Executable]));
                    }
                    else if (dependency.Module is C.StaticLibrary)
                    {
                        buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(System.IO.Path.GetDirectoryName(dependency.Module.Options.OutputPaths[C.OutputFileFlags.StaticLibrary]));
                    }
                }
            }

            if (null != node.RequiredDependents)
            {
                // no link dependency
                foreach (var dependency in node.RequiredDependents)
                {
                    var dependentData = dependency.Data as PBXNativeTarget;
                    if (null == dependentData)
                    {
                        continue;
                    }

                    var targetDependency = this.Project.TargetDependencies.Get(moduleName, dependentData);

                    var containerItemProxy = this.Project.ContainerItemProxies.Get(moduleName, dependentData, this.Project);
                    targetDependency.TargetProxy = containerItemProxy;

                    data.Dependencies.Add(targetDependency);
                }
            }

            // find header files
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Opus.Core.FileCollection;
                    foreach (string headerPath in headerFileCollection)
                    {
                        var headerFileRef = this.Project.FileReferences.Get(moduleName, PBXFileReference.EType.HeaderFile, headerPath, this.ProjectRootUri);
                        group.Children.AddUnique(headerFileRef);
                    }
                }
            }

            success = true;
            return data;
        }
    }
}