// Automatically generated by Opus v0.00

namespace Test4
{
    // Define module classes here
    class MyDynamicLib : C.DynamicLibrary
    {
        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "dynamiclibrary.c");
                this.UpdateOptions += SetIncludePaths;
                this.UpdateOptions += SetRuntimeLibrary;
            }

            [C.ExportCompilerOptionsDelegate]
            private void SetIncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this, "include");
            }

            [C.ExportCompilerOptionsDelegate]
            private static void SetRuntimeLibrary(Opus.Core.IModule module, Opus.Core.Target target)
            {
                VisualCCommon.ICCompilerOptions vcCompilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (vcCompilerOptions != null)
                {
                    vcCompilerOptions.RuntimeLibrary = VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL;
                }
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(MyStaticLib));

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[]{"visualc"})]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

    class MyStaticLib : C.StaticLibrary
    {
        public MyStaticLib()
        {
            this.sourceFile.SetRelativePath(this, "source", "staticlibrary.c");
            this.sourceFile.UpdateOptions += SetIncludePaths;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [C.ExportCompilerOptionsDelegate]
        private void SetIncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this, "include");
        }
    }
}
