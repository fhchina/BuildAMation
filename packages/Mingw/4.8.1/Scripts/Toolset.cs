// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class Toolset : MingwCommon.Toolset
    {
        public Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Opus.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Opus.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)] = new Opus.Core.ToolAndOptionType(new Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Opus.Core.ToolAndOptionType(new MingwCommon.Archiver(this), typeof(ArchiverOptionCollection));
            this.toolConfig[typeof(C.IWinResourceCompilerTool)] = new Opus.Core.ToolAndOptionType(new MingwCommon.Win32ResourceCompiler(this), typeof(C.Win32ResourceCompilerOptionCollection));
        }

        protected override void GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("Mingw") && Opus.Core.State.Has("Mingw", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("Mingw", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Mingw install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                var installPath = @"c:\Mingw"; // standard default
                if (!System.IO.Directory.Exists(installPath))
                {
                    throw new Opus.Core.Exception("Default install location '{0} does not exist", installPath);
                }

                this.installPath = installPath;
            }

            this.binPath = System.IO.Path.Combine(this.installPath, "bin");
            this.environment.Add(this.binPath);

            this.details = MingwCommon.MingwDetailGatherer.DetermineSpecs(baseTarget, this);
        }
    }
}