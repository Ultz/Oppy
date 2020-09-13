using Ultz.Oppy.Configuration;

namespace Ultz.Oppy.Core
{
    public class Oppy
    {
        public static readonly InstallationInfo InstallationInfo;

        static Oppy()
        {
            InstallationInfo = InstallationInfo.Get();
        }
    }
}