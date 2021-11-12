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
            var text = (string)Resources.ResourceManager.GetObject(resourceName);
            File.WriteAllText(fileName, text);
        }
    }
}