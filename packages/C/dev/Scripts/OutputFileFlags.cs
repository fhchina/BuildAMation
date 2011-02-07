// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.Flags]
    public enum OutputFileFlags
    {
        ObjectFile = (1 << 0),
        PreprocessedFile = (1 << 1),
        StaticLibrary = (1 << 2),
        Executable = (1 << 3),
        StaticImportLibrary = (1 << 4),
        MapFile = (1 << 5),

        CompilerProgramDatabase = (1 << 6),
        LinkerProgramDatabase = (1 << 7)
    }
}
