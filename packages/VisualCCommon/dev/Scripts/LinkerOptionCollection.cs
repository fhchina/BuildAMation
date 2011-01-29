// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions, VisualStudioProcessor.IVisualStudioSupport//, Opus.Core.IOutputPaths
    {
        public enum EOutputFile
        {
            OutputFile              = (1<<0),
            StaticImportLibraryFile = (1<<1),
            MapFile                 = (1<<2),
            ProgramDatabaseFile     = (1<<3)
        }

        private void SetDelegates(Opus.Core.Target target)
        {
            // common linker options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(ToolchainOptionCollectionCommandLine, ToolchainOptionCollectionVisualStudio);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine, OutputTypeVisualStudio);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine, DebugSymbolsVisualStudio);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLine, SubSystemVisualStudio);
            this["IgnoreStandardLibraries"].PrivateData = new PrivateData(IgnoreStandardLibrariesCommandLine, IgnoreStandardLibrariesVisualStudio);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine, DynamicLibraryVisualStudio);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine, LibraryPathsVisualStudio);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLine, StandardLibrariesVisualStudio);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLine, LibrariesVisualStudio);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine, GenerateMapFileVisualStudio);

            // linker specific options
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            this.NoLogo = true;

            Opus.Core.Target target = node.Target;

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.LibraryPaths.Add(toolChainInstance.LibPath(target), true);

            this.SetDelegates(target);
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override string OutputFilePath
        {
            get
            {
                if (this.OutputPaths.Has(EOutputFile.OutputFile))
                {
                    return this.OutputPaths[EOutputFile.OutputFile];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.OutputPaths[EOutputFile.OutputFile] = value;
                }
                else if (this.OutputPaths.Has(EOutputFile.OutputFile))
                {
                    this.OutputPaths.Remove(EOutputFile.OutputFile);
                }
            }
        }

        public override string StaticImportLibraryFilePath
        {
            get
            {
                if (this.OutputPaths.Has(EOutputFile.StaticImportLibraryFile))
                {
                    return this.OutputPaths[EOutputFile.StaticImportLibraryFile];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    this.OutputPaths[EOutputFile.StaticImportLibraryFile] = value;
                }
                else if (this.OutputPaths.Has(EOutputFile.StaticImportLibraryFile))
                {
                    this.OutputPaths.Remove(EOutputFile.StaticImportLibraryFile);
                }
            }
        }

        public override string MapFilePath
        {
            get
            {
                if (this.OutputPaths.Has(EOutputFile.MapFile))
                {
                    return this.OutputPaths[EOutputFile.MapFile];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.OutputPaths[EOutputFile.MapFile] = value;
                }
                else if (this.OutputPaths.Has(EOutputFile.MapFile))
                {
                    this.OutputPaths.Remove(EOutputFile.MapFile);
                }
            }
        }

        public string ProgramDatabaseFilePath
        {
            get
            {
                if (this.OutputPaths.Has(EOutputFile.ProgramDatabaseFile))
                {
                    return this.OutputPaths[EOutputFile.ProgramDatabaseFile];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    this.OutputPaths[EOutputFile.ProgramDatabaseFile] = value;
                }
                else if (this.OutputPaths.Has(EOutputFile.ProgramDatabaseFile))
                {
                    this.OutputPaths.Remove(EOutputFile.ProgramDatabaseFile);
                }
            }
        }

        private static void ToolchainOptionCollectionCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            RuntimeLibraryCommandLine(sender, commandLineBuilder, toolchainOptions.Value["RuntimeLibrary"], target);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ToolchainOptionCollectionVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            return RuntimeLibraryVisualStudio(sender, toolchainOptions.Value["RuntimeLibrary"], target);
        }

        protected static void OutputTypeSetHandler(object sender, Opus.Core.Option option)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    {
                        string executablePathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".exe";
                        options.OutputFilePath = executablePathname;
                    }
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    {
                        string dynamicLibraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".dll";
                        string importLibraryPathname = System.IO.Path.Combine(options.LibraryDirectoryPath, options.OutputName) + ".lib";
                        options.OutputFilePath = dynamicLibraryPathname;
                        options.StaticImportLibraryFilePath = importLibraryPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    commandLineBuilder.Append(System.String.Format("/OUT:\"{0}\" ", options.OutputFilePath));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        dictionary.Add("OutputFile", options.OutputFilePath);
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        protected static void DebugSymbolsSetHandler(object sender, Opus.Core.Option option)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                string pdbPathName = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".pdb";
                options.ProgramDatabaseFilePath = pdbPathName;
            }
            else
            {
                options.ProgramDatabaseFilePath = null;
            }
        }

        private static void DebugSymbolsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Append("/DEBUG ");

                LinkerOptionCollection options = sender as LinkerOptionCollection;
                commandLineBuilder.Append(System.String.Format("/PDB:\"{0}\" ", options.ProgramDatabaseFilePath));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DebugSymbolsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
            if (debugSymbolsOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                dictionary.Add("ProgramDatabaseFile", options.ProgramDatabaseFilePath);
            }
            return dictionary;
        }

        private static void NoLogoCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Append("/NOLOGO ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void SubSystemCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;

                case C.ESubsystem.Console:
                    commandLineBuilder.Append("/SUBSYSTEM:CONSOLE ");
                    break;

                case C.ESubsystem.Windows:
                    commandLineBuilder.Append("/SUBSYSTEM:WINDOWS ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary SubSystemVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                case C.ESubsystem.Console:
                case C.ESubsystem.Windows:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        dictionary.Add("SubSystem", subSystemOption.Value.ToString("D"));
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }

        private static void IgnoreStandardLibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Append("/NODEFAULTLIB ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IgnoreStandardLibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("IgnoreAllDefaultLibraries", ignoreStandardLibrariesOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void RuntimeLibraryCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = options.ToolchainOptionCollection as C.IToolchainOptions;
            bool isCPlusPlus = toolchainOptions.IsCPlusPlus;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMTD.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLLDebug:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRTD.lib");
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = options.ToolchainOptionCollection as C.IToolchainOptions;
            bool isCPlusPlus = toolchainOptions.IsCPlusPlus;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMTD.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLLDebug:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRTD.lib");
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
            return null;
        }

        private static void DynamicLibraryCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Append("/DLL ");
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (null != options.StaticImportLibraryFilePath)
                {
                    commandLineBuilder.AppendFormat("/IMPLIB:\"{0}\" ", options.StaticImportLibraryFilePath); 
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DynamicLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (null != options.StaticImportLibraryFilePath)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                dictionary.Add("ImportLibrary", options.StaticImportLibraryFilePath);
                return dictionary;
            }
            else
            {
                return null;
            }
        }

        private static void LibraryPathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                commandLineBuilder.AppendFormat("/LIBPATH:\"{0}\" ", includePath);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary LibraryPathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            System.Text.StringBuilder libraryPaths = new System.Text.StringBuilder();
            foreach (string includePath in includePathsOption.Value)
            {
                libraryPaths.Append(System.String.Format("\"{0}\";", includePath));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalLibraryDirectories", libraryPaths.ToString());
            return dictionary;
        }

        private static void StandardLibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (options.IgnoreStandardLibraries)
            {
                Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
                foreach (string includePath in includePathsOption.Value)
                {
                    commandLineBuilder.AppendFormat("\"{0}\" ", includePath);
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary StandardLibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (options.IgnoreStandardLibraries)
            {
                Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> standardLibraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
                System.Text.StringBuilder standardLibraryPaths = new System.Text.StringBuilder();
                // this stops any other libraries from being inherited
                standardLibraryPaths.Append("$(NOINHERIT) ");
                foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                {
                    standardLibraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
                }
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                dictionary.Add("AdditionalDependencies", standardLibraryPaths.ToString());
                return dictionary;
            }
            else
            {
                return null;
            }
        }

        private static void LibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> librariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            foreach (string libraryPath in librariesOption.Value)
            {
                commandLineBuilder.AppendFormat("\"{0}\" ", libraryPath);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary LibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            System.Text.StringBuilder libraryPaths = new System.Text.StringBuilder();
            // this stops any other libraries from being inherited
            libraryPaths.Append("$(NOINHERIT) ");
            foreach (string standardLibraryPath in libraryPathsOption.Value)
            {
                libraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalDependencies", libraryPaths.ToString());
            return dictionary;
        }

        protected static void GenerateMapFileSetHandler(object sender, Opus.Core.Option option)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                string mapPathName = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".map";
                options.MapFilePath = mapPathName;
            }
            else
            {
                options.MapFilePath = null;
            }
        }

        private static void GenerateMapFileCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                commandLineBuilder.AppendFormat("/MAP:\"{0}\" ", options.MapFilePath);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary GenerateMapFileVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("GenerateMapFile", boolOption.Value.ToString().ToLower());
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                dictionary.Add("MapFileName", options.MapFilePath);
            }
            return dictionary;
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.OutputFilePath), false);
            }
            if (null != this.StaticImportLibraryFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.StaticImportLibraryFilePath), false);
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target);
            return dictionary;
        }
    }
}