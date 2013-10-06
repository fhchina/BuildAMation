// <copyright file="ObjCxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    // this implementation is here because the specific version of the LLVMGcc compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class ObjCxxCompilerOptionCollection : ObjCCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            // TODO: think I can move this to GccCommon, but it misses out the C++ include paths for some reason (see Test9-dev)
            Opus.Core.Target target = node.Target;
            GccCommon.Toolset gccToolset = target.Toolset as GccCommon.Toolset;
            string machineType = gccToolset.GccDetail.Target;
            string cxxIncludePath = gccToolset.GccDetail.GxxIncludePath;

            if (!System.IO.Directory.Exists(cxxIncludePath))
            {
                throw new Opus.Core.Exception("llvm-g++ include path '{0}' does not exist. Is llvm-g++ installed?", cxxIncludePath);
            }
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cxxIncludePath, false);

            // TODO: not sure if this is necessary
#if false
            string cxxIncludePath2 = System.String.Format("{0}/{1}", cxxIncludePath, machineType);
            if (!System.IO.Directory.Exists(cxxIncludePath2))
            {
                throw new Opus.Core.Exception("llvm-g++ include path '{0}' does not exist. Is llvm-g++ installed?", cxxIncludePath2);
            }
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cxxIncludePath2, false);
#endif

            GccCommon.ObjCxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public ObjCxxCompilerOptionCollection()
            : base()
        {
        }

        public ObjCxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}