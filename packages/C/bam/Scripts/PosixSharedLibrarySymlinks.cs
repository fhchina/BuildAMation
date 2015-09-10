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
namespace C
{
    /// <summary>
    /// Posix shared library symlink creation
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IPosixSharedLibrarySymlinksTool))]
    public class PosixSharedLibrarySymlinks :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("PosixSOSymlinkOutputDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey LinkerSymlink = new Bam.Core.LocationKey("PosixSOSymlinkLinkerSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
        public static readonly Bam.Core.LocationKey MajorVersionSymlink = new Bam.Core.LocationKey("PosixSOSymlinkMajorVersionSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
        public static readonly Bam.Core.LocationKey MinorVersionSymlink = new Bam.Core.LocationKey("PosixSOSymlinkMinorVersionSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);

        public Bam.Core.Location RealSharedLibraryFileLocation
        {
            get;
            set;
        }
    }
}