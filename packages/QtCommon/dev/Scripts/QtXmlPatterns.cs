// <copyright file="QtXmlPatternsPatterns.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class XmlPatterns : Base
    {
        public XmlPatterns()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
#if true
            this.Locations[C.DynamicLibrary.OutputFile] = this.GetModuleDynamicLibrary(target, "QtXmlPatternsPatterns");
#else
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(target, "QtXmlPatternsPatterns");
#endif
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtXmlPatternsPatterns_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtXmlPatternsPatterns");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtXmlPatternsPatterns_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtXmlPatternsPatterns headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtXmlPatternsPatterns_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtXmlPatternsPatterns");
            }
        }
    }
}
