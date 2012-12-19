// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // this implementation is here because the specific version of the Mingw compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        public CxxCompilerOptionCollection()
            : base()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.GetInstance(typeof(Mingw.Toolset));

            C.ICompilerTool compilerTool = info.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string cppIncludePath = System.IO.Path.Combine(compilerTool.IncludePaths(node.Target)[0], "c++");
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);
            // TODO: might be able to clean this up from the MingwDetailData
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(System.IO.Path.Combine(cppIncludePath, "mingw32"), false);

            MingwCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }
    }
}
