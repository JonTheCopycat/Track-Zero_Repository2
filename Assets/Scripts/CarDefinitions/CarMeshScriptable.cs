using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarMeshScriptableObject", menuName = "ScriptableObjects/CarMeshs")]
public class CarMeshScriptable : ScriptableObject
{
    public GameObject[] carModels;
    public string[] carModelNames;

    public Dictionary<string, GameObject> carModelLibrary = new Dictionary<string, GameObject>();

    private void OnValidate()
    {
        for (int i = 0; i < carModelNames.Length; i++)
        {
            carModelLibrary.Add(carModelNames[i], carModels[i]);
        }
    }

    public GameObject GetGameObject(string name)
    {
        

        GameObject result;
        if (carModelLibrary.TryGetValue(name, out result))
        {
            return result;
        }
        else
        {
            for (int i = 0; i < carModels.Length; i++)
            {
                if (carModelNames[i].Equals(name))
                {
                    return carModels[i];
                }
            }
            Debug.LogError("Using an invalid car name");
            return null;
        }
    }
}
