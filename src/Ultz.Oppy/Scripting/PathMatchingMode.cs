using System;

namespace Ultz.Oppy.Scripting
{
    [Flags]
    public enum PathMatchingMode
    {
        None = 0,
        ExactFileMatch = 1 << 0,
        ExactDirMatch = 1 << 1,
        ExactMatch = ExactFileMatch | ExactDirMatch,
        AnyMatchingFileName = 1 << 2,
        AnyMatchingDirOrSubDir = 1 << 3,
        AnyMatching = AnyMatchingFileName | AnyMatchingDirOrSubDir,
        AnyFileExactDir = AnyMatchingFileName | ExactDirMatch,
        ExactFileAnyDir = ExactFileMatch | AnyMatchingDirOrSubDir,
        Default = AnyFileExactDir
    }
}