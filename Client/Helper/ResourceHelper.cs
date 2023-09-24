using System;
using System.ComponentModel.Design;
using System.IO;

namespace Client.Helper
{
    public static class ResourceHelper
    {
        /// <summary>
        /// extract a file from an embedded resource and save it to disk
        /// </summary>
        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            var obj = Resources.ResourceManager.GetObject(resourceName);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileInfo fileInfo = new FileInfo(assembly.Location);

            switch (obj)
            {
                case byte[] bytes:
                    File.WriteAllBytes(fileName, bytes);
                    File.SetCreationTimeUtc(fileName, fileInfo.LastWriteTime);
                    break;

                case string text:
                    File.WriteAllText(fileName, text);
                    File.SetCreationTimeUtc(fileName, fileInfo.LastWriteTime);
                    break;

                default:
                    throw new Exception("Resource is neither of type byte[] nor type text. Cannot save it!");
            }
        }
    }
}