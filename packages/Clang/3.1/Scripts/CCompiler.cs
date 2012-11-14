// <copyright file="CCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CCompiler : C.ICompilerTool
    {
        private Opus.Core.IToolset toolset;

        public CCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TODO: can we have this file extension somewhere central?
            return System.IO.Path.Combine(this.toolset.InstallPath((Opus.Core.BaseTarget)target), "clang.exe");
        }

        #endregion

#if false
        private Opus.Core.IToolset toolset = Opus.Core.ToolsetFactory.CreateToolset(typeof(Clang.Toolset));

        public CCompiler(Opus.Core.Target target)
        {
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TODO: can we have this extension somewhere central?
            return System.IO.Path.Combine(this.toolset.InstallPath((Opus.Core.BaseTarget)target), "clang.exe");
        }

        #endregion

        #region ICompiler Members

        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        Opus.Core.StringArray C.ICompiler.IncludePathCompilerSwitches
        {
            get
            {
                Opus.Core.StringArray switches = new Opus.Core.StringArray();
                switches.Add("-I");
                return switches;
            }
        }

        #endregion
#endif
    }
}
