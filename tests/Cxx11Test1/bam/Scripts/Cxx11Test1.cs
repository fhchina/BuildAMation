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
[assembly: Bam.Core.GlobalOptionCollectionOverride(typeof(Cxx11Test1.GlobalSettings))]
namespace Cxx11Test1
{
    public sealed class LocalPolicy :
        Bam.Core.V2.ISitePolicy
    {
        void
        ISitePolicy.DefineLocalSettings(
            Settings settings,
            Module module)
        {
            var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
            if (null != cxxCompiler)
            {
                cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.Cxx11;
                cxxCompiler.StandardLibrary = C.Cxx.EStandardLibrary.libcxx;
            }

            var cxxLinker = settings as C.V2.ICxxOnlyLinkerOptions;
            if (null != cxxLinker)
            {
                cxxLinker.StandardLibrary = C.Cxx.EStandardLibrary.libcxx;
            }
        }
    }

    public sealed class TestProgV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/main.cpp");

            source.PrivatePatch(settings =>
                {
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                        this.Linker is VisualC.V2.LinkerBase)
                    {
                        var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    class GlobalSettings : Bam.Core.IGlobalOptionCollectionOverride
    {
        #region IGlobalOptionCollectionOverride implementation
        void
        Bam.Core.IGlobalOptionCollectionOverride.OverrideOptions(
            Bam.Core.BaseOptionCollection optionCollection,
            Bam.Core.Target target)
        {
            var cOptions = optionCollection as C.ICCompilerOptions;
            if (null != cOptions)
            {
                var cxxOptions = optionCollection as C.ICxxCompilerOptions;
                if (null != cxxOptions)
                {
                    // enable exceptions (some flavours of STL need it)
                    cxxOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;

                    // default C++ mode to C++11
                    // note this is on the C compiler options, but only enabled when C++
                    // options are in use
                    cOptions.LanguageStandard = C.ELanguageStandard.Cxx11;
                }
            }
        }
        #endregion
    }

    class TestProg : C.Application
    {
        class Source : C.Cxx.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray windowsDeps = new Bam.Core.TypeArray() {
            typeof(WindowsSDK.WindowsSDK)
        };
    }
}