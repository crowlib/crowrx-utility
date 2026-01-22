using System.IO;
using UnityEngine;


namespace CrowRx.Utility
{
    public static class BytesFile
    {
        public static string CombinePersistentDataPath(string filename)
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }

        public static byte[] Read(string path)
        {
            return File.ReadAllBytes(path);
        }

        public static void Write(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public static byte[] ReadAtPersistentDataPath(string filename)
        {
            return Read(CombinePersistentDataPath(filename));
        }

        public static void WriteAtPersistentDataPath(string filename, byte[] bytes)
        {
            Write(CombinePersistentDataPath(filename), bytes);
        }
    }
}