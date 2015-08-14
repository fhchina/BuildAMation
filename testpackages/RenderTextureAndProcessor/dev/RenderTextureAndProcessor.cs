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
namespace RenderTextureAndProcessor
{
    // Define module classes here
    class RenderTexture :
        C.WindowsApplication
    {
        public
        RenderTexture()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var commonDir = sourceDir.SubDirectory("common");
            var renderTextureDir = sourceDir.SubDirectory("rendertexture");
            this.headerFiles.Include(commonDir, "*.h");
            this.headerFiles.Include(renderTextureDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var commonDir = sourceDir.SubDirectory("common");
                var renderTextureDir = sourceDir.SubDirectory("rendertexture");
                this.Include(commonDir, "*.cpp");
                this.Include(renderTextureDir, "*.cpp");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    options.IncludePaths.Include(sourceDir, "common");
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray requiredLibrariesVC = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "GDI32.lib",
            "USER32.lib",
            "WS2_32.lib",
            "SHELL32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Bam.Core.StringArray requiredLibrariesMingw = new Bam.Core.StringArray(
            "-lws2_32",
            "-lgdi32"
        );

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependentModules = new Bam.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependentModules = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [Bam.Core.RequiredModules]
        Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(typeof(TextureProcessor));
    }

    class TextureProcessor :
        C.Application
    {
        public
        TextureProcessor()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var commonDir = sourceDir.SubDirectory("common");
            this.headerFiles.Include(commonDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var commonDir = sourceDir.SubDirectory("common");
                var textureProcessorDir = sourceDir.SubDirectory("textureprocessor");
                this.Include(commonDir, "*.cpp");
                this.Include(textureProcessorDir, "*.cpp");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    options.IncludePaths.Include(sourceDir, "common");
                }
            }
        }

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray requiredLibrariesVC = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "WS2_32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Bam.Core.StringArray requiredLibrariesMingw = new Bam.Core.StringArray("-lws2_32");

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray dependentModules = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
