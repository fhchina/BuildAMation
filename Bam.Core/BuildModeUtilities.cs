#region License
// Copyright (c) 2010-2017, Mark Final
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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Static utility class used to identify and use packages that expose a build mode.
    /// </summary>
    public static class BuildModeUtilities
    {
        /// <summary>
        /// Does the name of the package specified indicate it exposes a build mode. Ends with 'Builder'.
        /// </summary>
        /// <returns><c>true</c> if is build mode package the specified packageName; otherwise, <c>false</c>.</returns>
        /// <param name="packageName">Name of the package to query.</param>
        public static bool
        IsBuildModePackage(
            string packageName)
        {
            return packageName.EndsWith("Builder");
        }

        /// <summary>
        /// Validates the package associated with the selected build mode.
        /// </summary>
        public static void
        ValidateBuildModePackage()
        {
            if (null == Graph.Instance.Mode)
            {
                return;
            }

            var builderPackageName = System.String.Format("{0}Builder", Graph.Instance.Mode);
            var builderPackage = Graph.Instance.Packages.FirstOrDefault(item => item.Name == builderPackageName);
            if (null != builderPackage)
            {
                return;
            }

            throw new Exception("Builder package '{0}' was not specified as a dependency", builderPackageName);
        }
    }
}
