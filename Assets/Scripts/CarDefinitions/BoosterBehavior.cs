using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cars
{
    public class BoosterBehavior : MonoBehaviour
    {
        [SerializeField]
        Color BoosterColor;

        [SerializeField]
        List<GameObject> BoosterLayers;

        [SerializeField]
        TrailRenderer trailRenderer;

        [SerializeField]
        List<ParticleSystem> particleSystems;

        List<MeshRenderer> meshRenderers;

        Camera mainCamera;

        Vector3 baseSize;

        Gradient ParticleGradient;

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
            SetMaterialColor(BoosterColor);
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
            for (int i = 0; i < BoosterLayers.Count; i++)
            {
                BoosterLayers[i].transform.LookAt(mainCamera.transform);
                BoosterLayers[i].transform.Rotate(0, 180, 0);
            }

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
                transform.localScale = Vector3.Lerp(startSize, new Vector3(0, 0, 0), Mathf.Pow((Time.time - timestamp) / length, 1f));
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

            //tempMat = trailRenderer.material;
            //Color ogColor = tempMat.color;
            //tempMat.color = new Color(ogColor.r, ogColor.g, ogColor.b, alpha);

            for (int i = 0; i < particleSystems.Count; i++)
            {
                var main = particleSystems[i].main;

                main.startColor = new Color(1, 1, 1, alpha);
            }

        }

        void SetMaterialColor(Color color)
        {
            Material tempMat;

            for (int i = 0; i < BoosterLayers.Count; i++)
            {
                tempMat = meshRenderers[i].material;
                tempMat.SetColor("_EmissionColor", color * 2);
            }

            tempMat = trailRenderer.material;
            tempMat.color = color * 2;

            ParticleGradient = new Gradient();
            float h = 0;
            float s = 1;
            float v = 1;
            Color.RGBToHSV(color, out h, out s, out v);
            //Debug.Log("H: " + h + " S: " + s + " V: " + v);
            Color hueShift = Color.HSVToRGB(Mathf.Repeat(h - 0.1f, 1), s, v * 0.5f);
            //Debug.Log("Original: " + color.ToString() + "\t Shifted: " + hueShift.ToString());

            // Blend color from red at 0% to blue at 100%
            var colors = new GradientColorKey[4];
            colors[0] = new GradientColorKey(color + Color.white * 0.5f, 0f);
            colors[1] = new GradientColorKey(color, 0.2f);
            colors[2] = new GradientColorKey(hueShift, 0.5f);
            colors[3] = new GradientColorKey(hueShift * 0.1f, 1.0f);

            // Blend alpha from opaque at 0% to transparent at 100%
            var alphas = new GradientAlphaKey[3];
            alphas[0] = new GradientAlphaKey(0.0f, 0.0f);
            alphas[1] = new GradientAlphaKey(1.0f, 0.2f);
            alphas[2] = new GradientAlphaKey(0.0f, 1.0f);

            ParticleGradient.SetKeys(colors, alphas);

            for (int i = 0; i < particleSystems.Count; i++)
            {
                var col = particleSystems[i].colorOverLifetime;
                col.enabled = true;

                col.color = ParticleGradient;
            }
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
}
