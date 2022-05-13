using System;
using System.Collections.Generic;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace MOR.Museum {
	public class StoreLighmapControl : MonoBehaviour {
		public bool assignLightmapToScript;
		public static string lightmapShader = "Assets\\Submodules\\Industries\\Art\\Shaders\\Standard_CustomLightmap.shader";
		private static string[] swapableShaders = { "Standard", "_MOR/Standard/Standard Stencil" }; //shaders we can swap to 'Standard customLightmap
		private static readonly int LightMap = Shader.PropertyToID("_LightMap");

		[Serializable]
		public class LightState {
			public Light light;
			public bool activeState;
			public bool enabled;
#if UNITY_EDITOR
			public void SetSceneState() {
				light.enabled = enabled;
				light.gameObject.SetActive(activeState);
				EditorUtility.SetDirty(light);
			}
#endif
		}

#if UNITY_EDITOR
		public List<LightState> lightStates = new List<LightState>();


		[Button]
		public void StoreLightStates() {
			GameObject scenePrefab = gameObject;
			GameObject diskPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
			Light[] lights = scenePrefab.GetComponentsInChildren<Light>(true);
			StoreLighmapControl lmControl = diskPrefab.GetComponentInChildren<StoreLighmapControl>(true);
			lmControl.lightStates.Clear();
			foreach (Light l in lights) {
				Light prefabLight = PrefabUtility.GetCorrespondingObjectFromSource(l);
				LightState state = null;
				foreach (LightState lightState in lmControl.lightStates) {
					if (lightState.light == l) {
						state = lightState;
						break;
					}
				}

				if (state == null) {
					state = new LightState() { light = prefabLight, activeState = l.gameObject.activeSelf, enabled = l.enabled };
					lmControl.lightStates.Add(state);
				}
			}

			/*SerializedObject s = new SerializedObject(lmControl);
			SerializedProperty prop = s.FindProperty(nameof(lightStates));
			s.ApplyModifiedPropertiesWithoutUndo();*/
			//prop.arraySize = lightStates.Count;

			EditorUtility.SetDirty(lmControl);
			string path = AssetDatabase.GetAssetPath(diskPrefab);
			/*PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction);*/
			Debug.Log(path);
			//PrefabUtility.ApplyObjectOverride(this,path,InteractionMode.AutomatedAction );
			AssetDatabase.SaveAssets();
		}
		/// <summary>
		/// Turn lghts off in prefab and on in scene
		/// </summary>
		[Button]
		public void ApplyLightStateToScene() {
			GameObject diskPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
			StoreLighmapControl diskComponent = PrefabUtility.GetCorrespondingObjectFromSource(this);
			PrefabAssetType type = PrefabUtility.GetPrefabAssetType(diskPrefab);
			if (type == PrefabAssetType.NotAPrefab) {
				Debug.LogWarning($"Need to use this button on a prefab in a scene", gameObject);
			}

			PrefabUtility.SavePrefabAsset(diskPrefab);			
			//Set light state 'in scene' 
			foreach (LightState lightState in diskComponent.lightStates) {
				lightState.SetSceneState();
			}	
			
			Light[] prefabLights = diskPrefab.GetComponentsInChildren<Light>(true);
			foreach (Light prefabLight in prefabLights) {
				prefabLight.gameObject.SetActive(false);
				EditorUtility.SetDirty(prefabLight);
			}

		}	

		/// <summary>
		/// Setup meshes in the scene one way, in the prefab another
		/// </summary>
		[Button]
		public void AddAndUpdateComponentsToLightmapped() {
			//AssetDatabase.StartAssetEditing();
			GameObject scenePrefab = gameObject;
			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
			MeshRenderer[] meshes = scenePrefab.GetComponentsInChildren<MeshRenderer>(true);
			int countL = 0;
			int countN = 0;
			//GameObject assetRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer.gameObject);
			//string path = AssetDatabase.GetAssetPath(prefab);

			foreach (MeshRenderer meshRenderer in meshes) {
				if (meshRenderer.receiveGI != ReceiveGI.Lightmaps) {
					continue;
				}
				if (meshRenderer.lightmapIndex >= 0 && meshRenderer.scaleInLightmap != 0) {
					//var control = meshRenderer.gameObject.AddComponentIfNeeded<StoreLightmapOffset>();
					StoreLightmapOffset control = meshRenderer.GetComponent<StoreLightmapOffset>();
					if (control == null) {
						control = meshRenderer.gameObject.AddComponent<StoreLightmapOffset>();
					}

					control.setPerObjectCoord = true;
					control.StoreCoordinate();
					countL++;
					
					if (assignLightmapToScript) {
						var sourceControl = PrefabUtility.GetCorrespondingObjectFromSource(control);
						if (meshRenderer.lightmapIndex < LightmapSettings.lightmaps.Length && meshRenderer.lightmapIndex >= 0) {
							// Debug.Log($" len = {LightmapSettings.lightmaps.Length} :  index {meshRenderer.lightmapIndex}");
							control.lightmap = LightmapSettings.lightmaps[meshRenderer.lightmapIndex]?.lightmapColor;
							var s = new SerializedObject(sourceControl);
							var prop = s.FindProperty(nameof(sourceControl.lightmap));
							prop.objectReferenceValue = LightmapSettings.lightmaps[meshRenderer.lightmapIndex]?.lightmapColor;
							s.ApplyModifiedPropertiesWithoutUndo();
							//PrefabUtility.ApplyObjectOverride(control,path,InteractionMode.AutomatedAction );
						}
					}
				} else {
					StoreLightmapOffset control = meshRenderer.GetComponent<StoreLightmapOffset>();
					if (control != null) {
						StoreLightmapOffset prefabComponent = PrefabUtility.GetCorrespondingObjectFromSource(control);
						GameObject obj = control.gameObject;
						DestroyImmediate(control); 
						PrefabUtility.ApplyRemovedComponent(obj,prefabComponent,InteractionMode.AutomatedAction);
					}

					countN++;
				}
			}

			PrefabUtility.SavePrefabAsset(prefab);
			Debug.Log($"Lightmapped = {countL} Not = {countN}");
			//AssetDatabase.StopAssetEditing();
		}

		[Button]
		public void ChangeShader() {
			var shader = Shader.Find("_MOR/Standard/Custom Lightmap");
			if (shader != null) {
				Debug.Log("FOUND!");
			}

			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);

			foreach (MeshRenderer meshRenderer in meshes) {
				bool notLightmapped = meshRenderer.lightmapIndex < 0 || meshRenderer.scaleInLightmap == 0;
				var c = meshRenderer.GetComponent<StoreLightmapOffset>();
				if (notLightmapped && c == null) {
					Debug.Log($"Skip {meshRenderer.name} - scale {meshRenderer.scaleInLightmap}");
					continue;
				}

				foreach (Material sharedMaterial in meshRenderer.sharedMaterials) {
					var mat = sharedMaterial;
					if (mat == null || mat.shader == null || string.IsNullOrEmpty(mat.shader.name)) {
						Debug.LogError($"Null shader problem on {meshRenderer.gameObject.name}", meshRenderer.gameObject);
						continue;
					}

					bool swappable = Array.IndexOf(swapableShaders, mat.shader.name) >= 0;
					if (mat == null || swappable == false) {
						if (mat != null) {
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
		}

		#endif

	}
	
}