// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
#if true
        private void
        nativeCopyNodeLocation(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context)
        {
            var moduleToCopy = meta.Node.Module;
            var moduleLocations = moduleToCopy.Locations;

            var sourceKey = nodeInfo.Key;
            if (!moduleLocations.Contains(sourceKey))
            {
                return;
            }

            var sourceLoc = moduleLocations[sourceKey];
            if (!sourceLoc.IsValid)
            {
                return;
            }

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }

#if false
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);
#endif

            if (sourceKey.IsFileKey)
            {
#if false
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                Publisher.ProductModuleUtilities.CopyFileToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
#endif
            }
            else if (sourceKey.IsSymlinkKey)
            {
#if false
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                Publisher.ProductModuleUtilities.CopySymlinkToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
#endif
            }
            else if (sourceKey.IsDirectoryKey)
            {
                throw new Opus.Core.Exception("Directories cannot be published yet");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported Location type");
            }
        }

        private void
        nativeCopyAdditionalDirectory(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath,
            object context)
        {
#if false
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirectoryPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
#endif
        }

        private void
        nativeCopyInfoPList(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context)
        {
#if false
            var plistNode = Opus.Core.ModuleUtilities.GetNode(
                namedLocation.ModuleType,
                (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);

            var moduleToCopy = plistNode.Module;
            var keyToCopy = namedLocation.Key;

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var contentsLoc = moduleToBuild.Locations[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
            var plistSourceLoc = moduleToCopy.Locations[keyToCopy];
            Publisher.ProductModuleUtilities.CopyFileToLocation(
                plistSourceLoc,
                contentsLoc,
                string.Empty,
                moduleToBuild,
                publishedKey);
#endif
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList,
                null);

            success = true;
            return null;
        }
#else
        private static void
        CopyFilesToDirectory(
            Opus.Core.BaseModule module,
            Opus.Core.DependencyNode primaryNode,
            string sourcePath,
            string destinationDirectory,
            QMakeData proData)
        {
#if true
            // TOOD: if there is only one place to write to, use this
            var targetName = (module is C.DynamicLibrary) ? "dlltarget" : "target";
            var customRules = new Opus.Core.StringArray();
            customRules.Add(System.String.Format("{0}.path={1}", targetName, destinationDirectory));
            customRules.Add(System.String.Format("INSTALLS+={0}", targetName));
#else
            // otherwise, if there are multiple places to install to, use this
            // Note: don't use absolute paths, unless they exist already - cannot refer to files that are to be built
            var targetName = System.String.Format("copy_{0}_for_{1}", module.OwningNode.ModuleName, primaryNode.ModuleName);
            var customRules = new Opus.Core.StringArray();
            customRules.Add(System.String.Format("{0}.path={1}", targetName, destinationDirectory.Replace('\\', '/')));
            // TODO: this does not resolve to the final destination path
            customRules.Add(System.String.Format("{0}.files=$${{DESTDIR}}/$${{TARGET}}.$${{TARGET_EXT})", targetName));
            customRules.Add(System.String.Format("INSTALLS+={0}", targetName));
#endif
            if (null == proData.CustomRules)
            {
                proData.CustomRules = customRules;
            }
            else
            {
                proData.CustomRules.AddRangeUnique(customRules);
            }
        }

        private static void
        CopyDirectoryToDirectory(
            Opus.Core.BaseModule module,
            Opus.Core.DependencyNode primaryNode,
            Opus.Core.Location sourceLoc,
            string destinationDirectory,
            QMakeData proData)
        {
            // TODO:
            // TODO: Mac app bundles need to be catered for
            Opus.Core.Log.MessageAll("Copy dir '{0}' to '{1}'", sourceLoc.GetSingleRawPath(), destinationDirectory);
        }

        private void
        PublishDependents(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath)
        {
            var dependents = new Opus.Core.DependencyNodeCollection();
            if (null != primaryNode.ExternalDependents)
            {
                dependents.AddRange(primaryNode.ExternalDependents);
            }
            if (null != primaryNode.RequiredDependents)
            {
                dependents.AddRange(primaryNode.RequiredDependents);
            }

            foreach (var dependency in dependents)
            {
                var proData = dependency.Data as QMakeData;
                if (null == proData)
                {
                    continue;
                }

                var module = dependency.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.CopyFileLocationsAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var attribute = candidates[0] as Publisher.CopyFileLocationsAttribute;
                    var matchesTarget = Opus.Core.TargetUtilities.MatchFilters(moduleToBuild.OwningNode.Target, attribute);
                    if (!matchesTarget)
                    {
                        continue;
                    }
                    var candidateData = field.GetValue(module) as Opus.Core.Array<Opus.Core.LocationKey>;
                    if (null != candidateData)
                    {
                        foreach (var key in candidateData)
                        {
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            var sourceLoc = module.Locations[key];
                            if (!sourceLoc.IsValid)
                            {
                                continue;
                            }
                            var sourcePath = sourceLoc.GetSingleRawPath();

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                                primaryNode.Module,
                                module,
                                key);
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                sourcePath,
                                publishDirPath,
                                string.Empty,
                                string.Empty,
                                moduleToBuild,
                                newKey);
                            var destDir = System.IO.Path.GetDirectoryName(destPath);

                            CopyFilesToDirectory(module, primaryNode, sourcePath, destDir, proData);
                        }
                    }
                    else
                    {
                        var candidateData2 = field.GetValue(module) as Opus.Core.Array<Publisher.PublishDependency>;
                        if (null == candidateData2)
                        {
                            throw new Opus.Core.Exception("Unrecognized type for dependency data");
                        }

                        foreach (var dep in candidateData2)
                        {
                            var key = dep.Key;
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            var sourceLoc = module.Locations[key];
                            if (!sourceLoc.IsValid)
                            {
                                continue;
                            }
                            var sourcePath = sourceLoc.GetSingleRawPath();

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                                primaryNode.Module,
                                module,
                                key);
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                sourcePath,
                                publishDirPath,
                                subDirectory,
                                string.Empty,
                                moduleToBuild,
                                newKey);
                            var destDir = System.IO.Path.GetDirectoryName(destPath);

                            CopyFilesToDirectory(module, primaryNode, sourcePath, destDir, proData);
                        }
                    }
                }
            }
        }

        private void
        PublishAdditionalDirectories(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath)
        {
            var additionalDirsData = Publisher.ProductModuleUtilities.GetAdditionalDirectoriesData(moduleToBuild);
            if (null != additionalDirsData)
            {
                var sourceLoc = additionalDirsData.SourceDirectory;
                CopyDirectoryToDirectory(moduleToBuild, primaryNode, sourceLoc, publishDirPath, primaryNode.Data as QMakeData);
            }
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var options = moduleToBuild.Options as Publisher.IPublishOptions;

            var primaryNodeData = Publisher.ProductModuleUtilities.GetPrimaryNodeData(moduleToBuild);
            if (null == primaryNodeData)
            {
                success = true;
                return null;
            }

            var primaryNode = primaryNodeData.Node;
            if (options.OSXApplicationBundle)
            {
                var data = primaryNode.Data as QMakeData;
                data.OSXApplicationBundle = true;
            }

            var locationMap = primaryNode.Module.Locations;
            var publishDirLoc = (locationMap[primaryNodeData.Key] as Opus.Core.ScaffoldLocation).Base;
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            this.PublishDependents(moduleToBuild, primaryNode, publishDirPath);
            this.PublishAdditionalDirectories(moduleToBuild, primaryNode, publishDirPath);

            success = true;
            return null;
        }
#endif
    }
}
