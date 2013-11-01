// <copyright file="QtWebKit.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class WebKit : QtCommon.WebKit
    {
        public WebKit()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}