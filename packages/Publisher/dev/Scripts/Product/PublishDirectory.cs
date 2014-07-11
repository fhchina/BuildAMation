// <copyright file="PublishDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public class PublishDirectory
    {
        public PublishDirectory(
            Opus.Core.Location root,
            string directory)
        {
            this.Root = root;
            this.Directory = directory;
            this.DirectoryLocation = new Opus.Core.ScaffoldLocation(root, directory, Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
        }

        public Opus.Core.Location Root
        {
            get;
            private set;
        }

        public string Directory
        {
            get;
            private set;
        }

        public Opus.Core.Location DirectoryLocation
        {
            get;
            private set;
        }
    }
}