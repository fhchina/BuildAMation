// <copyright file="IToolset.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolset
    {
        string Version(Opus.Core.Target target);

        string InstallPath(Opus.Core.Target target);
        
        string BinPath(Opus.Core.Target target);
        
        Opus.Core.StringArray Environment
        {
            get;
        }
    }
}