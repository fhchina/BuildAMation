// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common linker options
#if false
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null);
#endif
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLine);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLine);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine);
            this["Libraries"].PrivateData = new PrivateData(null);
            this["StandardLibraries"].PrivateData = new PrivateData(null);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine);

            // linker specific options
            this["EnableAutoImport"].PrivateData = new PrivateData(EnableAutoImportCL);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this as C.ILinkerOptions).DoNotAutoIncludeStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line
            (this as ILinkerOptions).EnableAutoImport = false;

            // we use gcc as the linker - if there is C++ code included, link against libstdc++
            foreach (Opus.Core.DependencyNode child in node.Children)
            {
                if (child.Module is C.CPlusPlus.ObjectFile || child.Module is C.CPlusPlus.ObjectFileCollection)
                {
                    (this as C.ILinkerOptions).Libraries.Add("-lstdc++");
                    break;
                }
            }

            /*
             This is an example link line using gcc with -v
             
Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    string outputPathName = options.OutputFilePath;
                    if (outputPathName.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-o \"{0}\"", outputPathName));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPathName));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void DebugSymbolsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }

        // TODO: do these actually work?
        private static void SubSystemCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;

                case C.ESubsystem.Console:
                    commandLineBuilder.Add("-Wl,--subsystem,console");
                    break;

                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("-Wl,--subsystem,windows");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }

        private static void DoNotAutoIncludeStandardLibrariesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }

        private static void DynamicLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Add("-shared");
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (null != options.StaticImportLibraryFilePath)
                {
                    if (options.StaticImportLibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,--out-implib,\"{0}\"", options.StaticImportLibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,--out-implib,{0}", options.StaticImportLibraryFilePath));
                    }
                }
            }
        }

        private static void LibraryPathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-L{0}", libraryPath));
                }
            }
        }

        private static void GenerateMapFileCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (options.MapFilePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-Map,\"{0}\"", options.MapFilePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", options.MapFilePath));
                }
            }
        }

        private static void AdditionalOptionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }

        public static void EnableAutoImportCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wl,--enable-auto-import");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string outputPathName = this.OutputFilePath;
            if (null != outputPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(outputPathName), false);
            }

            string libraryPathName = this.StaticImportLibraryFilePath;
            if (null != libraryPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(libraryPathName), false);
            }

            return directoriesToCreate;
        }
    }
}