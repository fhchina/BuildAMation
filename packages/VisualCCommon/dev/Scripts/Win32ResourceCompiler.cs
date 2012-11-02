// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // NEW STYLE
#if true
    sealed class Win32ResourceCompiler : C.IWinResourceCompilerTool
    {
        private Opus.Core.IToolset toolset;

        public Win32ResourceCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region IWinResourceCompilerTool Members

        string C.IWinResourceCompilerTool.CompiledResourceSuffix
        {
            get
            {
                return ".res";
            }
        }

        string C.IWinResourceCompilerTool.InputFileSwitch
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IWinResourceCompilerTool.OutputFileSwitch
        {
            get
            {
                return "/fo ";
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TODO: would like a better way of doing this
            string platformBinFolder = WindowsSDK.WindowsSDK.BinPath(target);
            return System.IO.Path.Combine(platformBinFolder, "rc.exe");
        }

        #endregion
    }
#else
    sealed class Win32ResourceCompiler : C.Win32ResourceCompilerBase
    {
        private string platformBinFolder;

        public Win32ResourceCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            this.platformBinFolder = WindowsSDK.WindowsSDK.BinPath(target);
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "rc.exe");
        }

        public override string InputFileSwitch
        {
            get
            {
                return string.Empty;
            }
        }

        public override string OutputFileSwitch
        {
            get
            {
                return "/fo ";
            }
        }
    }
#endif
}
