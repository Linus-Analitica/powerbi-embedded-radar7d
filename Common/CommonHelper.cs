using System;

namespace TemplateAngularCoreSAML.Common
{
    public static class CommonHelper
    {
        public static string GetCertificateAbsolutePath(string certificateRelativePath)
        {
            // BaseDirectory apunta a la raíz de la app publicada (ej. wwwroot en Azure)
            string basePath = AppContext.BaseDirectory;

            // Elimina slash inicial en caso de estar presente
            if (certificateRelativePath.StartsWith("/") || certificateRelativePath.StartsWith("\\"))
            {
                certificateRelativePath = certificateRelativePath.Substring(1);
            }

            return Path.Combine(basePath, certificateRelativePath);
        }
    }
}
