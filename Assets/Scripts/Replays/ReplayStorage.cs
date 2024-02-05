using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ReplayStorage
{
    public static string directory = "/replays/";
    public static string fileName = ".replay";

    public static Replay lastReplay = null;

    public static void SaveReplay(string name, Replay replay)
    {
        BinaryFormatter formatter = new BinaryFormatter();
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
            foreach(string sFile in Directory.GetFiles(dir, $"*{fileName}"))
            {
                Debug.Log($"Deleted {sFile}");
                File.Delete(sFile);
            }
        }
        else
            Debug.LogWarning("Directory not created yet, no replay files");
    }

    public static string[] GetAllReplays()
    {
        string[] result = new string[0];
        string dir = Application.dataPath + directory;

        if (Directory.Exists(dir))
        {
            try
            {
                result = Directory.GetFiles(dir, "*.replay");
            }
            catch
            {
                Debug.LogError($"Failed to get files in {dir}");
            }
        }
        else
            Debug.LogWarning("Directory not created yet, no replay files");

        return result;

    }
}
