using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    public GameObject PlayerMarker;
    public float PositionScale;
    public Vector2 positionOffset;
    GameObject FinishLine;
    List<GameObject> allPlayers;
    List<GameObject> allMarkers;
    int objectCount;
    
    // Start is called before the first frame update
    void Start()
    {
        objectCount = FindObjectsOfType<GameObject>().Length;


        allPlayers = new List<GameObject>();
        allMarkers = new List<GameObject>();


        foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
        {
            if (gameObject.tag.Equals("Player"))
                allPlayers.Add(gameObject);

            if (gameObject.tag.Equals("Finish"))
                FinishLine = gameObject;
        }

        for (int i = 0; i < allPlayers.Count; i++)
        {
            allMarkers.Add(Instantiate(PlayerMarker, transform));
            allMarkers[i].GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (objectCount != FindObjectsOfType<GameObject>().Length)
        {
            objectCount = FindObjectsOfType<GameObject>().Length;
            UpdateMarkers();
        }
        
        for (int i = 0; i < allPlayers.Count; i++)
        {
            allMarkers[i].GetComponent<RectTransform>().localPosition = 
                new Vector2((allPlayers[i].transform.position - FinishLine.transform.position).x, 
                (allPlayers[i].transform.position - FinishLine.transform.position).z) * PositionScale + positionOffset;
        }
    }

    private void UpdateMarkers()
    {
        for (int i = 0; i < allMarkers.Count; i++)
        {
            Destroy(allMarkers[i]);
        }
        
        allPlayers = new List<GameObject>();
        allMarkers = new List<GameObject>();

        foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
        {
            if (gameObject.tag.Equals("Player"))
                allPlayers.Add(gameObject);

            if (gameObject.tag.Equals("Finish"))
                FinishLine = gameObject;
        }

        for (int i = 0; i < allPlayers.Count; i++)
        {
            allMarkers.Add(Instantiate(PlayerMarker, transform));
            allMarkers[i].GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
