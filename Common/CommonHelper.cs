using System;

namespace TemplateAngularCoreSAML.Common
{
    public static class CommonHelper
    {
        public static string GetCertificateAbsolutePath(string certificateRelativePath)
        {
            // Se obtiene la ruta donde se encuentra la DLL en ejecución.
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath;

            // Esta validación es para revisar desde donde se esta ejecutando la DLL.
            // Desde la Web App de Azure.
            if (currentPath.Contains(@"\site\wwwroot\"))
                absolutePath = currentPath + certificateRelativePath;
            else
                // Desde Debug
                absolutePath = currentPath.Substring(0, currentPath.LastIndexOf("TemplateAngularCoreSAML") + 23) + certificateRelativePath;

            return absolutePath;
        }
    }
}
