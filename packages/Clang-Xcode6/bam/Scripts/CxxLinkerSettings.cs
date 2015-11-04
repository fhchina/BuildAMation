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
namespace Clang
{
    public class CxxLinkerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        XcodeProjectProcessor.IConvertToProject,
        C.ICommonLinkerSettings,
        C.ICxxOnlyLinkerSettings,
        C.ICommonLinkerSettingsOSX,
        C.IAdditionalSettings
    {
        public CxxLinkerSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);

            // not in the defaults in the C package to avoid a compile-time dependency on the Clang package
            (this as C.ICommonLinkerSettingsOSX).MinimumVersionSupported =
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MinimumVersionSupported;
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(ClangCommon.CommandLineLinkerImplementation), this, module, commandLine);
        }

        void
        XcodeProjectProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            XcodeProjectProcessor.Conversion.Convert(typeof(ClangCommon.XcodeLinkerImplementation), this, module, configuration);
        }

        C.ELinkerOutput C.ICommonLinkerSettings.OutputType
        {
            get;
            set;
        }

        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries
        {
            get;
            set;
        }

        bool? C.ICommonLinkerSettings.DebugSymbols
        {
            get;
            set;
        }

        C.Cxx.EStandardLibrary? C.ICxxOnlyLinkerSettings.StandardLibrary
        {
            get;
            set;
        }

        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.Frameworks
        {
            get;
            set;
        }

        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.FrameworkSearchPaths
        {
            get;
            set;
        }

        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.InstallName
        {
            get;
            set;
        }

        string C.ICommonLinkerSettingsOSX.MinimumVersionSupported
        {
            get;
            set;
        }

        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }
    }
}