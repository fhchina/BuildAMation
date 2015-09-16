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
using System.Linq;
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Argument array.</param>
        static void
        Main(
            string[] args)
        {
#if false
            // take control of Ctrl+C
            System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);
#endif

            try
            {
                var verbosityLevel = (Core.EVerboseLevel)Core.CommandLineProcessor.Evaluate(new Core.VerbosityLevel());
                switch (verbosityLevel)
                {
                    case Core.EVerboseLevel.None:
                    case Core.EVerboseLevel.Info:
                    case Core.EVerboseLevel.Detail:
                    case Core.EVerboseLevel.Full:
                        Core.State.VerbosityLevel = verbosityLevel;
                        break;

                    default:
                        throw new Core.Exception("Unrecognized verbosity level, {0}", verbosityLevel);
                }

                Core.State.ForceDefinitionFileUpdate = Core.CommandLineProcessor.Evaluate(new Core.ForceDefinitionFileUpdate());

                if (Core.CommandLineProcessor.Evaluate(new Core.PrintHelp()))
                {
                    CommandLineArgumentHelper.PrintHelp();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.PrintVersion()))
                {
                    CommandLineArgumentHelper.PrintVersion();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.CreateDebugProject()))
                {
                    DebugProject.Create();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.MakePackage()))
                {
                    Core.PackageUtilities.MakePackage();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.AddDependentPackage()))
                {
                    Core.PackageUtilities.AddDependentPackage();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.ShowDefinitionFile()))
                {
                    Core.PackageUtilities.IdentifyMainAndDependentPackages(true, false);
                    Core.Graph.Instance.MasterPackage.Show();
                    return;
                }

                // configure
                Core.State.BuildRoot = Core.CommandLineProcessor.Evaluate(new Core.BuildRoot());
                Core.State.CompileWithDebugSymbols = Core.CommandLineProcessor.Evaluate(new Core.UseDebugSymbols());
                Core.State.BuildMode = Core.CommandLineProcessor.Evaluate(new Core.BuildMode());
                if (null == Core.State.BuildMode)
                {
                    throw new Core.Exception("No build mode specified");
                }

                var configs = new Core.Array<Core.Environment>();
                var requestedConfigs = Core.CommandLineProcessor.Evaluate(new Core.BuildConfigurations());
                if (0 == requestedConfigs.Count)
                {
                    // default
                    requestedConfigs.Add(new Core.StringArray("debug"));
                }
                foreach (var configOption in requestedConfigs)
                {
                    foreach (var config in configOption)
                    {
                        var env = new Core.Environment();
                        env.Configuration = Core.Configuration.FromString(config);
                        configs.Add(env);
                    }
                }

                Core.EntryPoint.Execute(configs);
            }
            catch (Bam.Core.Exception exception)
            {
                Core.Log.ErrorMessage(exception.Message);
                if (null != exception.InnerException)
                {
                    Core.Log.ErrorMessage("Additional details:");
                    Core.Log.ErrorMessage(exception.InnerException.Message);
                }
                System.Environment.ExitCode = -1;
            }
            finally
            {
                if (Core.Graph.Instance.BuildEnvironments.Count > 0)
                {
                    Core.Log.Info((0 == System.Environment.ExitCode) ? "\nBuild Succeeded" : "\nBuild Failed");
                    Core.Log.DebugMessage("Exit code {0}", System.Environment.ExitCode);
                }
            }
        }

#if false
        private static void
        HandleCancellation(
            object sender,
            System.ConsoleCancelEventArgs e)
        {
            // allow the build to fail gracefully
            var buildManager = Core.State.BuildManager;
            if (null != buildManager)
            {
                buildManager.Cancel();
                e.Cancel = true;
            }
        }
#endif
    }
}
