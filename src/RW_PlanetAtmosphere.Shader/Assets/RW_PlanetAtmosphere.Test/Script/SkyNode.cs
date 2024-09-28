using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace RW_PlanetAtmosphere
{
    public abstract class SkyNode // : IExposable
    {
        private RenderTexture color;
        private RenderTexture depth;

        public RenderTexture ColorTexture
        {
            get
            {
                if (color == null || color.width != Screen.width || color.height != Screen.height)
                {
                    if (color != null) GameObject.Destroy(color);
                    color = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
                }
                return color;
            }
        }

        public RenderTexture DepthTexture
        {
            get
            {
                if (depth == null || depth.width != Screen.width || depth.height != Screen.height)
                {
                    if (depth != null) GameObject.Destroy(depth);
                    depth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.RFloat);
                }
                return depth;
            }
        }

        public abstract void GenBaseColor(CommandBuffer commandBuffer);
        public abstract void ApplyShadows(CommandBuffer commandBuffer, SkyNode other);
        public abstract void BlendTexture(CommandBuffer commandBuffer, RenderTexture sourceColor, RenderTexture sourceDepth, RenderTexture sourceIdMap, int id);
        // public abstract void ExposeData();
    }

    // public class SkyNode_Cloud : SkyNode
    // {

    // }
}