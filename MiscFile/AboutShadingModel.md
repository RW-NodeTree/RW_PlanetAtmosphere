# About the Shading Model

## Logic:

This shading model processes the correct colors for light, shadow, and translucent objects through the following steps:

1. Patch screen space shadow.
2. Compute screen space shadows for each translucent object surface.
3. Draw translucent objects with the primary light.
4. Add translucent objects with other lights.
5. Select the next translucent object and repeat from step 2.

## Screen Space Shadow Rendering Patch `(LightEvent.AfterScreenspaceMask)`:

This step applies the light color of non-translucent objects when light penetrates translucent objects.

- **Apply Translucent Shadow:**
    - Compute shadows for all translucent objects and blend the results into `Zero SrcColor`.

## Camera Rendering Patch `(CameraEvent.BeforeImageEffects)`:

This step draws and applies the light of translucent objects.

- **Compute Screen-Space Shadow Map for Primary Light:**
    - For each translucent object, use the light-space shadow map to compute shadows in screen-space texture, blending the results into `Zero SrcColor`.

- **Draw Translucent Objects in Primary Light with Shadow Map:**
    - Blend the background with the translucent objects using the primary light.

- **Compute Screen-Space Shadow Map for Other Lights:**
    - For each translucent object, use the light-space shadow map to compute shadows in screen-space texture, blending the results into `Zero SrcColor`.

- **Draw Translucent Objects in Other Lights with Shadow Map:**
    - Blend the results using `One One`. If there are more lights to blend, the renderer will return to the previous step.
