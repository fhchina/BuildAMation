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
using Bam.Core;
using CodeGenTest.CodeGenExtension;
namespace CodeGenTest
{
    namespace CodeGenExtension
    {
        public static class CodeGenExtension
        {
            public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
            GenerateSource(
                this C.CObjectFileCollection module)
            {
                // generated source file
                var generatedSourceFile = Bam.Core.Module.Create<GeneratedSourceModule>(module);

                // compile source
                var objFile = module.AddFile(GeneratedSourceModule.Key, generatedSourceFile);

                return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(generatedSourceFile, objFile);
            }
        }
    }

    public sealed class BuildCodeGenTool :
        C.ConsoleApplication,
        Bam.Core.ICommandLineTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateCSourceContainer("$(packagedir)/source/codegentool/main.c");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }

        public Settings
        CreateDefaultSettings<T>(
            T module) where T : Module
        {
            return new GeneratedSourceSettings();
        }

        public System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> InheritedEnvironmentVariables
        {
            get;
            private set;
        }

        public TokenizedString Executable
        {
            get
            {
                return this.GeneratedPaths[C.ConsoleApplication.Key];
            }
        }
    }

    public sealed class GeneratedSourceSettings :
        Bam.Core.Settings
    {
    }

    public interface IGeneratedSourcePolicy
    {
        void
        GenerateSource(
            GeneratedSourceModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString generatedFilePath);
    }

    public class GeneratedSourceModule :
        C.SourceFile
    {
        private Bam.Core.ICommandLineTool Compiler;
        private IGeneratedSourcePolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<BuildCodeGenTool>();
            this.Requires(this.Compiler as Bam.Core.Module);
            this.GeneratedPaths[Key].Aliased(this.CreateTokenizedString("$(buildroot)/Generated.c"));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = ExecuteReasoning.Undefined();
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }

            this.Policy.GenerateSource(this, context, this.Compiler, this.GeneratedPaths[Key]);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "CodeGenTest." + mode + "GenerateSource";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IGeneratedSourcePolicy>.Create(className);
        }
    }

    public sealed class TestApp :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/testapp/main.c");

            /*var generatedSourceTuple = */source.GenerateSource();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
