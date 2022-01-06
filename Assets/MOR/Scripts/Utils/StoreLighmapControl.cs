using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Fbx;
using EasyButtons;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityEngine.Rendering;

namespace MOR.Museum {
	public class StoreLighmapControl : MonoBehaviour {
	    public bool assignLightmapToScript;
		public static string lightmapShader = "Assets\\Submodules\\Industries\\Art\\Shaders\\Standard_CustomLightmep.shader";
		private static string[] swapableShaders = {"Standard", "_MOR/Standard/Standard Stencil"}; //shaders we can swap to 'Standard customLightmap
		private static readonly int LightMap = Shader.PropertyToID("_LightMap");



		[Serializable]
		public class LightState {
			public Light light;
			public bool activeState;
			public bool enabled;

			public void SetSceneState() {
				light.enabled = enabled;
				light.gameObject.SetActive(activeState);
				EditorUtility.SetDirty(light);
			}
		}

		#if UNITY_EDITOR
		public GameObject matTest;
		public Material material;
		
		/// <summary>
		/// This is to extract materials being used in scene that exist only in the .fbx, as we can't change the shader unless it's a unity asset
		/// </summary>
		[Button]
		
		public void MatTest() {
			var meshes = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mesh in meshes) {
				string modelPath = AssetDatabase.GetAssetPath(mesh.sharedMaterial );
				if (modelPath.EndsWith(".fbx") == false) {
					continue;
				}
				Debug.Log(modelPath,mesh.gameObject);
			}

		}

		
		
		
		
		
		public List<LightState> lightStates = new List<LightState>();

		[Button]
		public void StoreLightStates() {
			var lights = GetComponentsInChildren<Light>(true);
			foreach (Light l in lights) {
				LightState state = null;
				foreach (LightState lightState in lightStates) {
					if (lightState.light == l) {
						state = lightState;
						break;
					}
				}
				if (state == null) {
					state = new LightState() { light = l, activeState = l.gameObject.activeSelf,enabled = l.enabled};
					lightStates.Add(state);
				}
				
			}
			var s = new SerializedObject(this);
			
			var prop = s.FindProperty(nameof(lightStates));
			//prop.arraySize = lightStates.Count;
			//s.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(this);
			GameObject assetRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
			
			string path = AssetDatabase.GetAssetPath(assetRoot);
			PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction);
			Debug.Log(path);
			PrefabUtility.ApplyObjectOverride(this,path,InteractionMode.AutomatedAction );
		}
		
		[Button]
		public void ApplyLightStateToScene() {
			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(this.gameObject);
			var type = PrefabUtility.GetPrefabAssetType(prefab);
			if (type == PrefabAssetType.NotAPrefab) {
				Debug.LogWarning($"Need to use this button on a prefab in a scene",gameObject);
			}
			//Set light state 'in scene' 
			foreach (LightState lightState in lightStates) {
				lightState.SetSceneState(); 
			}

			var prefabLights = prefab.GetComponentsInChildren<Light>(true);
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
			AssetDatabase.StartAssetEditing();
			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(this.gameObject);
			MeshRenderer[] meshes = prefab.GetComponentsInChildren<MeshRenderer>(true);
			int countL = 0;
			int countN = 0;
            //GameObject assetRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer.gameObject);
            string path = AssetDatabase.GetAssetPath(prefab);
		                   
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
		                   var s = new SerializedObject(control);
		                   var prop = s.FindProperty(nameof(control.lightmap));
		                   prop.objectReferenceValue =  LightmapSettings.lightmaps[meshRenderer.lightmapIndex]?.lightmapColor;
		                   s.ApplyModifiedPropertiesWithoutUndo();
		                   //PrefabUtility.ApplyObjectOverride(control,path,InteractionMode.AutomatedAction );
	                    }
                    }
				} else {
					StoreLightmapOffset control = meshRenderer.GetComponent<StoreLightmapOffset>();
					if (control != null) {
						Destroy(control); //TODO:?? how to apply this change to prefab ?? 
						
					}

					countN++;
				}
			}

			Debug.Log($"Lightmapped = {countL} Not = {countN}");
			AssetDatabase.StopAssetEditing();
		}
		[Button]
		public void AssignReflectionProbes() {
			var type = PrefabUtility.GetPrefabAssetType(gameObject);
			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource<GameObject>(this.gameObject);
			if (prefab == null) {
				Debug.Log($"prefab null : {type}");
				prefab = this.gameObject;
				//return;
			}

			var lightProbes = prefab.GetComponentsInChildren<LightProbeGroup>();
			bool usingLightProbes = lightProbes.Length > 0;
			AssetDatabase.StartAssetEditing();
			MeshRenderer[] meshes = prefab.GetComponentsInChildren<MeshRenderer>(true);
			int i = 0;
			foreach (MeshRenderer meshRenderer in meshes) {
				SerializedObject s = new SerializedObject(meshRenderer);
				SerializedProperty prop = s.FindProperty("m_LightProbeUsage");
				prop.intValue = (int)LightProbeUsage.Off;
				meshRenderer.lightProbeUsage =  LightProbeUsage.Off;
				
				if (meshRenderer.reflectionProbeUsage == ReflectionProbeUsage.Off) {
					continue;
				}

				ReflectionProbe probe = GetStrongestProbe(meshRenderer);
				if (probe == null) {
					Debug.Log($"Probe null {i++}",meshRenderer.gameObject);
					continue;
				}

				prop = s.FindProperty("m_ReflectionProbeUsage");
				if (prop == null) {
					continue;
				}
				prop.intValue = (int)ReflectionProbeUsage.Simple;
				meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Simple;				
				
				prop = s.FindProperty("m_ProbeAnchor");
				if (prop == null) {
					continue;
				}
				//string path = AssetDatabase.GetAssetPath(this);
				//GameObject assetRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer.gameObject);
				prop.objectReferenceValue = probe.transform;
				meshRenderer.probeAnchor = probe.transform;
				Debug.Log($"{i++} - {meshRenderer.name}");
				Debug.Log(meshRenderer.probeAnchor);
				//PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction );
				EditorUtility.SetDirty(meshRenderer);
				s.ApplyModifiedPropertiesWithoutUndo();

				//PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction );
				//PrefabUtility.ApplyObjectOverride(meshRenderer,path,InteractionMode.AutomatedAction);
			}

			if (type != PrefabAssetType.NotAPrefab) {
				PrefabUtility.SavePrefabAsset(prefab);
			}

			AssetDatabase.StopAssetEditing();
			Selection.activeObject = prefab;
		}

		/*
		[Button]
		void ResumeAssetImport() {
			AssetDatabase.StopAssetEditing();
		}*/
		public ReflectionProbe GetStrongestProbe(Renderer mRenderer) {
			ReflectionProbe probe = null;
			List<ReflectionProbeBlendInfo> probes = new List<ReflectionProbeBlendInfo>();
			mRenderer.GetClosestReflectionProbes(probes);
			float biggestWeight = -1;
			foreach (ReflectionProbeBlendInfo probeInfo in probes) {
				if (probeInfo.probe.transform.root != mRenderer.transform.root) {
					//Debug.Log($"probe not on same root : {probeInfo.probe.transform.root}  : {mRenderer.transform.root}");
					continue;
				}
				if (probeInfo.weight > biggestWeight) {
					probe = probeInfo.probe;
					biggestWeight = probeInfo.weight;
				}
			}
			return probe;
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
                		Debug.LogError($"Null shader problem on {meshRenderer.gameObject.name}",meshRenderer.gameObject);
                		continue;
                	}
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
		}
		protected T[] GetMeshPropertyArray<T>(T[] sourceArray, int firstIndex, int numVerts) {
			T[] outArray = new T[numVerts];
			for (int j = 0; j < numVerts; j++) {
				int sourceIndex = j + firstIndex;
				outArray[j] = sourceArray[sourceIndex];
			}
			return outArray;
		}
		
		[Button]
		public void SplitMeshes() {
			
			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);
			List<Mesh> meshSources = new List<Mesh>();
			List<string> splitFileSources = new List<string>();
			foreach (MeshRenderer meshRenderer in meshes) {

				if (meshRenderer.sharedMaterials.Length < 2) {
					continue;
				}
				MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				Mesh sourceMesh = meshFilter.sharedMesh;
				string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
				Debug.Log(sourcePath,meshRenderer.gameObject);
				
				//copy out of hierarchy for target duplication
				GameObject sourceObject = Instantiate(meshRenderer.gameObject,null);
				Transform sourceObjectTransform = sourceObject.transform;
				if (sourceObjectTransform.childCount > 0) { //Clear Children.
					for (int i = sourceObjectTransform.childCount - 1; i >= 0; i--) {
						Transform child = sourceObjectTransform.GetChild(i);
						DestroyImmediate(child.gameObject);
					}
				}				
				//info for Colliders
				MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
				bool hasMeshCollider = meshCollider != null && meshCollider.sharedMesh == sourceMesh;
				Collider otherCollider = hasMeshCollider ? null : meshRenderer.GetComponent<Collider>();
				bool hasOtherCollider = otherCollider != null;
				
				int subMeshCount = sourceMesh.subMeshCount;
				GameObject[] newObjects = new GameObject[subMeshCount];
				MeshFilter[] newFilters = new MeshFilter[subMeshCount];
				
				
				//Create Destination Objects
				for (int i = 0; i < subMeshCount; i++) {
					GameObject newObject = Instantiate<GameObject>(sourceObject,meshRenderer.transform); //Need to make sure no children copied so don't parent yet
                	MeshRenderer newRenderer = newObject.GetComponent<MeshRenderer>();
                	newFilters[i] = newObject.GetComponent<MeshFilter>();
                	newRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[i] };
                	newObjects[i] = newObject; //add to out array.
                }
				
				
				int indexInList = meshSources.IndexOf( sourceMesh);
                string resultPath;
				if (indexInList >= 0) { //We've already exported this before.
					resultPath = splitFileSources[indexInList];
				} else { //Make new meshes, and export to fbx.
					Mesh[] newMeshes = new Mesh[subMeshCount];
					
					Vector3[] sourceVerts = sourceMesh.vertices;
					Vector2[] sourceUvs0 = sourceMesh.uv;
					int[] sourceFaces = sourceMesh.triangles;


					Vector3[] sourceNormals = null;
					bool hasNormals = sourceMesh.HasVertexAttribute(VertexAttribute.Normal);
					if (hasNormals) {
						sourceNormals = sourceMesh.normals;
					}

					Color[] sourceColours = null;
					bool hasColor = sourceMesh.HasVertexAttribute(VertexAttribute.Color);
					if (hasColor) {
						sourceColours = sourceMesh.colors;
					}

					Vector2[] sourceUvs1 = null;
					bool hasUv1 = sourceMesh.HasVertexAttribute(VertexAttribute.TexCoord1);
					if (hasUv1) {
						sourceUvs1 = sourceMesh.uv2;
					}

					for (int i = 0; i < subMeshCount; i++) {
						SubMeshDescriptor subMesh = sourceMesh.GetSubMesh(i);
						int firstIndex = subMesh.firstVertex;
						int numVerts = subMesh.vertexCount;
						int index0 = subMesh.indexStart;
						int indexCount = subMesh.indexCount;


						MeshFilter newFilter = newFilters[i];
						//Make new mesh based off of old.
						Mesh newMesh = new Mesh();
						newMeshes[i] = newMesh;
						newMesh.name = $"{meshRenderer.name}{i:00}";


						newMesh.vertices = GetMeshPropertyArray(sourceVerts, firstIndex, numVerts);
						newMesh.uv = GetMeshPropertyArray(sourceUvs0, firstIndex, numVerts);
						
						if (hasNormals) {
							newMesh.normals = GetMeshPropertyArray(sourceNormals, firstIndex, numVerts);
						}

						if (hasColor) {
							newMesh.colors = GetMeshPropertyArray(sourceColours, firstIndex, numVerts);
						}

						if (hasUv1) {
							newMesh.uv2 = GetMeshPropertyArray(sourceUvs1, firstIndex, numVerts);
						}

						int[] faces = new int[indexCount];
						for (int j = 0; j < indexCount; j++) {
							faces[j] = sourceFaces[j + index0] - firstIndex;
						}

						newMesh.triangles = faces;
						newFilter.sharedMesh = newMesh; //assign to new object
					}
					
					string sourceFilename = Path.GetFileNameWithoutExtension(sourcePath);
                    string outPath = Path.Combine(Path.GetDirectoryName(sourcePath), $"{sourceFilename}_split.fbx");
                    if (File.Exists(outPath)) {
                    	Debug.LogWarning("Overwriting file : This may break previously assigned objects using these meshes");
                    }
					resultPath = ModelExporter.ExportObjects(outPath, newObjects);
					
					meshSources.Add(sourceMesh);
                    splitFileSources.Add(resultPath);
                    
                    //Debug.Log($"New Path = {resultPath}",meshRenderer.gameObject);
					foreach (Mesh newMesh in newMeshes) {
						DestroyImmediate(newMesh);
					}                    
				}

				//Import new model...
				GameObject newFBX = AssetDatabase.LoadAssetAtPath<GameObject>(resultPath);
				
				//Add meshes out of new model into the newObjects array mesh filters
				MeshFilter[] importMeshes = newFBX.GetComponentsInChildren <MeshFilter>(true);
				for (int i = 0; i < subMeshCount; i++) {
					GameObject newObject = newObjects[i];
					
					newObject.transform.SetParent(meshRenderer.transform,false);
					newObject.transform.localPosition = Vector3.zero;
					newObject.transform.localRotation = Quaternion.identity;
					newObject.transform.localScale = Vector3.one;
					if (i >= importMeshes.Length) {
						Debug.LogError($"Wrong number of meshes {importMeshes.Length} - index = {i}",meshRenderer.gameObject);
					} else {
						Mesh newMesh = importMeshes[i].sharedMesh;
	                    if (hasMeshCollider) {
                    		MeshCollider newMeshCollider = newObject.GetComponent<MeshCollider>();
                    		newMeshCollider.sharedMesh = newMesh;
	                    }
						newFilters[i].sharedMesh = newMesh;						
					}

					if (hasOtherCollider) {
						Collider newObjectCollider = newObject.GetComponent<Collider>();
						DestroyImmediate(newObjectCollider);
					}
				}
				//Clean up the meshes we just exported and no longer need, as we're getting them from file now.

				DestroyImmediate(meshFilter);
				StoreLightmapOffset lmOffset = meshRenderer.GetComponent<StoreLightmapOffset>();
				DestroyImmediate(meshRenderer);
				if (hasMeshCollider) {
					DestroyImmediate(meshCollider);
				}
				if (lmOffset != null) {
					DestroyImmediate(lmOffset);
				}
				DestroyImmediate(sourceObject);//kill temp object used for copying
			}
			
		}
	#endif
	}
}