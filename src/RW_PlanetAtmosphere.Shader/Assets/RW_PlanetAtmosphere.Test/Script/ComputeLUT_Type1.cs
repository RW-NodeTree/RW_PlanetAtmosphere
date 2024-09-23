using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ComputeLUT_Type1 : MonoBehaviour
{
    public int scatterIteratorCount = 2;
    public Vector2Int translucentLUTSize = new Vector2Int(256, 256);
    public Vector4 scatterLUTSize = new Vector4( 128, 32, 8, 32);
    public Shader TranslucentGenrater = null;
    public Shader ScatterGenrater = null;
    public Material materialSkyLUT = null;
    public Material materialCityLight = null;
    public List<Material> materialCloudLUT = new List<Material>();
    private Material materialTranslucentGenrater = null;
    private Material materialScatterGenrater = null;
    private RenderTexture atmosphereInfoMap = null;
    private RenderTexture translucentLUT = null;
    private RenderTexture scatterLUT_Reayleigh = null;
    private RenderTexture scatterLUT_Mie = null;
    private RenderTexture atmosphereInfoMapCache = null;
    // Start is called before the first frame update
    void Start()
    {
        if(TranslucentGenrater != null && ScatterGenrater != null && materialSkyLUT != null)
        {
            // atmosphereInfoMap = new RenderTexture(atmosphereInfoMapSize.x << 4, atmosphereInfoMapSize.y << 4, 0);
            // atmosphereInfoMap.useMipMap = false;
            // atmosphereInfoMap.format = RenderTextureFormat.ARGBHalf;
            // atmosphereInfoMap.wrapMode = TextureWrapMode.Repeat;
            // atmosphereInfoMap.Create();
            // atmosphereInfoMapCache = new RenderTexture(atmosphereInfoMapSize.x << 4, atmosphereInfoMapSize.y << 4, 0);
            // atmosphereInfoMapCache.useMipMap = false;
            // atmosphereInfoMapCache.format = RenderTextureFormat.ARGBHalf;
            // atmosphereInfoMapCache.wrapMode = TextureWrapMode.Repeat;
            // atmosphereInfoMapCache.Create();
            translucentLUT = new RenderTexture(translucentLUTSize.x, translucentLUTSize.y, 0);
            //translucentLUT.enableRandomWrite = true;
            translucentLUT.useMipMap = false;
            translucentLUT.format = RenderTextureFormat.ARGBFloat;
            translucentLUT.wrapMode = TextureWrapMode.Clamp;
            //translucentLUT.filterMode = FilterMode.Point;
            translucentLUT.Create();
            scatterLUT_Reayleigh = new RenderTexture(((int)scatterLUTSize.x * (int)scatterLUTSize.z), ((int)scatterLUTSize.y * (int)scatterLUTSize.w), 0);
            //scatterLUT_Reayleigh.enableRandomWrite = true;
            scatterLUT_Reayleigh.useMipMap = false;
            scatterLUT_Reayleigh.format = RenderTextureFormat.ARGBHalf;
            // scatterLUT_Reayleigh.format = RenderTextureFormat.ARGBFloat;
            scatterLUT_Reayleigh.wrapMode = TextureWrapMode.Clamp;
            //scatterLUT_Reayleigh.filterMode = FilterMode.Point;
            scatterLUT_Reayleigh.Create();
            scatterLUT_Mie = new RenderTexture(((int)scatterLUTSize.x * (int)scatterLUTSize.z), ((int)scatterLUTSize.y * (int)scatterLUTSize.w), 0);
            //scatterLUT_Mie.enableRandomWrite = true;
            scatterLUT_Mie.useMipMap = false;
            scatterLUT_Mie.format = RenderTextureFormat.ARGBHalf;
            // scatterLUT_Mie.format = RenderTextureFormat.ARGBFloat;
            scatterLUT_Mie.wrapMode = TextureWrapMode.Clamp;
            //scatterLUT_Mie.filterMode = FilterMode.Point;
            scatterLUT_Mie.Create();

            materialTranslucentGenrater = new Material(TranslucentGenrater);
            materialScatterGenrater = new Material(ScatterGenrater);

            materialTranslucentGenrater.SetTexture("translucentLUT", translucentLUT);
            materialTranslucentGenrater.SetVector("reayleigh_scatter", materialSkyLUT.GetVector("reayleigh_scatter"));
            materialTranslucentGenrater.SetVector("molecule_absorb", materialSkyLUT.GetVector("molecule_absorb"));
            materialTranslucentGenrater.SetVector("OZone_absorb", materialSkyLUT.GetVector("OZone_absorb"));
            materialTranslucentGenrater.SetVector("mie_scatter", materialSkyLUT.GetVector("mie_scatter"));
            materialTranslucentGenrater.SetVector("mie_absorb", materialSkyLUT.GetVector("mie_absorb"));
            materialTranslucentGenrater.SetFloat("minh", materialSkyLUT.GetFloat("minh"));
            materialTranslucentGenrater.SetFloat("maxh", materialSkyLUT.GetFloat("maxh"));
            materialTranslucentGenrater.SetFloat("H_Reayleigh", materialSkyLUT.GetFloat("H_Reayleigh"));
            materialTranslucentGenrater.SetFloat("H_Mie", materialSkyLUT.GetFloat("H_Mie"));
            materialTranslucentGenrater.SetFloat("H_OZone", materialSkyLUT.GetFloat("H_OZone"));
            materialTranslucentGenrater.SetFloat("D_OZone", materialSkyLUT.GetFloat("D_OZone"));
            Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);

            materialScatterGenrater.SetTexture("translucentLUT", translucentLUT);
            materialScatterGenrater.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
            materialScatterGenrater.SetVector("reayleigh_scatter", materialSkyLUT.GetVector("reayleigh_scatter"));
            materialScatterGenrater.SetVector("molecule_absorb", materialSkyLUT.GetVector("molecule_absorb"));
            materialScatterGenrater.SetVector("OZone_absorb", materialSkyLUT.GetVector("OZone_absorb"));
            materialScatterGenrater.SetVector("mie_scatter", materialSkyLUT.GetVector("mie_scatter"));
            materialScatterGenrater.SetVector("mie_absorb", materialSkyLUT.GetVector("mie_absorb"));
            materialScatterGenrater.SetFloat("deltaL", materialSkyLUT.GetFloat("deltaL"));
            materialScatterGenrater.SetFloat("deltaW", materialSkyLUT.GetFloat("deltaW"));
            materialScatterGenrater.SetFloat("lengthL", materialSkyLUT.GetFloat("lengthL"));
            materialScatterGenrater.SetFloat("lengthW", materialSkyLUT.GetFloat("lengthW"));
            materialScatterGenrater.SetFloat("minh", materialSkyLUT.GetFloat("minh"));
            materialScatterGenrater.SetFloat("maxh", materialSkyLUT.GetFloat("maxh"));
            materialScatterGenrater.SetFloat("H_Reayleigh", materialSkyLUT.GetFloat("H_Reayleigh"));
            materialScatterGenrater.SetFloat("H_Mie", materialSkyLUT.GetFloat("H_Mie"));
            materialScatterGenrater.SetFloat("H_OZone", materialSkyLUT.GetFloat("H_OZone"));
            materialScatterGenrater.SetFloat("D_OZone", materialSkyLUT.GetFloat("D_OZone"));
            materialScatterGenrater.SetFloat("sunPerspective", materialSkyLUT.GetFloat("sunPerspective"));
            //Graphics.Blit(null, scatterLUT, materialScatterGenrater);
            Graphics.SetRenderTarget(new RenderBuffer[] { scatterLUT_Reayleigh.colorBuffer, scatterLUT_Mie.colorBuffer }, scatterLUT_Reayleigh.depthBuffer);
            Graphics.Blit(null, materialScatterGenrater);
            RenderTexture.active = null;


            // Graphics.Blit(null, atmosphereInfoMap, materialCloudGenrater);
            // materialCloudGenrater.SetTexture("cloudInfoMap", atmosphereInfoMap);

            //int scatterIteratorComputeKernel = scatterIteratorCompute.FindKernel("CSMain");
            //scatterIteratorCompute.Dispatch(scatterIteratorComputeKernel, scatterLUTSize.x, scatterLUTSize.y, scatterLUTSize.z << 4);

            //materialLUT.SetFloat("mie_scatter", 3.996f);
            //materialLUT.SetFloat("mie_absorb", 1.11f);
            //materialLUT.SetFloat("minh", 63.71393f);
            materialSkyLUT.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
            materialSkyLUT.SetTexture("translucentLUT", translucentLUT);
            materialSkyLUT.SetTexture("scatterLUT_Reayleigh", scatterLUT_Reayleigh);
            materialSkyLUT.SetTexture("scatterLUT_Mie", scatterLUT_Mie);

            if(materialCloudLUT != null)
            {
                for(int i = 0; i < materialCloudLUT.Count; i++)
                {
                    Material cloud = materialCloudLUT[i];
                    if(cloud != null)
                    {
                        // cloud.SetFloat("exposure", materialSkyLUT.GetFloat("exposure"));
                        // cloud.SetFloat("ground_refract", materialSkyLUT.GetFloat("ground_refract"));
                        // cloud.SetFloat("ground_light", materialSkyLUT.GetFloat("ground_light"));
                        cloud.SetFloat("deltaL", materialSkyLUT.GetFloat("deltaL"));
                        cloud.SetFloat("deltaW", materialSkyLUT.GetFloat("deltaW"));
                        cloud.SetFloat("lengthL", materialSkyLUT.GetFloat("lengthL"));
                        cloud.SetFloat("lengthW", materialSkyLUT.GetFloat("lengthW"));
                        cloud.SetFloat("sunPerspective", materialSkyLUT.GetFloat("sunPerspective"));
                        cloud.SetFloat("minh", materialSkyLUT.GetFloat("minh"));
                        cloud.SetFloat("maxh", materialSkyLUT.GetFloat("maxh"));
                        cloud.SetVector("SunColor", materialSkyLUT.GetVector("SunColor"));
                        cloud.SetVector("reayleigh_scatter", materialSkyLUT.GetVector("reayleigh_scatter"));
                        cloud.SetVector("molecule_absorb", materialSkyLUT.GetVector("molecule_absorb"));
                        cloud.SetVector("OZone_absorb", materialSkyLUT.GetVector("OZone_absorb"));
                        cloud.SetVector("mie_scatter", materialSkyLUT.GetVector("mie_scatter"));
                        cloud.SetVector("mie_absorb", materialSkyLUT.GetVector("mie_absorb"));
                        cloud.SetVector("mie_eccentricity", materialSkyLUT.GetVector("mie_eccentricity"));
                        cloud.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
                        // cloud.SetTexture("cloudTexture", atmosphereInfoMap);
                        cloud.SetTexture("translucentLUT", translucentLUT);
                        cloud.SetTexture("scatterLUT_Reayleigh", scatterLUT_Reayleigh);
                        cloud.SetTexture("scatterLUT_Mie", scatterLUT_Mie);
                    }
                }
            }
            if(materialCityLight != null)
            {
                
                // materialCityLight.SetFloat("exposure", materialSkyLUT.GetFloat("exposure"));
                // materialCityLight.SetFloat("ground_refract", materialSkyLUT.GetFloat("ground_refract"));
                // materialCityLight.SetFloat("ground_light", materialSkyLUT.GetFloat("ground_light"));
                materialCityLight.SetFloat("deltaL", materialSkyLUT.GetFloat("deltaL"));
                materialCityLight.SetFloat("deltaW", materialSkyLUT.GetFloat("deltaW"));
                materialCityLight.SetFloat("lengthL", materialSkyLUT.GetFloat("lengthL"));
                materialCityLight.SetFloat("lengthW", materialSkyLUT.GetFloat("lengthW"));
                materialCityLight.SetFloat("minh", materialSkyLUT.GetFloat("minh"));
                materialCityLight.SetFloat("maxh", materialSkyLUT.GetFloat("maxh"));
                materialCityLight.SetVector("SunColor", materialSkyLUT.GetVector("SunColor"));
                materialCityLight.SetVector("reayleigh_scatter", materialSkyLUT.GetVector("reayleigh_scatter"));
                materialCityLight.SetVector("molecule_absorb", materialSkyLUT.GetVector("molecule_absorb"));
                materialCityLight.SetVector("OZone_absorb", materialSkyLUT.GetVector("OZone_absorb"));
                materialCityLight.SetVector("mie_scatter", materialSkyLUT.GetVector("mie_scatter"));
                materialCityLight.SetVector("mie_absorb", materialSkyLUT.GetVector("mie_absorb"));
                materialCityLight.SetVector("mie_eccentricity", materialSkyLUT.GetVector("mie_eccentricity"));
                materialCityLight.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
                // materialCityLight.SetTexture("cloudTexture", atmosphereInfoMap);
                materialCityLight.SetTexture("translucentLUT", translucentLUT);
                materialCityLight.SetTexture("scatterLUT_Reayleigh", scatterLUT_Reayleigh);
                materialCityLight.SetTexture("scatterLUT_Mie", scatterLUT_Mie);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        // if(TranslucentGenrater != null && ScatterGenrater != null && materialSkyLUT != null && materialCloudLUT != null && materialCloudGenrater != null)
        // {
        //     Graphics.Blit(null, atmosphereInfoMapCache, materialCloudGenrater);
        //     Graphics.Blit(atmosphereInfoMapCache, atmosphereInfoMap);
        // }
    }

    void OnGUI()
    {
        if(Input.GetKeyUp(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            ScreenCapture.CaptureScreenshot($"Screenshot-{DateTime.Now.ToString("yyyy-MM-dd-")}{DateTime.Now.Ticks - DateTime.Today.Ticks}.png");
        // GUI.DrawTexture(new Rect(0, 0, translucentLUT.width, translucentLUT.height), translucentLUT);
        // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT_Reayleigh.width, scatterLUT_Reayleigh.height), scatterLUT_Reayleigh);
    }
}
