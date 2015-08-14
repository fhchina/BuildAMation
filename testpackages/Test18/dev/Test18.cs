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
namespace Test18
{
    public sealed class ControlV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.LinkAgainst<XV2>();
            this.LinkAgainst<YV2>();
            this.LinkAgainst<ZV2>();
        }
    }

    public sealed class XV2 :
        C.V2.StaticLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            this.CompileAgainst<YV2>();
        }
    }

    public sealed class YV2 :
        C.V2.DynamicLibrary
    {
    }

    public sealed class ZV2 :
        C.V2.StaticLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            this.CompileAgainst<XV2>();
        }
    }

    class Control : C.Application
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(X),
            typeof(Y),
            typeof(Z)
            );
    }

    class X : C.StaticLibrary
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(Y)
            );
    }

    class Y : C.DynamicLibrary
    {
    }

    class Z : C.StaticLibrary
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(X)
            );
    }
}
