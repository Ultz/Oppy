// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;

namespace Ultz.Oppy.Scripting
{
    /// <summary>
    /// Encapsulates path matching modes of the default path matcher.
    /// </summary>
    [Flags]
    public enum PathMatchingMode
    {
        /// <summary>
        /// Match no paths.
        /// </summary>
        None = 0,

        /// <summary>
        /// Match file names that exactly match the script file name with its first extension stripped (i.e. the file name without
        /// the .csx at the end)
        /// </summary>
        ExactFileMatch = 1 << 0,

        /// <summary>
        /// Match directory names that exactly match the script file name with its first extension script (i.e. the file name
        /// without the .csx at the end), but not their subdirectories.
        /// </summary>
        ExactDirMatch = 1 << 1,

        /// <summary>
        /// Equivalent to <see cref="ExactFileMatch" /> and <see cref="ExactDirMatch" />
        /// </summary>
        ExactMatch = ExactFileMatch | ExactDirMatch,

        /// <summary>
        /// Matches file names that, when either their first extensions are stripped or un-stripped, match the script file name
        /// with its first extension stripped.
        /// </summary>
        AnyMatchingFileName = 1 << 2,

        /// <summary>
        /// Match directory names that exactly match the script file name with its first extension script (i.e. the file name
        /// without the .csx at the end), including their subdirectories.
        /// </summary>
        AnyMatchingDirOrSubDir = 1 << 3,

        /// <summary>
        /// Equivalent to <see cref="AnyMatchingFileName" /> and <see cref="AnyMatchingDirOrSubDir" />. Default.
        /// </summary>
        AnyMatching = AnyMatchingFileName | AnyMatchingDirOrSubDir,

        /// <summary>
        /// Equivalent to <see cref="AnyMatchingFileName" /> and <see cref="ExactDirMatch" />.
        /// </summary>
        AnyFileExactDir = AnyMatchingFileName | ExactDirMatch,

        /// <summary>
        /// Equivalent to <see cref="ExactFileMatch" /> and <see cref="AnyMatchingDirOrSubDir" />.
        /// </summary>
        ExactFileAnyDir = ExactFileMatch | AnyMatchingDirOrSubDir,

        /// <summary>
        /// The default path matching mode. Equivalent to <see cref="AnyMatching" />.
        /// </summary>
        Default = AnyMatching
    }
}