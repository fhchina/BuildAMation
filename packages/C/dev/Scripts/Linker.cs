// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportLinkerOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalLinkerOptionsDelegateAttribute : System.Attribute
    {
    }

    public abstract class Linker
    {
        public abstract string ExecutableCPlusPlus(Opus.Core.Target target);

        protected abstract string StartLibraryList
        {
            get;
        }

        protected abstract string EndLibraryList
        {
            get;
        }

        public void AppendLibrariesToCommandLine(Opus.Core.StringArray commandLineBuilder,
                                                 ILinkerOptions linkerOptions,
                                                 Opus.Core.StringArray otherLibraryPaths)
        {
            bool includeStandardLibraries = linkerOptions.DoNotAutoIncludeStandardLibraries && linkerOptions.StandardLibraries.Count > 0;
            bool includeOtherLibraries = (null != otherLibraryPaths) && (otherLibraryPaths.Count > 0);
            if (includeStandardLibraries ||
                (linkerOptions.Libraries.Count > 0) ||
                includeOtherLibraries)
            {
                commandLineBuilder.Add(this.StartLibraryList);

                if (includeOtherLibraries)
                {
                    commandLineBuilder.AddRange(otherLibraryPaths);
                }

                foreach (string libraryPath in linkerOptions.Libraries)
                {
                    if (libraryPath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("\"{0}\"", libraryPath));
                    }
                    else
                    {
                        commandLineBuilder.Add(libraryPath);
                    }
                }

                if (includeStandardLibraries)
                {
                    foreach (string standardLibraryPath in linkerOptions.StandardLibraries)
                    {
                        if (standardLibraryPath.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("\"{0}\"", standardLibraryPath));
                        }
                        else
                        {
                            commandLineBuilder.Add(standardLibraryPath);
                        }
                    }
                }

                commandLineBuilder.Add(this.EndLibraryList);
            }
        }
    }
}