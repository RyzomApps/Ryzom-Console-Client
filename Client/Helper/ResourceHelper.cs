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

            switch (obj)
            {
                case byte[] bytes:
                    File.WriteAllBytes(fileName, bytes);
                    break;

                case string text:
                    File.WriteAllText(fileName, text);
                    break;

                default:
                    throw new Exception("Resource is neither of type byte[] nor type text. Cannot save it!");
            }
        }
    }
}