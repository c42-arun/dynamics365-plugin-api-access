using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace spf_api.Helpers
{
    public class ResourcesHelper
    {
        public string GetAmotizationSpreadSheetFileName()
        {
            string resourceName = "loan-amortization-schedule";

            Stream fileStream = GetResourceFileStream(resourceName);

            if (fileStream == null) return string.Empty;

            string fileNameWithPath = Path.Combine(Path.GetTempPath(), $"{resourceName}.xlsx");

            if (File.Exists(fileNameWithPath)) File.Delete(fileNameWithPath);

            CopyStream(fileStream, fileNameWithPath);

            return fileNameWithPath;
        }

        // https://stackoverflow.com/questions/8083473/open-excel-workbook-file-using-embedded-resource
        private Stream GetResourceFileStream(string fileName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            // Get all embedded resources
            string[] arrResources = currentAssembly.GetManifestResourceNames();

            foreach (string resourceName in arrResources)
            {
                if (resourceName.Contains(fileName))
                {
                    return currentAssembly.GetManifestResourceStream(resourceName);
                }
            }

            return null;
        }

        // https://stackoverflow.com/questions/411592/how-do-i-save-a-stream-to-a-file-in-c
        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}