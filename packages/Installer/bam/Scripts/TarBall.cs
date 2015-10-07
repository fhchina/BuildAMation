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
namespace Installer
{
    class TarInputFiles :
        Bam.Core.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.FileKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.FileKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.FileKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.FileKey>();

        public TarInputFiles()
        {
            this.ScriptPath = Bam.Core.TokenizedString.Create("$(buildroot)/$(modulename)/tarinput.txt", this);
        }

        public Bam.Core.TokenizedString ScriptPath
        {
            get;
            private set;
        }

        public void
        AddFile(
            Bam.Core.Module module,
            Bam.Core.FileKey key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        public void
        AddPath(
            Bam.Core.Module module,
            Bam.Core.FileKey key)
        {
            this.DependsOn(module);
            this.Paths.Add(module, key);
        }

        public override void
        Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var path = this.ScriptPath.Parse();
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                foreach (var dep in this.Files)
                {
                    var filePath = dep.Key.GeneratedPaths[dep.Value].ToString();
                    var fileDir = System.IO.Path.GetDirectoryName(filePath);
                    // TODO: this should probably be a setting
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        scriptWriter.WriteLine("-C");
                        scriptWriter.WriteLine(fileDir);
                    }
                    else
                    {
                        scriptWriter.WriteLine("-C {0}", fileDir);
                    }
                    scriptWriter.WriteLine(System.IO.Path.GetFileName(filePath));
                }
                foreach (var dep in this.Paths)
                {
                    var fileDir = dep.Key.GeneratedPaths[dep.Value].ToString();
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        scriptWriter.WriteLine("-C");
                        scriptWriter.WriteLine(fileDir);
                    }
                    else
                    {
                        scriptWriter.WriteLine("-C {0}", fileDir);
                    }
                    scriptWriter.WriteLine(".");
                }
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
        }
    }

    public sealed class TarSettings :
        Bam.Core.Settings
    {
    }

    public sealed class TarCompiler :
        Bam.Core.PreBuiltTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new TarSettings();
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim("tar");
            }
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux | Bam.Core.EPlatform.OSX)]
    public abstract class TarBall :
        Bam.Core.Module
    {
        public static Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("Installer");

        private TarInputFiles InputFiles;
        private Bam.Core.PreBuiltTool Compiler;
        private ITarPolicy Policy;

        public TarBall()
        {
            this.RegisterGeneratedFile(Key, Bam.Core.TokenizedString.Create("$(buildroot)/installer.tar", this));

            // TODO: this actually needs to be a new class each time, otherwise multiple installers won't work
            // need to find a way to instantiate a non-abstract instance of an abstract class
            // looks like emit is needed
            this.InputFiles = Bam.Core.Graph.Instance.FindReferencedModule<TarInputFiles>();
            this.DependsOn(this.InputFiles);

            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<TarCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        Include<DependentModule>(
            Bam.Core.FileKey key) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.InputFiles.AddFile(dependent, key);
        }

        public void
        SourceFolder<DependentModule>(
            Bam.Core.FileKey key) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.InputFiles.AddPath(dependent, key);
        }

        public override void
        Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateTarBall(this, context, this.Compiler, this.InputFiles.ScriptPath, this.GeneratedPaths[Key]);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Installer." + mode + "TarBall";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<ITarPolicy>.Create(className);
            }
        }
    }
}
