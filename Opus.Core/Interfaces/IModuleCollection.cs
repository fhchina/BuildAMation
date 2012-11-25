// <copyright file="IModuleCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IModuleCollection : INestedDependents
    {
        IModule GetChildModule(object owner, params string[] pathSegments);
    }
}
