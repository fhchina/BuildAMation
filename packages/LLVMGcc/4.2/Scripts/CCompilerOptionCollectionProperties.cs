// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ICCompilerOptions.cs -n=LLVMGcc -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=GccCommon.PrivateData -e
namespace LLVMGcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        LLVMGcc.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<LLVMGcc.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<LLVMGcc.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
        #endregion
    }
}