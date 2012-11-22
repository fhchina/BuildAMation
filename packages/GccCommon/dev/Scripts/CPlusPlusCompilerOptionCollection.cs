// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);

            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLine);
        }

        public static void ExportedDefaults<T>(T options, Opus.Core.DependencyNode node) where T : CCompilerOptionCollection, C.ICxxCompilerOptions
        {
            C.ICCompilerOptions cInterfaceOptions = options as C.ICCompilerOptions;
            C.ICxxCompilerOptions cxxInterfaceOptions = options as C.ICxxCompilerOptions;

            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);
            ExportedDefaults(this, node);
        }

        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public static void ExceptionHandlerCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.Cxx.EExceptionHandler.Disabled:
                    commandLineBuilder.Add("-fno-exceptions");
                    break;

                case C.Cxx.EExceptionHandler.Asynchronous:
                case C.Cxx.EExceptionHandler.Synchronous:
                    commandLineBuilder.Add("-fexceptions");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
        }
    }
}
