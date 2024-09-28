using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ComputeLUT_Type1 : MonoBehaviour
{
    public int scatterIteratorCount = 2;
    public Vector2Int translucentLUTSize = new Vector2Int(256, 256);
    public Vector2Int outSunLightLUTSize = new Vector2Int(256, 256);
    public Vector2Int inSunLightLUTSize = new Vector2Int(256, 256);
    public Vector4 scatterLUTSize = new Vector4( 128, 32, 8, 32);
    public Shader TranslucentGenrater = null;
    public Shader OutSunLightLUTGenrater = null;
    public Shader InSunLightLUTGenrater = null;
    public Shader ScatterGenrater = null;
    public Material materialSkyLUT = null;
    public List<Material> materialCloudLUT = new List<Material>();
    private Material materialTranslucentGenrater = null;
    private Material materialOutSunLightLUTGenrater = null;
    private Material materialInSunLightLUTGenrater = null;
    private Material materialScatterGenrater = null;
    private RenderTexture atmosphereInfoMap = null;
    private RenderTexture translucentLUT = null;
    private RenderTexture outSunLightLUT = null;
    private RenderTexture inSunLightLUT = null;
    private RenderTexture scatterLUT_Reayleigh = null;
    private RenderTexture scatterLUT_Mie = null;
    private RenderTexture atmosphereInfoMapCache = null;
    // Start is called before the first frame update

    
#region propsIDs

    private static int propId_exposure              = Shader.PropertyToID("exposure");
    private static int propId_ground_refract        = Shader.PropertyToID("ground_refract");
    private static int propId_ground_light          = Shader.PropertyToID("ground_light");
    private static int propId_deltaL                = Shader.PropertyToID("deltaL");
    private static int propId_deltaW                = Shader.PropertyToID("deltaW");
    private static int propId_lengthL               = Shader.PropertyToID("lengthL");
    private static int propId_lengthW               = Shader.PropertyToID("lengthW");
    private static int propId_sunRadius             = Shader.PropertyToID("sunRadius");
    private static int propId_sunDistance           = Shader.PropertyToID("sunDistance");
    private static int propId_minh                  = Shader.PropertyToID("minh");
    private static int propId_maxh                  = Shader.PropertyToID("maxh");
    private static int propId_H_Reayleigh           = Shader.PropertyToID("H_Reayleigh");
    private static int propId_H_Mie                 = Shader.PropertyToID("H_Mie");
    private static int propId_H_OZone               = Shader.PropertyToID("H_OZone");
    private static int propId_D_OZone               = Shader.PropertyToID("D_OZone");
    private static int propId_reayleigh_scatter     = Shader.PropertyToID("reayleigh_scatter");
    private static int propId_molecule_absorb       = Shader.PropertyToID("molecule_absorb");
    private static int propId_OZone_absorb          = Shader.PropertyToID("OZone_absorb");
    private static int propId_mie_scatter           = Shader.PropertyToID("mie_scatter");
    private static int propId_mie_absorb            = Shader.PropertyToID("mie_absorb");
    private static int propId_mie_eccentricity      = Shader.PropertyToID("mie_eccentricity");
    private static int propId_sunColor              = Shader.PropertyToID("sunColor");
    private static int propId_scatterLUT_Size       = Shader.PropertyToID("scatterLUT_Size");
    private static int propId_translucentLUT        = Shader.PropertyToID("translucentLUT");
    private static int propId_outSunLightLUT        = Shader.PropertyToID("outSunLightLUT");
    private static int propId_inSunLightLUT         = Shader.PropertyToID("inSunLightLUT");
    private static int propId_scatterLUT_Reayleigh  = Shader.PropertyToID("scatterLUT_Reayleigh");
    private static int propId_scatterLUT_Mie        = Shader.PropertyToID("scatterLUT_Mie");
        
#endregion


    void UpdateMaterialDyn(Material material, float ground_refract, float ground_light)
    {
        if(material == null || materialSkyLUT == null) return;
        
        material.SetFloat(propId_exposure, materialSkyLUT.GetFloat(propId_exposure));
        material.SetFloat(propId_ground_refract, ground_refract);
        material.SetFloat(propId_ground_light, ground_light);
        material.SetVector(propId_sunColor, materialSkyLUT.GetVector(propId_sunColor));
        material.SetVector(propId_mie_eccentricity, materialSkyLUT.GetVector(propId_mie_eccentricity));
    }
    
    void UpdateMaterialStatic(Material material)
    {
        if(material == null || materialSkyLUT == null) return;

        material.SetFloat(propId_deltaL, materialSkyLUT.GetFloat(propId_deltaL));
        material.SetFloat(propId_deltaW, materialSkyLUT.GetFloat(propId_deltaW));
        material.SetFloat(propId_lengthL, materialSkyLUT.GetFloat(propId_lengthL));
        material.SetFloat(propId_lengthW, materialSkyLUT.GetFloat(propId_lengthW));
        material.SetFloat(propId_sunRadius, materialSkyLUT.GetFloat(propId_sunRadius));
        material.SetFloat(propId_sunDistance, materialSkyLUT.GetFloat(propId_sunDistance));
        material.SetFloat(propId_minh, materialSkyLUT.GetFloat(propId_minh));
        material.SetFloat(propId_maxh, materialSkyLUT.GetFloat(propId_maxh));
        material.SetFloat(propId_H_Reayleigh, materialSkyLUT.GetFloat(propId_H_Reayleigh));
        material.SetFloat(propId_H_Mie, materialSkyLUT.GetFloat(propId_H_Mie));
        material.SetFloat(propId_H_OZone, materialSkyLUT.GetFloat(propId_H_OZone));
        material.SetFloat(propId_D_OZone, materialSkyLUT.GetFloat(propId_D_OZone));
        material.SetVector(propId_reayleigh_scatter, materialSkyLUT.GetVector(propId_reayleigh_scatter));
        material.SetVector(propId_molecule_absorb, materialSkyLUT.GetVector(propId_molecule_absorb));
        material.SetVector(propId_OZone_absorb, materialSkyLUT.GetVector(propId_OZone_absorb));
        material.SetVector(propId_mie_scatter, materialSkyLUT.GetVector(propId_mie_scatter));
        material.SetVector(propId_mie_absorb, materialSkyLUT.GetVector(propId_mie_absorb));
    }
    
    void UpdateMaterialLUT(Material material)
    {
        if(material == null || materialSkyLUT == null) return;

        material.SetVector(propId_scatterLUT_Size, new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
        material.SetTexture(propId_translucentLUT, translucentLUT);
        material.SetTexture(propId_outSunLightLUT, outSunLightLUT);
        material.SetTexture(propId_inSunLightLUT, inSunLightLUT);
        material.SetTexture(propId_scatterLUT_Reayleigh, scatterLUT_Reayleigh);
        material.SetTexture(propId_scatterLUT_Mie, scatterLUT_Mie);
    }

    void Start()
    {
        if(TranslucentGenrater != null && ScatterGenrater != null && OutSunLightLUTGenrater != null && InSunLightLUTGenrater != null && materialSkyLUT != null)
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


            outSunLightLUT = new RenderTexture(outSunLightLUTSize.x, outSunLightLUTSize.y, 0);
            outSunLightLUT.useMipMap = false;
            outSunLightLUT.format = RenderTextureFormat.ARGBFloat;
            outSunLightLUT.wrapMode = TextureWrapMode.Clamp;
            outSunLightLUT.Create();

            
            inSunLightLUT = new RenderTexture(inSunLightLUTSize.x, inSunLightLUTSize.y, 0);
            inSunLightLUT.useMipMap = false;
            inSunLightLUT.format = RenderTextureFormat.ARGBFloat;
            inSunLightLUT.wrapMode = TextureWrapMode.Clamp;
            inSunLightLUT.Create();


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
            materialOutSunLightLUTGenrater = new Material(OutSunLightLUTGenrater);
            materialInSunLightLUTGenrater = new Material(InSunLightLUTGenrater);
            materialScatterGenrater = new Material(ScatterGenrater);

            UpdateMaterialStatic(materialTranslucentGenrater);
            UpdateMaterialLUT(materialTranslucentGenrater);
            Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);

            UpdateMaterialStatic(materialOutSunLightLUTGenrater);
            UpdateMaterialLUT(materialOutSunLightLUTGenrater);
            Graphics.Blit(null, outSunLightLUT, materialOutSunLightLUTGenrater);

            UpdateMaterialStatic(materialInSunLightLUTGenrater);
            UpdateMaterialLUT(materialInSunLightLUTGenrater);
            Graphics.Blit(null, inSunLightLUT, materialInSunLightLUTGenrater);

            UpdateMaterialStatic(materialScatterGenrater);
            UpdateMaterialLUT(materialScatterGenrater);
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
            UpdateMaterialLUT(materialSkyLUT);

            if(materialCloudLUT != null)
            {
                for(int i = 0; i < materialCloudLUT.Count; i++)
                {
                    Material cloud = materialCloudLUT[i];
                    if(cloud != null)
                    {
                        UpdateMaterialStatic(cloud);
                        UpdateMaterialLUT(cloud);
                    }
                }
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
        // GUI.DrawTexture(new Rect(0, 0, outSunLightLUT.width, outSunLightLUT.height), outSunLightLUT);
        // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT_Reayleigh.width, scatterLUT_Reayleigh.height), scatterLUT_Reayleigh);
    }

}
