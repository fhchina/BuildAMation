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
namespace XcodeBuilder
{
    public sealed class PBXContainerItemProxySection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXContainerItemProxySection()
        {
            this.ContainerItemProxies = new System.Collections.Generic.List<PBXContainerItemProxy>();
        }

        public PBXContainerItemProxy
        Get(
            string name,
            XcodeNodeData remote,
            XcodeNodeData portal)
        {
            lock (this.ContainerItemProxies)
            {
                foreach (var containerItem in this.ContainerItemProxies)
                {
                    if ((containerItem.Name == name) && (containerItem.Remote == remote) && (containerItem.Portal == portal))
                    {
                        return containerItem;
                    }
                }

                var newContainerItem = new PBXContainerItemProxy(name, remote, portal);
                this.ContainerItemProxies.Add(newContainerItem);
                return newContainerItem;
            }
        }

        private System.Collections.Generic.List<PBXContainerItemProxy> ContainerItemProxies
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.ContainerItemProxies.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXContainerItemProxy>(this.ContainerItemProxies);
            orderedList.Sort(
                delegate(PBXContainerItemProxy p1, PBXContainerItemProxy p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXContainerItemProxy section */");
            foreach (var item in orderedList)
            {
                (item as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXContainerItemProxy section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.ContainerItemProxies.GetEnumerator();
        }

#endregion
    }
}
