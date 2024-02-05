using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoosterBehavior : MonoBehaviour
{
    [SerializeField]
    Color BoosterColor;

    [SerializeField]
    List<GameObject> BoosterLayers;

    [SerializeField]
    TrailRenderer trailRenderer;

    List<MeshRenderer> meshRenderers;

    Camera mainCamera;

    Vector3 baseSize;

    Coroutine CurrentAnimation;

    public float sizeMultiplier = 1;

    bool on;

    public bool isOn()
    {
        return on;
    }

    //private void OnValidate()
    //{
        
    //    SetMaterialColor(BoosterColor * 2);
    //    SetMaterialAlpha(1);
    //    ResetBoosterSize();
    //}

    // Start is called before the first frame update
    void Start()
    {
        on = false;
        baseSize = transform.localScale;
        InitializeMeshRenderList();
        SetMaterialColor(BoosterColor * 2);
        SetMaterialAlpha(0);
        transform.localScale = new Vector3(0, 0, 0);

        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, baseSize * sizeMultiplier, (1 / 0.25f) * Time.deltaTime);
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
    }

    public void BoostStart()
    {
        on = true;
        if (CurrentAnimation != null)
            StopCoroutine(CurrentAnimation);

        CurrentAnimation = StartCoroutine(AnimateStart(0.4f));
    }

    public void BoostStop()
    {
        on = false;
        if (CurrentAnimation != null)
            StopCoroutine(CurrentAnimation);

        CurrentAnimation = StartCoroutine(AnimateStop(0.2f));
    }

    IEnumerator AnimateStart(float length)
    {
        float startAlpha = trailRenderer.material.color.a;
        Vector3 startSize = transform.localScale;
        float timestamp = Time.time;

        while (Time.time - timestamp < length)
        {
            SetMaterialAlpha(Mathf.Lerp(startAlpha + 0.1f, 1, Mathf.Pow((Time.time - timestamp) / length, 2)));
            transform.localScale = Vector3.Lerp(startSize, baseSize, Mathf.Pow((Time.time - timestamp) / length, 0.5f));
            yield return null;
        }
        SetMaterialAlpha(1);
        transform.localScale = baseSize;
    }

    IEnumerator AnimateOn()
    {
        while (false)
        {
            yield return null;
        }
    }

    IEnumerator AnimateStop(float length)
    {
        float startAlpha = trailRenderer.material.color.a;
        Vector3 startSize = transform.localScale;
        float timestamp = Time.time;

        while (Time.time - timestamp < length)
        {
            SetMaterialAlpha(Mathf.Lerp(startAlpha + 0.1f, 0, Mathf.Pow((Time.time - timestamp) / length, 0.5f)));
            transform.localScale = Vector3.Lerp(startSize, new Vector3 (0, 0, 0), Mathf.Pow((Time.time - timestamp) / length, 1f));
            yield return null;
        }
        SetMaterialAlpha(0);
        transform.localScale = new Vector3(0, 0, 0);
    }

    void SetMaterialAlpha(float alpha)
    {
        Material tempMat;
        for (int i = 0; i < BoosterLayers.Count; i++)
        {
            tempMat = meshRenderers[i].material;
            tempMat.color = new Color(tempMat.color.r, tempMat.color.g, tempMat.color.b, alpha);
        }

        tempMat = trailRenderer.material;
        Color ogColor = tempMat.color;
        tempMat.color = new Color(ogColor.r, ogColor.g, ogColor.b, alpha);
    }

    void SetMaterialColor(Color color)
    {
        Material tempMat;

        for (int i = 0; i < BoosterLayers.Count; i++)
        {
            tempMat = meshRenderers[i].material;
            tempMat.SetColor("_EmissionColor", color);
        }

        tempMat = trailRenderer.material;
        tempMat.color = color;
    }

    void InitializeMeshRenderList()
    {
        meshRenderers = new List<MeshRenderer>();
        for (int i = 0; i < BoosterLayers.Count; i++)
        {
            meshRenderers.Add(BoosterLayers[i].GetComponent<MeshRenderer>());
        }
    }

    public void ResetBoosterSize()
    {
        sizeMultiplier = 1;
        for (int i = 0; i < BoosterLayers.Count; i++)
        {
            BoosterLayers[i].transform.localScale = Vector3.one;
        }
        baseSize = transform.localScale;
    }
}
