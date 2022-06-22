using System;
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
            ApplyMaterialPropertyBlockSettings();
            //pBlock.SetTextureScale("_LightMap", new Vector2(lightmapCoords.x, lightmapCoords.y));
            //pBlock.SetTextureOffset("_LightMap", new Vector2(lightmapCoords.z, lightmapCoords.w));
        }
        public void ApplyMaterialPropertyBlockSettings(){
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
        }
        
#if UNITY_EDITOR
        public UnityEngine.Rendering.ShadowCastingMode shadowcasting;
        [Button]
        public void SetLightmapCoordsOnMaterial()
        {
            MeshRenderer rend = GetComponent<MeshRenderer>();
            Material mat = rend.sharedMaterial;
            mat.SetVector("_LightMap_ST", lightmapCoords);
            mat.SetTextureScale("_LightMap", new Vector2(lightmapCoords.x, lightmapCoords.y));
            mat.SetTextureOffset("_LightMap", new Vector2(lightmapCoords.z, lightmapCoords.w));
        }
        
        
        [Button]
        public void StoreCoordinate()
        {
            Renderer render = GetComponent<Renderer>();
            lightmapIndex = render.lightmapIndex;
            lightmapCoords = render.lightmapScaleOffset;
            
            var s = new SerializedObject(this);
            var prop = s.FindProperty(nameof(lightmapIndex));
            prop.intValue = render.lightmapIndex;
            
            prop = s.FindProperty(nameof(lightmapCoords));
            prop.vector4Value = render.lightmapScaleOffset;
            
            prop = s.FindProperty(nameof(shadowcasting));
            prop.intValue = (int)render.shadowCastingMode;
            
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(this);

            if (PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(this.gameObject);
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabRoot);
                if (string.IsNullOrEmpty(path)) {
                    Debug.LogWarning($"No path to prefab. Root = {prefabRoot} | ",gameObject);
                    return;
                }
                if (PrefabUtility.IsPartOfAnyPrefab(this) == false) {
                    PrefabUtility.ApplyAddedComponent(this,path,InteractionMode.AutomatedAction);
                }
                //try {
                if(PrefabUtility.HasPrefabInstanceAnyOverrides(prefabRoot,false)) {
                    PrefabUtility.ApplyObjectOverride(this, path, InteractionMode.AutomatedAction);
                }
                //}
               // catch (Exception e) {
                //   Debug.LogError(e.Message,gameObject);
               // }
            }
        }

        [Button]
        public void SetLightmapCoordinate()
        {
            MeshRenderer render = GetComponent<MeshRenderer>();
            SerializedObject serializeable = new SerializedObject(render);
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