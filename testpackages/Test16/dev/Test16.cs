// Automatically generated by Opus v0.50
namespace Test16
{
    public class StaticLibrary2 : C.StaticLibrary
    {
        public StaticLibrary2()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");
        }

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            [C.ExportCompilerOptionsDelegate]
            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var cOptions = module.Options as C.ICCompilerOptions;
                if (null != cOptions)
                {
                    cOptions.IncludePaths.Include(this.PackageLocation, "include");
                }
            }
        }

        [C.HeaderFiles]
        Opus.Core.FileCollection headers = new Opus.Core.FileCollection();
        
        [Opus.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(Test15.StaticLibrary1)
            );
    }
}
