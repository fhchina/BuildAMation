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
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
        {
            settings.Bits = (module as CModule).BitDepth;
            settings.DebugSymbols = module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug;
            settings.OmitFramePointer = module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug;
            settings.Optimization = module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug ? EOptimization.Off : EOptimization.Speed;
            settings.OutputType = ECompilerOutput.CompileOnly;
            settings.PreprocessorDefines.Add(System.String.Format("D_BAM_CONFIGURATION_{0}", module.BuildEnvironment.Configuration.ToString().ToUpper()));
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_WINDOWS");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_UNIX");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_OSX");
            }
            else
            {
                throw new Bam.Core.Exception("Unknown platform");
            }
            settings.TargetLanguage = ETargetLanguage.C;
            settings.WarningsAsErrors = true;
        }
        public static void Empty(this C.V2.ICommonCompilerOptions settings)
        {
            settings.DisableWarnings = new Bam.Core.StringArray();
            settings.IncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            settings.PreprocessorDefines = new PreprocessorDefinitions();
            settings.PreprocessorUndefines = new Bam.Core.StringArray();
            settings.SystemIncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
        }
        public static void
        Delta(
            this C.V2.ICommonCompilerOptions settings,
            C.V2.ICommonCompilerOptions delta,
            C.V2.ICommonCompilerOptions other)
        {
            if (settings.Bits != other.Bits)
            {
                delta.Bits = settings.Bits;
            }
            foreach (var define in settings.PreprocessorDefines)
            {
                if (!other.PreprocessorDefines.Contains(define.Key) ||
                    define.Value != other.PreprocessorDefines[define.Key])
                {
                    delta.PreprocessorDefines.Add(define.Key, define.Value);
                }
            }
            foreach (var path in settings.IncludePaths)
            {
                if (!other.IncludePaths.Contains(path))
                {
                    delta.IncludePaths.AddUnique(path);
                }
            }
            foreach (var path in settings.SystemIncludePaths)
            {
                if (!other.SystemIncludePaths.Contains(path))
                {
                    delta.SystemIncludePaths.AddUnique(path);
                }
            }
            if (settings.OutputType != other.OutputType)
            {
                delta.OutputType = settings.OutputType;
            }
            if (settings.DebugSymbols != other.DebugSymbols)
            {
                delta.DebugSymbols = settings.DebugSymbols;
            }
            if (settings.WarningsAsErrors != other.WarningsAsErrors)
            {
                delta.WarningsAsErrors = settings.WarningsAsErrors;
            }
            if (settings.Optimization != other.Optimization)
            {
                delta.Optimization = settings.Optimization;
            }
            if (settings.TargetLanguage != other.TargetLanguage)
            {
                delta.TargetLanguage = settings.TargetLanguage;
            }
            if (settings.OmitFramePointer != other.OmitFramePointer)
            {
                delta.OmitFramePointer = settings.OmitFramePointer;
            }
            foreach (var path in settings.DisableWarnings)
            {
                if (!other.DisableWarnings.Contains(path))
                {
                    delta.DisableWarnings.AddUnique(path);
                }
            }
            foreach (var path in settings.PreprocessorUndefines)
            {
                if (!other.PreprocessorUndefines.Contains(path))
                {
                    delta.PreprocessorUndefines.AddUnique(path);
                }
            }
        }
        public static void
        Clone(
            this C.V2.ICommonCompilerOptions settings,
            C.V2.ICommonCompilerOptions other)
        {
            settings.Bits = other.Bits;
            foreach (var define in other.PreprocessorDefines)
            {
                settings.PreprocessorDefines.Add(define.Key, define.Value);
            }
            foreach (var path in other.IncludePaths)
            {
                settings.IncludePaths.AddUnique(path);
            }
            foreach (var path in other.SystemIncludePaths)
            {
                settings.SystemIncludePaths.AddUnique(path);
            }
            settings.OutputType = other.OutputType;
            settings.DebugSymbols = other.DebugSymbols;
            settings.WarningsAsErrors = other.WarningsAsErrors;
            settings.Optimization = other.Optimization;
            settings.TargetLanguage = other.TargetLanguage;
            settings.OmitFramePointer = other.OmitFramePointer;
            foreach (var path in other.DisableWarnings)
            {
                settings.DisableWarnings.AddUnique(path);
            }
            foreach (var path in other.PreprocessorUndefines)
            {
                settings.PreprocessorUndefines.AddUnique(path);
            }
        }
    }
}
    public abstract class SettingsBase :
        Bam.Core.V2.Settings
    {
        protected void
        InitializeAllInterfaces(
            Bam.Core.V2.Module module,
            bool emptyFirst,
            bool useDefaults)
        {
            var attributeType = typeof(Bam.Core.V2.SettingsExtensionsAttribute);
            var moduleType = typeof(Bam.Core.V2.Module);
            var baseI = typeof(Bam.Core.V2.ISettingsBase);
            foreach (var i in this.GetType().GetInterfaces())
            {
                // is it a true settings interface?
                if (!baseI.IsAssignableFrom(i))
                {
                    Bam.Core.Log.DebugMessage("Ignored interface {0} on {1}", i.ToString(), this.GetType().ToString());
                    continue;
                }
                if (i == baseI)
                {
                    continue;
                }

                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as Bam.Core.V2.SettingsExtensionsAttribute;

                // TODO: the Empty function could be replaced by the auto-property initializers in C#6.0 (when Mono catches up)
                // although it won't then be a centralized definition, so the extension method as-is is probably better
                if (emptyFirst)
                {
                    var emptyMethod = attribute.GetMethod("Empty", new[] { i });
                    if (null != emptyMethod)
                    {
                        Bam.Core.Log.DebugMessage("Executing {0}", emptyMethod.ToString());
                        emptyMethod.Invoke(null, new[] { this });
                    }
                    else
                    {
                        Bam.Core.Log.DebugMessage("Unable to find method {0}.Empty({1})", attribute.ClassType.ToString(), i.ToString());
                    }
                }

                if (useDefaults)
                {
                    var defaultMethod = attribute.GetMethod("Defaults", new[] { i, moduleType });
                    if (null == defaultMethod)
                    {
                        throw new Bam.Core.Exception("Unable to find method {0}.Defaults({1}, {2})", attribute.ClassType.ToString(), i.ToString(), moduleType.ToString());
                    }
                    Bam.Core.Log.DebugMessage("Executing {0}", defaultMethod.ToString());
                    defaultMethod.Invoke(null, new object[] { this, module });
                }
            }
        }

        public SettingsBase
        Delta(
            Bam.Core.V2.Settings other,
            Module module)
        {
            var settingsType = this.GetType();
            var deltaSettings = System.Activator.CreateInstance(settingsType, module, false) as SettingsBase;

            var baseI = typeof(ISettingsBase);
            var attributeType = typeof(SettingsExtensionsAttribute);

            var interfaces = settingsType.GetInterfaces();
            foreach (var i in interfaces)
            {
                // is it a true settings interface?
                if (!baseI.IsAssignableFrom(i))
                {
                    Bam.Core.Log.DebugMessage("Ignored interface {0} on {1}", i.ToString(), this.GetType().ToString());
                    continue;
                }
                if (i == baseI)
                {
                    continue;
                }
                if (!i.IsAssignableFrom(other.GetType()))
                {
                    continue;
                }

                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as Bam.Core.V2.SettingsExtensionsAttribute;

                var deltaMethod = attribute.GetMethod("Delta", new[] { i, i, i });
                if (null != deltaMethod)
                {
                    Bam.Core.Log.DebugMessage("Executing {0}", deltaMethod.ToString());
                    deltaMethod.Invoke(null, new[] { this, deltaSettings, other });
                }
                else
                {
                    throw new Bam.Core.Exception("Unable to find method {0}.Delta({1}, {1}, {1})", attribute.ClassType.ToString(), i.ToString());
                }
            }

            return deltaSettings;
        }

        public SettingsBase
        Clone(
            Module module)
        {
            var settingsType = this.GetType();
            var clonedSettings = System.Activator.CreateInstance(settingsType, module, false) as SettingsBase;

            var baseI = typeof(ISettingsBase);
            var attributeType = typeof(SettingsExtensionsAttribute);

            var interfaces = settingsType.GetInterfaces();
            foreach (var i in interfaces)
            {
                // is it a true settings interface?
                if (!baseI.IsAssignableFrom(i))
                {
                    Bam.Core.Log.DebugMessage("Ignored interface {0} on {1}", i.ToString(), this.GetType().ToString());
                    continue;
                }
                if (i == baseI)
                {
                    continue;
                }

                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as Bam.Core.V2.SettingsExtensionsAttribute;

                var cloneMethod = attribute.GetMethod("Clone", new[] { i, i });
                if (null != cloneMethod)
                {
                    Bam.Core.Log.DebugMessage("Executing {0}", cloneMethod.ToString());
                    cloneMethod.Invoke(null, new[] { clonedSettings, this });
                }
                else
                {
                    throw new Bam.Core.Exception("Unable to find method {0}.Clone({1}, {1})", attribute.ClassType.ToString(), i.ToString());
                }
            }

            return clonedSettings;
        }
    }

    public enum EBit
    {
        ThirtyTwo = 32,
        SixtyFour = 64
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        EBit? Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? OutputType
        {
            get;
            set;
        }

        bool? DebugSymbols
        {
            get;
            set;
        }

        bool? WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? TargetLanguage
        {
            get;
            set;
        }

        bool? OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray PreprocessorUndefines
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        C.ECLanguageStandard? LanguageStandard
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.ObjC.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface IObjectiveCOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        string ConstantStringClass
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface IObjectiveCxxOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
    }
}
    public interface ICCompilerOptions
    {
        /// <summary>
        /// Preprocessor definitions
        /// </summary>
        C.DefineCollection Defines
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor user header search paths: #includes of the style #include "..."
        /// </summary>
        Bam.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor system header search paths: #includes of the stylr #include <...>
        /// </summary>
        Bam.Core.DirectoryCollection SystemIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Type of file output from the compiler
        /// </summary>
        C.ECompilerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Compiled objects contain debug symbols
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Treat all warnings as errors
        /// </summary>
        bool WarningsAsErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Compiler ignores all built in standard include paths. User must provide them.
        /// </summary>
        bool IgnoreStandardIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// The level of optimization the compiler uses
        /// </summary>
        C.EOptimization Optimization
        {
            get;
            set;
        }

        /// <summary>
        /// Custom optimization settings not provided by the Optimization option.
        /// </summary>
        string CustomOptimization
        {
            get;
            set;
        }

        /// <summary>
        /// The target language of the compiled source code
        /// </summary>
        C.ETargetLanguage TargetLanguage
        {
            get;
            set;
        }

        /// <summary>
        /// Display the paths of the header files included on stdout
        /// </summary>
        bool ShowIncludes
        {
            get;
            set;
        }

        /// <summary>
        /// Explicit compiler options added to the compilation step
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Omit the frame pointer from the compiled object code for each stack frame
        /// </summary>
        bool OmitFramePointer
        {
            get;
            set;
        }

        /// <summary>
        /// A list of warnings that are disabled
        /// </summary>
        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        /// <summary>
        /// The target character set in use by the compiled code
        /// </summary>
        C.ECharacterSet CharacterSet
        {
            get;
            set;
        }

        /// <summary>
        /// The language standard for C/C++ compilation
        /// </summary>
        C.ELanguageStandard LanguageStandard
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor definitions to be undefined
        /// </summary>
        C.DefineCollection Undefines
        {
            get;
            set;
        }
    }
}
