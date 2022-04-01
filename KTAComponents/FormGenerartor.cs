using System;
using System.IO;

namespace KTAComponents
{
    public class FileManager
    {

        public void createFolderIfNeeded(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        // génération fichier depuis un string
        public void GenerateFileFromString(string file, string str)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(file))
                {

                    sw.Write(str);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur FormGenerator.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }
    }
}
