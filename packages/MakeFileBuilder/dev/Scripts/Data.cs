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
namespace MakeFileBuilder
{
namespace V2
{
    // Notes:
    // A rule is target + prerequisities + receipe
    // A recipe is a collection of commands
    using System.Linq;

    public sealed class MakeFileCommonMetaData
    {
        public MakeFileCommonMetaData()
        {
            this.Directories = new Bam.Core.StringArray();
            this.Environment = new System.Collections.Generic.Dictionary<string, string>();
        }

        public Bam.Core.StringArray Directories
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, string> Environment
        {
            get;
            private set;
        }

        public void
        ExtendEnvironmentVariables(
            System.Collections.Generic.Dictionary<string, Bam.Core.V2.TokenizedStringArray> import)
        {
            foreach (var env in import)
            {
                if (!this.Environment.ContainsKey(env.Key))
                {
                    this.Environment.Add(env.Key, string.Empty);
                }
                foreach (var path in env.Value)
                {
                    if (this.Environment[env.Key].Contains(path.ToString()))
                    {
                        continue;
                    }
                    this.Environment[env.Key] += path.ToString() + System.IO.Path.PathSeparator;
                }
            }
        }
    }

    public sealed class MakeFileMeta
    {
        public MakeFileMeta(Bam.Core.V2.Module module)
        {
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
            this.Recipe = new System.Collections.Generic.List<string>();

            if (Bam.Core.V2.Graph.Instance.IsReferencedModule(module))
            {
                // make the target names unique across configurations
                this.TargetVariable = System.String.Format("{0}_{1}", module.GetType().Name, module.BuildEnvironment.Configuration.ToString());
            }

            this.CommonMetaData = Bam.Core.V2.Graph.Instance.MetaData as MakeFileCommonMetaData;

            module.MetaData = this;
        }

        public MakeFileCommonMetaData CommonMetaData
        {
            get;
            private set;
        }

        public Bam.Core.V2.TokenizedString Target
        {
            get;
            set;
        }

        public System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Prequisities
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> Recipe
        {
            get;
            private set;
        }

        public string TargetVariable
        {
            get;
            private set;
        }

        public static void PreExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            graph.MetaData = new MakeFileCommonMetaData();
        }

        public static void PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var commonMeta = graph.MetaData as MakeFileCommonMetaData;

            var makeEnvironment = new System.Text.StringBuilder();
            var makeVariables = new System.Text.StringBuilder();
            var makeRules = new System.Text.StringBuilder();

            // delete suffix rules
            makeEnvironment.AppendLine(".SUFFIXES:");
            foreach (var env in commonMeta.Environment)
            {
                makeEnvironment.AppendFormat("{0}:={1}", env.Key, env.Value);
                makeEnvironment.AppendLine();
            }

            if (commonMeta.Directories.Count > 0)
            {
                makeVariables.Append("DIRS:=");
                foreach (var dir in commonMeta.Directories)
                {
                    makeVariables.AppendFormat("{0} ", dir);
                }
                makeVariables.AppendLine();
            }

            makeRules.Append("all:");
            foreach (var module in graph.TopLevelModules)
            {
                var metadata = module.MetaData as MakeFileMeta;
                if (null == metadata)
                {
                    throw new Bam.Core.Exception("Top level module did not have any Make metadata");
                }
                makeRules.AppendFormat("$({0}) ", metadata.TargetVariable);
            }
            makeRules.AppendLine();

            makeRules.AppendLine("$(DIRS):");
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                makeRules.AppendLine("\tmkdir $@");
            }
            else
            {
                makeRules.AppendLine("\tmkdir -p $@");
            }

            foreach (var rank in graph.Reverse())
            {
                foreach (var module in rank)
                {
                    var metadata = module.MetaData as MakeFileMeta;
                    if (null == metadata)
                    {
                        continue;
                    }

                    if (metadata.TargetVariable != null)
                    {
                        // simply expanded variable
                        makeVariables.AppendFormat("{0}:={1}", metadata.TargetVariable, metadata.Target);
                        makeVariables.AppendLine();

                        makeRules.AppendFormat("$({0}):", metadata.TargetVariable);
                    }
                    else
                    {
                        makeRules.AppendFormat("{0}:", metadata.Target);
                    }
                    foreach (var pre in metadata.Prequisities)
                    {
                        makeRules.AppendFormat("{0} ", pre.Key.GeneratedPaths[pre.Value]);
                    }
                    makeRules.AppendFormat("| $(DIRS)");
                    makeRules.AppendLine();
                    foreach (var command in metadata.Recipe)
                    {
                        makeRules.AppendFormat("\t{0}", command);
                        makeRules.AppendLine();
                    }
                }
            }

            Bam.Core.Log.DebugMessage(makeEnvironment.ToString());
            Bam.Core.Log.DebugMessage(makeVariables.ToString());
            Bam.Core.Log.DebugMessage(makeRules.ToString());

            var makeFilePath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/Makefile", null);
            makeFilePath.Parse();

            using (var writer = new System.IO.StreamWriter(makeFilePath.ToString()))
            {
                writer.Write(makeEnvironment.ToString());
                writer.Write(makeVariables.ToString());
                writer.Write(makeRules.ToString());
            }
        }
    }
}
    public sealed class MakeFileData
    {
        public
        MakeFileData(
            string makeFilePath,
            MakeFileTargetDictionary targetDictionary,
            MakeFileVariableDictionary variableDictionary,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment)
        {
            this.MakeFilePath = makeFilePath;
            this.TargetDictionary = targetDictionary;
            this.VariableDictionary = variableDictionary;
            if (null != environment)
            {
                // TODO: better way to do a copy?
                this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
                foreach (var key in environment.Keys)
                {
                    this.Environment[key] = environment[key];
                }
            }
            else
            {
                this.Environment = null;
            }
        }

        public string MakeFilePath
        {
            get;
            private set;
        }

        public MakeFileTargetDictionary TargetDictionary
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary VariableDictionary
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment
        {
            get;
            private set;
        }
    }
}
