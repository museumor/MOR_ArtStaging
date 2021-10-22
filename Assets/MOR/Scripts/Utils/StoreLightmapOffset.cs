using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace MOR.Museum
{

    public class StoreLightmapOffset : MonoBehaviour
    {
        public int lightmapIndex;
        public Vector4 lightmapCoords;

        public bool setPerObjectCoord;
        private static readonly int LightMapSt = Shader.PropertyToID("_LightMap_ST");
        public Texture2D lightmap;
        private static readonly int LightMap = Shader.PropertyToID("_LightMap");
        public void Start() {
            var rend = GetComponent<Renderer>();
            if (rend == null) {
                return;
            }

            if (setPerObjectCoord == false) {
                return;
            }
            MaterialPropertyBlock pBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(pBlock);
            pBlock.SetVector(LightMapSt, lightmapCoords);
            if (lightmap != null) {
                pBlock.SetTexture(LightMap,lightmap);
            }
            rend.SetPropertyBlock(pBlock);
            
            //pBlock.SetTextureScale("_LightMap", new Vector2(lightmapCoords.x, lightmapCoords.y));
            //pBlock.SetTextureOffset("_LightMap", new Vector2(lightmapCoords.z, lightmapCoords.w));
        }
#if UNITY_EDITOR
        [Button]
        public void SetLightmapCoordsOnMaterial()
        {
            var rend = GetComponent<MeshRenderer>();
            var mat = rend.sharedMaterial;
            mat.SetVector("_LightMap_ST", lightmapCoords);
            mat.SetTextureScale("_LightMap", new Vector2(lightmapCoords.x, lightmapCoords.y));
            mat.SetTextureOffset("_LightMap", new Vector2(lightmapCoords.z, lightmapCoords.w));
        }
        [Button]
        public void StoreCoordinate()
        {
            var render = GetComponent<Renderer>();
            lightmapIndex = render.lightmapIndex;
            lightmapCoords = render.lightmapScaleOffset;
            EditorUtility.SetDirty(this);
        }

        [Button]
        public void SetLightmapCoordinate()
        {
            var render = GetComponent<MeshRenderer>();
            var serializeable = new SerializedObject(render);
            /*var prop = serializeable.FindProperty(nameof(render.lightmapIndex));
            prop.intValue = lightmapIndex;
            prop = serializeable.FindProperty(nameof(render.lightmapScaleOffset));
            prop.vector4Value = lightmapCoords;
            serializeable.ApplyModifiedProperties();*/
            render.lightmapIndex = lightmapIndex;
            render.lightmapScaleOffset = lightmapCoords;
            PrefabUtility.RecordPrefabInstancePropertyModifications(render);
        }


#endif
    
    }
}