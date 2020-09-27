// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.IO;

namespace Ultz.Oppy.Content
{
    internal readonly struct RegisteredContent : IEquatable<RegisteredContent>
    {
        public RegisteredContent(DateTime lastAccess, DateTime lastModified, FileAttributes attributes, long length)
        {
            LastAccess = lastAccess;
            LastModified = lastModified;
            Attributes = attributes;
            Length = length;
        }

        public static RegisteredContent CreateFrom(FileInfo info)
        {
            return new RegisteredContent(info.LastAccessTime, info.LastWriteTime, info.Attributes, info.Length);
        }

        public DateTime LastAccess { get; }
        public DateTime LastModified { get; }
        public FileAttributes Attributes { get; }
        public long Length { get; }

        public static bool operator ==(RegisteredContent left, RegisteredContent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RegisteredContent left, RegisteredContent right)
        {
            return !(left == right);
        }

        public bool Equals(RegisteredContent other)
        {
            return LastAccess.Equals(other.LastAccess) && LastModified.Equals(other.LastModified) &&
                   Attributes == other.Attributes && Length == other.Length;
        }

        public override bool Equals(object? obj)
        {
            return obj is RegisteredContent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LastAccess, LastModified, (int) Attributes, Length);
        }
    }
}