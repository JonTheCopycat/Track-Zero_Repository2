using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;

sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;

        String currentAssembly = Assembly.GetExecutingAssembly().FullName;

        // In this case we are always using the current assembly
        assemblyName = currentAssembly;

        // Get the type using the typeName and assemblyName
        typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
            typeName, assemblyName));

        return typeToDeserialize;
    }
}

namespace Replays
{
    public static class ReplayStorage
    {
        public static string directory = "/replays/";
        public static string fileName = ".replay";

        public static Replay lastReplay = null;
        public static string selectedTrack = "";
        public static Replay selectedReplay = null;
        public static List<Replay> allReplays = new List<Replay>();


        public static void SaveReplay(string name, Replay replay)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // To prevent errors serializing between version number differences (e.g. Version 1 serializes, and Version 2 deserializes)
            formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

            string dir = Application.dataPath + directory;
            string fullDir = dir + name + fileName;


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(fullDir))
            {
                File.Delete(fullDir);
            }
            FileStream stream = new FileStream(fullDir, FileMode.Create);

            //string json = JsonUtility.ToJson(path);

            //File.WriteAllText(dir + fileName + "_" + name, json);

            formatter.Serialize(stream, replay);
            stream.Close();
            Debug.Log("File written to: " + dir);
        }

        public static Replay LoadReplay(string name)
        {
            string dir = Application.dataPath + directory + name + fileName;
            Debug.Log(dir);

            Replay replay = new Replay();

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

                    replay = (Replay)formatter.Deserialize(stream);
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
                return null;
            }

            return replay;
        }

        public static void ClearAllReplays()
        {
            string dir = Application.dataPath + directory;

            if (Directory.Exists(dir))
            {
                foreach (string sFile in Directory.GetFiles(dir, $"*{fileName}"))
                {
                    Debug.Log($"Deleted {sFile}");
                    File.Delete(sFile);
                }
            }
            else
                Debug.LogWarning("Directory not created yet, no replay files");
        }

        public static List<Replay> LoadAllReplays()
        {
            string[] result = new string[0];
            string dir = Application.dataPath + directory;

            allReplays.Clear();
            if (Directory.Exists(dir))
            {
                result = Directory.GetFiles(dir, $"*{fileName}");

                Replay tempReplay = new Replay();

                foreach (string file in result)
                {
                    Debug.Log($"{file}");

                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(file, FileMode.Open);

                    // To prevent errors serializing between version number differences (e.g. Version 1 serializes, and Version 2 deserializes)
                    formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

                    tempReplay = (Replay)formatter.Deserialize(stream);
                    allReplays.Add(tempReplay);
                    stream.Close();
                    try
                    {

                    }
                    catch
                    {
                        Debug.LogError("File failed to read");
                    }
                }
                try
                {

                }
                catch
                {
                    Debug.LogError($"Failed to get files in {dir}");
                }
            }
            else
                Debug.LogWarning("Directory not created yet, no replay files");

            return allReplays;
        }
    }
}

