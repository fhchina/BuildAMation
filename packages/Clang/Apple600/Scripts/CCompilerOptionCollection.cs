#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Clang
{
    public partial class CCompilerOptionCollection :
        ClangCommon.CCompilerOptionCollection,
        C.ICCompilerOptionsOSX
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var options = this as C.ICCompilerOptionsOSX;
            options.FrameworkSearchDirectories = new Bam.Core.DirectoryCollection();
            options.SDKVersion = "10.9";
            options.DeploymentTarget = "10.8";
            options.SupportedPlatform = C.EOSXPlatform.MacOSX;
            options.CompilerName = System.String.Empty; // clang is the only compiler now
        }
    }
}
