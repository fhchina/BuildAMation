// Automatically generated by Opus v0.10
namespace Test14
{
    // Define module classes here
    class DynamicLibraryA : C.DynamicLibrary
    {
        public DynamicLibraryA()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "dynamicLibraryA.c");
            this.source.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(DynamicLibraryA_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(DynamicLibraryA_UpdateOptions);

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.header.Include(includeDir, "dynamicLibraryA.h");
        }

        [C.ExportCompilerOptionsDelegate]
        void DynamicLibraryA_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void DynamicLibraryA_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [C.HeaderFiles]
        Opus.Core.FileCollection header = new Opus.Core.FileCollection();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    class DynamicLibraryB : C.DynamicLibrary
    {
        public DynamicLibraryB()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "dynamicLibraryB.c");
            this.source.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(DynamicLibraryB_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(DynamicLibraryB_UpdateOptions);

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.header.Include(includeDir, "dynamicLibraryB.h");
        }

        void DynamicLibraryB_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void DynamicLibraryB_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [C.HeaderFiles]
        Opus.Core.FileCollection header = new Opus.Core.FileCollection();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(DynamicLibraryA));

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    class Application : C.Application
    {
        public Application()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "main.c");
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Application_UpdateOptions);
        }

        void Application_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(DynamicLibraryA),
            typeof(DynamicLibraryB)
        );
    }

#if false
    // TODO: rework with publishing
#if OPUSPACKAGE_FILEUTILITIES_DEV
    class PublishDynamicLibraries : FileUtilities.CopyFileCollection
    {
        public PublishDynamicLibraries(Opus.Core.Target target)
        {
            this.Include(target,
                         C.OutputFileFlags.Executable,
                         typeof(DynamicLibraryA),
                         typeof(DynamicLibraryB));
        }

        [FileUtilities.BesideModule(C.OutputFileFlags.Executable)]
        System.Type nextTo = typeof(Application);
    }
#elif OPUSPACKAGE_FILEUTILITIES_1_0
    class PublishDynamicLibraries : FileUtilities.CopyFiles
    {
        [FileUtilities.DestinationModuleDirectory(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray destinationModule = new Opus.Core.TypeArray(typeof(Application));

        [FileUtilities.SourceModules(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray sourceModules = new Opus.Core.TypeArray(
            typeof(DynamicLibraryA),
            typeof(DynamicLibraryB)
        );
    }
#else
#error Unknown FileUtilities package version
#endif
#endif
}
