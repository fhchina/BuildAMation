#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace C
{
namespace V2
{
    public class DynamicLibrary :
        ConsoleApplication
    {
        static public Bam.Core.V2.FileKey ImportLibraryKey = Bam.Core.V2.FileKey.Generate("Import Library File");

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths[Key] = Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(dynamicprefix)$(OutputName)$(dynamicext)", this);
            this.Macros.Add("LinkOutput", this.GeneratedPaths[Key]);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.RegisterGeneratedFile(ImportLibraryKey, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(libprefix)$(OutputName)$(libext)", this));
            }

            this.PrivatePatch(settings =>
            {
                var linker = settings as C.V2.ICommonLinkerOptions;
                if (null != linker)
                {
                    linker.OutputType = ELinkerOutput.DynamicLibrary;
                }

                var osxLinker = settings as C.V2.ILinkerOptionsOSX;
                if (null != osxLinker)
                {
                    osxLinker.InstallName = Bam.Core.V2.TokenizedString.Create("@executable_path/@filename($(LinkOutput))", this);
                }
            });
        }

        // TODO: what is this used for?
        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> Source
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.sourceModules.ToArray());
            }
        }

        public override CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCSourceContainer(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.V2.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        public override Cxx.V2.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCxxSourceContainer(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.V2.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        public void
        CompileAgainstPublicly<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source module argument must be passed to {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, this.ToString());
            }

            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
                this.UsePublicPatches(dependent);
            }
        }
    }
}
namespace Cxx
{
namespace V2
{
    public class DynamicLibrary :
        C.V2.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Linker = C.V2.DefaultToolchain.Cxx_Linker(this.BitDepth);
        }
    }
}
}
    /// <summary>
    /// C/C++ dynamic library
    /// </summary>
    public partial class DynamicLibrary :
        Application,
        Bam.Core.IPostActionModules
    {
        public static readonly Bam.Core.LocationKey ImportLibraryFile = new Bam.Core.LocationKey("ImportLibraryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey ImportLibraryDir = new Bam.Core.LocationKey("ImportLibraryDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        protected
        DynamicLibrary()
        {
            this.PostActionModuleTypes = new Bam.Core.TypeArray();
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.PostActionModuleTypes.Add(typeof(PosixSharedLibrarySymlinks));
            }
        }

        [LocalCompilerOptionsDelegate]
        protected static void
        DynamicLibrarySetDLLBuildPreprocessorDefine(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
        }

        [LocalLinkerOptionsDelegate]
        protected static void
        DynamicLibraryEnableDLL(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.DynamicLibrary;

            if (module.Options is ILinkerOptionsOSX && target.HasPlatform(Bam.Core.EPlatform.OSX32))
            {
                // only required for 32-bit builds
                (module.Options as ILinkerOptionsOSX).SuppressReadOnlyRelocations = true;
            }
        }

        public Bam.Core.TypeArray PostActionModuleTypes
        {
            get;
            private set;
        }

        #region IPostActionModules Members

        Bam.Core.TypeArray
        Bam.Core.IPostActionModules.GetPostActionModuleTypes(
            Bam.Core.BaseTarget target)
        {
            return this.PostActionModuleTypes;
        }

        #endregion
    }
}