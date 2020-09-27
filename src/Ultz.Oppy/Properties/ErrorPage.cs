// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System.IO;

namespace Ultz.Oppy.Properties
{
    /// <summary>
    /// Encapsulates the default Oppy error page, embedded in the assembly.
    /// </summary>
    public static class ErrorPage
    {
        /// <summary>
        /// The HTML contents of the default error page.
        /// </summary>
        public static readonly string Content;

        static ErrorPage()
        {
            using var stream =
                typeof(ErrorPage).Assembly.GetManifestResourceStream(typeof(ErrorPage).FullName! + ".html") ??
                throw new FileNotFoundException();
            using var streamReader = new StreamReader(stream);
            Content = streamReader.ReadToEnd();
        }
    }
}