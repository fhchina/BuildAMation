[assembly:Opus.Core.GlobalOptionCollectionOverride(typeof(Test.OptionOverride))]

// Automatically generated by Opus v0.00
namespace Test
{
    class OptionOverride : Opus.Core.IGlobalOptionCollectionOverride
    {
        public void OverrideOptions(Opus.Core.BaseOptionCollection optionCollection, Opus.Core.Target target)
        {
            if (optionCollection is C.ICCompilerOptions)
            {
                C.ICCompilerOptions compilerOptions = optionCollection as C.ICCompilerOptions;
                compilerOptions.Defines.Add("GLOBALOVERRIDE");
            }
        }
    }

    sealed class TestCompile : C.ObjectFile
    {
        public TestCompile()
        {
            this.SetRelativePath(this, "source", "main.c");
            this.UpdateOptions += UpdateCompilerOptions;
        }

        private static void UpdateCompilerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;

            compilerOptions.WarningsAsErrors = false;

            string toolchain = target.Toolchain;
            if ("mingw" == toolchain)
            {
                Opus.Core.Log.MessageAll("mingw is the toolchain");

                if (Opus.Core.EConfiguration.Debug == target.Configuration)
                {
                    compilerOptions.Optimization = C.EOptimization.Custom;
                }

                compilerOptions.AdditionalOptions = "-Wall";

                MingwCommon.ICCompilerOptions mingwCompilerOptions = compilerOptions as MingwCommon.ICCompilerOptions;
                mingwCompilerOptions.InlineFunctions = true;
            }
            else if ("visualc" == toolchain)
            {
                Opus.Core.Log.MessageAll("visualc is the toolchain");

                compilerOptions.Optimization = C.EOptimization.Custom;
                compilerOptions.CustomOptimization = "/Ox";
                compilerOptions.AdditionalOptions = "/openmp";

                compilerOptions.DebugSymbols = true;
                VisualCCommon.ICCompilerOptions vcCompilerOptions = compilerOptions as VisualCCommon.ICCompilerOptions;
                vcCompilerOptions.DebugType = VisualCCommon.EDebugType.Embedded;
                vcCompilerOptions.BasicRuntimeChecks = VisualCCommon.EBasicRuntimeChecks.None;
                vcCompilerOptions.SmallerTypeConversionRuntimeCheck = false;
            }
            else if ("gcc" == toolchain)
            {
                Opus.Core.Log.MessageAll("gcc is the toolchain");
                compilerOptions.AdditionalOptions = "-Wall";

                GccCommon.ICCompilerOptions gccCompilerOptions = compilerOptions as GccCommon.ICCompilerOptions;
                gccCompilerOptions.PositionIndependentCode = true;
            }
            else
            {
                Opus.Core.Log.MessageAll("Unknown toolchain");
            }
        }
    }

    sealed class TestCompile2 : C.ObjectFile
    {
        public TestCompile2()
        {
            this.SetRelativePath(this, "source", "main.c");
        }
    }

    sealed class TestCompile3 : C.ObjectFileCollection
    {
        public TestCompile3()
        {
            this.AddRelativePaths(this, "source", "*.c");

            this.UpdateOptions += OverrideOptionCollection;

            // override the options on one specific file
            Opus.Core.IModule mainObjFile = this.GetChildModule(this, "source", "main.c");
            if (null != mainObjFile)
            {
                mainObjFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(mainObjFile_UpdateOptions);
            }
        }

        void mainObjFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.Defines.Add("DEFINE_FOR_MAIN_ONLY");
        }

        public void OverrideOptionCollection(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.WarningsAsErrors = false;
            compilerOptions.Defines.Add("DEFINE_FOR_ALL_SOURCE");
        }
    }

    sealed class TestApplication1 : C.Application
    {
        sealed class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.AddRelativePaths(this, "source", "main.c");

                this.UpdateOptions += OverrideOptionCollection;
            }

            private static void OverrideOptionCollection(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.WarningsAsErrors = false;
                //options.CharacterSet = C.Compiler.ECharacterSet.NotSet;

                VisualC.CCompilerOptionCollection vcOptions = compilerOptions as VisualC.CCompilerOptionCollection;
                if (null != vcOptions)
                {
                    //vcOptions.DebugType = VisualC.EDebugType.Embedded;
                }
                Mingw.CCompilerOptionCollection mingwOptions = compilerOptions as Mingw.CCompilerOptionCollection;
                if (null != mingwOptions)
                {
                }
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc" })]
        Opus.Core.Array<System.Type> dependents = new Opus.Core.Array<System.Type>(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

    [Opus.Core.ModuleTargets(Platform=Opus.Core.EPlatform.Windows)]
    sealed class TestWindowsApplication1 : C.WindowsApplication
    {
        sealed class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.AddRelativePaths(this, "source", "main.c");
            }
        }

        sealed class Win32Resources : C.Win32Resource
        {
            public Win32Resources()
            {
                this.ResourceFile.SetRelativePath(this, "resources", "win32.rc");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows)]
        Opus.Core.TypeArray mingwDependents = new Opus.Core.TypeArray(
            typeof(Win32Resources)
            );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }
}
