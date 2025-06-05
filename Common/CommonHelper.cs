using System;
using System.IO;

namespace Radar7D.Common
{
    public static class CommonHelper
    {
        public static string GetCertificateAbsolutePath(string certificateRelativePath)
        {
            string basePath = AppContext.BaseDirectory;

            if (certificateRelativePath.StartsWith("/") || certificateRelativePath.StartsWith("\\"))
            {
                certificateRelativePath = certificateRelativePath.Substring(1);
            }

            return Path.Combine(basePath, certificateRelativePath);
        }
    }
}
