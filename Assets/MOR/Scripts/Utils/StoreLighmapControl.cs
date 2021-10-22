using System;
using EasyButtons;
using UnityEngine;

namespace MOR.Museum {
	public class StoreLighmapControl : MonoBehaviour {
    public bool assignLightmapToScript;
	public static string lightmapShader = "Assets\\Submodules\\Industries\\Art\\Shaders\\Standard_CustomLightmep.shader";
	private static string[] swapableShaders = {"Standard", "_MOR/Standard/Standard Stencil"}; //shaders we can swap to 'Standard customLightmap
	private static readonly int LightMap = Shader.PropertyToID("_LightMap");

	#if UNITY_EDITOR
		[Button]
		public void AddComponentsToLightmapped() {
			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);
			int countL = 0;
			int countN = 0;

			foreach (MeshRenderer meshRenderer in meshes) {
				if ( meshRenderer.lightmapIndex >= 0 && meshRenderer.scaleInLightmap != 0 ) {
					//var control = meshRenderer.gameObject.AddComponentIfNeeded<StoreLightmapOffset>();
					StoreLightmapOffset control = meshRenderer.GetComponent<StoreLightmapOffset>();
					if (control == null) {
						control = meshRenderer.gameObject.AddComponent<StoreLightmapOffset>();
					}

					control.setPerObjectCoord = true;
					control.StoreCoordinate();
					countL++;
                    if(assignLightmapToScript) {
	                    if(meshRenderer.lightmapIndex < LightmapSettings.lightmaps.Length && meshRenderer.lightmapIndex >= 0) {
		                   // Debug.Log($" len = {LightmapSettings.lightmaps.Length} :  index {meshRenderer.lightmapIndex}");
		                   control.lightmap = LightmapSettings.lightmaps[meshRenderer.lightmapIndex]?.lightmapColor;
	                    }
                    }
				} else {
					countN++;
				}
			}

			Debug.Log($"Lightmappe = {countL} Not = {countN}");
		}

		
		[Button]
		public void ChangeShader() {
			var shader = Shader.Find("_MOR/Standard/Custom Lightmap");
			if (shader != null) {
				Debug.Log("FOUND!");
			}

			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);

			foreach (MeshRenderer meshRenderer in meshes) {
				if (meshRenderer.lightmapIndex < 0 || meshRenderer.scaleInLightmap == 0) {
					Debug.Log($"Skip {meshRenderer.name} - scale {meshRenderer.scaleInLightmap}");
					continue;
				}
				var mat = meshRenderer.sharedMaterial;
				bool swappable = Array.IndexOf(swapableShaders, mat.shader.name) >= 0;
				if (mat == null || swappable == false) {
					if(mat!=null) {
						Debug.Log($"{mat.shader.name}");
					}

					continue;
				}

				if (mat.shader != shader) {
					mat.shader = shader;
					if (meshRenderer.lightmapIndex >= 0 && LightmapSettings.lightmaps.Length > meshRenderer.lightmapIndex) {
						var lm = LightmapSettings.lightmaps[meshRenderer.lightmapIndex];

						mat.SetTexture(LightMap, lm.lightmapColor);
						mat.SetInt("_UseAmbientOverride", 1);
						mat.EnableKeyword("_USE_AMBIENT_OVERRIDE");
						mat.EnableKeyword("_LIGHTMAP");
						mat.EnableKeyword("_DISABLEDIRECTIONAL");
						mat.SetInt("_UseLightmap", 1);
						mat.SetInt("_DisableDirectional", 1);
					}
				}
			}
		}
	#endif
	}
}