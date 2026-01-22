// ReSharper disable InconsistentNaming

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace CrowRx.Utility
{
    public static class PlayerPrefUtil
    {
        private static BinaryFormatter _binaryFormatter;


        // serializableObject is any struct or class marked with [Serializable]
        public static void SaveObjectToPlayerPref(string prefKey, object serializableObject)
        {
            using MemoryStream memoryStream = new();

            _binaryFormatter ??= new BinaryFormatter();
            _binaryFormatter.Serialize(memoryStream, serializableObject);

            PlayerPrefs.SetString(prefKey, System.Convert.ToBase64String(memoryStream.ToArray()));
        }

        public static object LoadObjectFromPlayerPref(string prefKey)
        {
            string tmp = PlayerPrefs.GetString(prefKey, string.Empty);
            if (tmp == string.Empty)
            {
                return null;
            }

            using MemoryStream memoryStream = new(System.Convert.FromBase64String(tmp));

            _binaryFormatter ??= new BinaryFormatter();
            return _binaryFormatter.Deserialize(memoryStream);
        }
    }
}