using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AI
{
    public static class PathDataManager
    {
        public static string directory = "/AIPaths/";
        public static string fileName = "AIPath.p";



        public static void SaveAIPath(string name, AIPath path)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            string dir = Application.streamingAssetsPath + directory;
            string fullDir = dir + name + "_" + fileName;


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileStream stream = new FileStream(fullDir, FileMode.Create);

            // To prevent errors serializing between version number differences (e.g. Version 1 serializes, and Version 2 deserializes)
            formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

            //string json = JsonUtility.ToJson(path);

            //File.WriteAllText(dir + fileName + "_" + name, json);

            formatter.Serialize(stream, path);
            stream.Close();
            Debug.Log("File written to: " + dir);
        }

        public static AIPath LoadAIPath(string name)
        {
            string dir = Application.streamingAssetsPath + directory + name + "_" + fileName;
            Debug.Log(dir);

            AIPath path = new AIPath();

            if (File.Exists(dir))
            {
                //string json = File.ReadAllText(dir);
                //Debug.Log(json);
                //path = JsonUtility.FromJson<AIPath>(json);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(dir, FileMode.Open);

                    // To prevent errors serializing between version number differences (e.g. Version 1 serializes, and Version 2 deserializes)
                    formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

                    path = (AIPath)formatter.Deserialize(stream);
                    stream.Close();
                }
                catch
                {
                    Debug.LogError("File failed to read");
                }
            }
            else
            {
                Debug.LogWarning("File does not exists");
            }

            return path;
        }
    }
}
