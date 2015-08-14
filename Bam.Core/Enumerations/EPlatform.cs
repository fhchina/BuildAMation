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
namespace Bam.Core
{
    [System.Flags]
    public enum EPlatform
    {
        Invalid = 0,
        Win32   = (1 << 0),
        Win64   = (1 << 1),
        Unix32  = (1 << 2),
        Unix64  = (1 << 3),
        OSX32   = (1 << 4),
        OSX64   = (1 << 5),

        Windows = Win32 | Win64,
        Unix    = Unix32 | Unix64,
        OSX     = OSX32 | OSX64,
        Posix   = Unix | OSX,

        NotWindows = ~Windows,
        NotUnix    = ~Unix,
        NotOSX     = ~OSX,
        NotPosix   = ~Posix,

        All        = Windows | Unix | OSX
    }

namespace V2
{
    static public class PlatformExtensions
    {
        // can't just use HasFlag, as the logic is inversed with the combined flag enum values
        public static bool
        Includes(
            this EPlatform platform,
            EPlatform choice)
        {
            return (0 != (platform & choice));
        }
    }
}
}
