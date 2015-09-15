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
    /// <summary>
    /// Container representing a 'ranked' collection of modules
    /// </summary>
    public sealed class ModuleCollection :
        System.Collections.Generic.IEnumerable<Module>
    {
        private System.Collections.Generic.List<Module> Modules = new System.Collections.Generic.List<Module>();

        public void
        Add(
            Module m)
        {
            if (null != m.OwningRank)
            {
                if (this == m.OwningRank)
                {
                    return;
                }
                var originalRank = Graph.Instance.DependencyGraph[m.OwningRank];
                var newRank = Graph.Instance.DependencyGraph[this];
                if (newRank <= originalRank)
                {
                    return;
                }
                this.Move(m);
            }
            this.Modules.Add(m);
            var r = Graph.Instance.DependencyGraph[this];
            m.OwningRank = this;
        }

        private void
        Move(
            Module m)
        {
            if (null == m.OwningRank)
            {
                throw new System.Exception("Cannot move a module that has not been assigned");
            }
            m.OwningRank.Modules.Remove(m);
            m.OwningRank = null;
        }

        public System.Collections.Generic.IEnumerator<Module>
        GetEnumerator()
        {
            for (int i = 0; i < this.Modules.Count; ++i)
            {
                yield return this.Modules[i];
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
