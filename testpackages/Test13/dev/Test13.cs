// Automatically generated by Opus v0.00
namespace Test13
{
    // Define module classes here
    class QtApplication : C.Application
    {
#if false
        private const string WinTarget = "win.*-.*-.*";
        private const string WinVCTarget = "win.*-.*-visualc";
        private const string WinVCDebugTarget = "win.*-debug-visualc";
        private const string WinVCOptimizedTarget = "win.*-optimized-visualc";
        private const string WinMingwTarget = "win.*-.*-mingw";
        private const string WinMingwDebugTarget = "win.*-debug-mingw";
        private const string WinMingwOptimizedTarget = "win.*-optimized-mingw";
        private const string UnixGccTarget = "unix.*-.*-gcc";
#endif

        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.AddRelativePaths(this, "source", "*.cpp");
            }

            /*
            class MyMocFile : Qt.MocFile
            {
                public MyMocFile()
                {
                    this.SetRelativePath(this, "source", "myobject.h");
                }
            }
             */
            class MyMocFiles : Qt.MocFileCollection
            {
                public MyMocFiles()
                {
                    this.AddRelativePaths(this, "source", "*.h");
                }
            }

            [Opus.Core.DependentModules]
            //Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(SourceFiles.MyMocFile));
            Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(SourceFiles.MyMocFiles));
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(Qt.Qt));

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc" })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform=Opus.Core.EPlatform.Windows, Configuration=Opus.Core.EConfiguration.Debug, Toolchains=new string[] { "mingw" })]
        Opus.Core.StringArray winMingwDebugLibraries = new Opus.Core.StringArray(
            "-lQtCored4",
            "-lQtGuid4"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Configuration = Opus.Core.EConfiguration.All & ~Opus.Core.EConfiguration.Debug, Toolchains = new string[] { "mingw" })]
        Opus.Core.StringArray winMingwOptimizedLibraries = new Opus.Core.StringArray(
            "-lQtCore4",
            "-lQtGui4"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray winVCLibraries = new Opus.Core.StringArray("KERNEL32.lib");

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Configuration=Opus.Core.EConfiguration.Debug, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray winVCDebugLibraries = new Opus.Core.StringArray(
            "QtCored4.lib",
            "QtGuid4.lib"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Configuration = Opus.Core.EConfiguration.All & ~Opus.Core.EConfiguration.Debug, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray winVCOptimizedLibraries = new Opus.Core.StringArray(
            "QtCore4.lib",
            "QtGui4.lib"
        );

        [C.RequiredLibraries(Platform=Opus.Core.EPlatform.Unix, Toolchains=new string[] { "gcc" })]
        Opus.Core.StringArray unixGCCLibraries = new Opus.Core.StringArray(
            "-lQtCore",
            "-lQtGui"
        );
    }

    class PublishDynamicLibraries : FileUtilities.CopyFiles
    {
        public PublishDynamicLibraries(Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                if (Opus.Core.EConfiguration.Debug == target.Configuration)
                {
                    this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "QtCored4.dll");
                    this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "QtGuid4.dll");
                }
                else
                {
                    this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "QtCore4.dll");
                    this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "QtGui4.dll");
                }
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "libQtCore.so");
                this.sourceFiles.AddRelativePaths(Qt.Qt.BinPath, "libQtGui.so");
            }
        }

        [Opus.Core.SourceFiles]
        Opus.Core.FileCollection sourceFiles = new Opus.Core.FileCollection();

        [FileUtilities.DestinationModuleDirectory(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray destinationTarget = new Opus.Core.TypeArray(typeof(QtApplication));
    }
}
 