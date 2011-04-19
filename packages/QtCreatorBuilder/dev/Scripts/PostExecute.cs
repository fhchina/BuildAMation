// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCreatorBuilder package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public void PostExecute(Opus.Core.DependencyNodeCollection nodeCollection)
        {
            Opus.Core.Log.DebugMessage("PostExecute for QtCreator");

            Opus.Core.PackageInformation mainPackage = Opus.Core.State.PackageInfo[0];
            string fileName = System.String.Format("{0}.pro", mainPackage.FullName);
            string rootDirectory = mainPackage.BuildDirectory;
            string topProPathName = System.IO.Path.Combine(rootDirectory, fileName);
            rootDirectory += System.IO.Path.DirectorySeparatorChar;

            using (System.IO.TextWriter proWriter = new System.IO.StreamWriter(topProPathName))
            {
                proWriter.WriteLine("# -- Generated by Opus --");
                proWriter.WriteLine("TEMPLATE = subdirs");
                proWriter.WriteLine("SUBDIRS += \\");

                foreach (Opus.Core.DependencyNode node in nodeCollection)
                {
                    NodeData data = node.Data as NodeData;
                    if ((null != data) && (null != data.ProjectFileDirectory))
                    {
                        string subDirProjectDir = data.ProjectFileDirectory + System.IO.Path.DirectorySeparatorChar;
                        string relativeDir = Opus.Core.RelativePathUtilities.GetPath(subDirProjectDir, rootDirectory);
                        relativeDir = relativeDir.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                        proWriter.WriteLine("\t{0}\\", relativeDir.Replace('\\', '/'));
                    }
                }
            }
        }
    }
}