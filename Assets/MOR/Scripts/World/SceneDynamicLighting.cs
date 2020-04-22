using UnityEngine;
using UnityEngine.Rendering;


namespace MOR.Museum
{
    public class SceneDynamicLighting : MonoBehaviour
    {
        private void Start()
        {
            MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in allRenderers)
            {
                bool usesLightmaps = meshRenderer.lightmapIndex >= 0;
                if (usesLightmaps && meshRenderer.shadowCastingMode == ShadowCastingMode.On)
                {
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                } else if (usesLightmaps && meshRenderer.shadowCastingMode == ShadowCastingMode.TwoSided)
                {
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                
                if (usesLightmaps && meshRenderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                {
                    meshRenderer.receiveShadows = false;
                    meshRenderer.enabled = false;
                }
            }
        }
    }
}
