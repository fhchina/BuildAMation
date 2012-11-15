// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // NEW STYLE
#if true
    public sealed class Linker : MingwCommon.Linker
    {
        public Linker(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "mingw32-gcc-3.4.5";
            }
        }
    }
#else
    public sealed class Linker : MingwCommon.Linker, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string binPath;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target))
            {
                throw new Opus.Core.Exception("Mingw linker is only supported under win32 and win64 platforms");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            this.binPath = info.BinPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
#endif

            this.requiredEnvironmentVariables.Add("TEMP");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "mingw32-gcc-3.4.5");
        }

        // OLD STYLE
#if false
        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "mingw32-g++.exe");
        }
#endif

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            return info.Environment;
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
#endif
        }
    }
#endif
}