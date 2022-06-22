using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MOR.Industries;
using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MOR.Museum {
	public class ArtStagingSettings : ScriptableObject {
		private static readonly Color ColorUnset = new Color(0.64f, 0.64f, 0.64f, 1f);

		public enum ValidationState {
			NotValidated,
			Valid,
			CriticalFail,
			HasWarnings
		};

		[ReadOnly] public ValidationState sceneValidationState;
		[ReadOnly] public ValidationState modelsValidationState;
		[ReadOnly] public ValidationState textureValidationState;
		[ReadOnly] public ValidationState materialValidationState;
		[ReadOnly] public ValidationState lightProbeValidationState;

		public bool isThroughPortal;
		public GameObject rootPrefabSource;
		public GameObject MORPlaceablePrefab;
		[ReadOnly] public bool playableDirectorOK = true;
		[ReadOnly] public ValidationState colliderValidationState;
		public ValidationState skyboxValidationState;
		public bool inEditorMORLightmapApply = false; 
		public void Save() {
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
		}

		public bool ThroughPortal {
			set {
				if (isThroughPortal != value) {
					isThroughPortal = value;
					Save();
				}
			}
		}


		public Color GetSceneValidState() {
			return GetValidationColor(sceneValidationState);
		}

		public Color GetModelValidState() {
			return GetValidationColor(modelsValidationState);
		}

		public Color GetTextureValidState() {
			return GetValidationColor(textureValidationState);
		}

		public Color GetMaterialValidState() {
			return GetValidationColor(materialValidationState);
		}

		public Color GetLightProbeValidState() {
			return GetValidationColor(lightProbeValidationState);
		}

		public Color GetColliderValidState() {
			return GetValidationColor(colliderValidationState);
		}

		public Color GetSkyboxValidState() {
			return GetValidationColor(skyboxValidationState);
		}


		public Color GetValidationColor(ValidationState state) {
			return state switch {
				ValidationState.NotValidated => ColorUnset,
				ValidationState.Valid => Color.green,
				ValidationState.CriticalFail => Color.red,
				ValidationState.HasWarnings => Color.yellow,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}

	[InitializeOnLoad]
	public class ArtStagingEditor : EditorWindow {
		private const int INDICATOR_WIDTH = 10;
		protected const float BOUNDS_EXTRA_FOR_SKYBOX = 20f;
		protected const string SKYBOX_MODEL_PATH_MOR = "Assets/MOR/Art/Models/SkyboxSphereProceduralShaders.fbx/Sphere";
		protected const string SKYBOX_MODEL_PATH_STAGING = "Assets/Art/SkyboxSphereProceduralShaders.fbx/Sphere";
		
		protected static string[] morRootGameObjects = { "MOR", "Markers", "FakeSkyboxCeilingExample", "WallpaperIdNeeded", "MORPlayer" };
		private static readonly string[] NonFbxFiletypes = { ".blend", ".mb", ".ma", ".max", ".c4d", ".lxo" };
		private static int VERT_COUNT_MAX_PC = 1_000_000;


		public ArtStagingSettings settings;

		protected List<GameObject> nonPrefabbedObjects = new List<GameObject>();

		private static bool needsMeshSplit = false;
		private static bool badFileFormats = false;
		private static bool largeFileSize = false;

		private bool initialized = false;
		private bool componentsOnRoot = false;
		private string warningMessage = "";
		private string errorMessage = "";

		[MenuItem("Tools/MOR ArtStaging")]
		public static void Init() {
			if (Application.isPlaying) {
				return;
			}
			ArtStagingEditor window = GetWindow<ArtStagingEditor>("MOR Art Staging", true);
			window.minSize = window.minSize + new Vector2(0, 30); //MAke taller by default?
			window.Show();
			ArtStagingSettings artStagingSettings = AssetDatabase.LoadAssetAtPath<ArtStagingSettings>("Assets/MOR/ArtStagingSettings.asset");

			if (artStagingSettings == null) {
				artStagingSettings = CreateInstance<ArtStagingSettings>();
				if (Directory.Exists("Assets/MOR") == false) {
					Directory.CreateDirectory("Assets/MOR");
				}
				try {
					AssetDatabase.CreateAsset(artStagingSettings, "Assets/MOR/ArtStagingSettings.asset");
				}
				catch (Exception e){
					Debug.LogError(e.Message);
				}
			}
			window.settings = artStagingSettings;
		}


		protected void OnGUI(){
			if (initialized == false) {
				RootHasMORComponents();
				initialized = true;
			}

			//if (GUILayout.Button("Text")) {
			//	Test( FindSceneInstance(settings.rootPrefabSource));
			//}
			GUILayout.Space(10);
			Rect topRect = GUILayoutUtility.GetRect(20, 18);
			GUI.enabled = false;
			EditorGUI.ObjectField(topRect, settings, typeof(ArtStagingSettings), false);
			GUI.enabled = true;
			GUILayout.Space(10);
			Rect boxRect = GUILayoutUtility.GetRect(20, 18);
			//Draw top boxes for all checks
			boxRect.x += 10;
			boxRect.width = INDICATOR_WIDTH;
			EditorGUI.DrawRect(boxRect, settings.GetSceneValidState());
			boxRect.x += 12;

			EditorGUI.DrawRect(boxRect, settings.GetModelValidState());
			boxRect.x += 12;
			if (settings.isThroughPortal) {
				EditorGUI.DrawRect(boxRect, settings.GetColliderValidState());
				boxRect.x += 12;
			}


			EditorGUI.DrawRect(boxRect, settings.GetTextureValidState());
			boxRect.x += 12;

			EditorGUI.DrawRect(boxRect, settings.GetMaterialValidState());
			boxRect.x += 12;

			EditorGUI.DrawRect(boxRect, settings.GetLightProbeValidState());
			
			boxRect.x = 320;
			boxRect.height = 20;
			boxRect.width = 170;
			if (GUI.Button(boxRect, "Reset Checks")) {
				settings.sceneValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.modelsValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.textureValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.materialValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.lightProbeValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.colliderValidationState = ArtStagingSettings.ValidationState.NotValidated;
				settings.skyboxValidationState = ArtStagingSettings.ValidationState.HasWarnings;
			}





			GUILayout.Space(10);

			
			
			boxRect = GUILayoutUtility.GetRect(20, 18);
			GUI.Label(boxRect, "Root Prefab");
			settings.rootPrefabSource = (GameObject)EditorGUILayout.ObjectField(settings.rootPrefabSource, typeof(GameObject), false);
			if (settings.rootPrefabSource == null) {
				if (GUILayout.Button("Get Root Prefab")) {
					settings.rootPrefabSource = GetRootPrefab();
					settings.Save();
				}

				if (settings.rootPrefabSource == null) {
					return;
				}
			}
			else {
				if (componentsOnRoot == false) {
					if (GUILayout.Button("Add MOR components to Root")) {
						AddMORComponentsToRootPrefab(settings.rootPrefabSource);
						componentsOnRoot = true;
					}
				}
			}

			// We likely don't want this for 'Metaverse' type version of this tool, as all will be in their pocket universes or however we end up setting things up.
			settings.ThroughPortal = !GUILayout.Toggle(!settings.isThroughPortal, "Is Standalone Artwork");

			GUILayout.BeginHorizontal();
			{
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 10;
				boxRect.width = INDICATOR_WIDTH;
				EditorGUI.DrawRect(boxRect, settings.GetSceneValidState());
				boxRect.x += 20;
				boxRect.width = 180;
				if (GUI.Button(boxRect, "Check Prefab")) {
					(bool hasErrors, bool hasWarnings) = ValidateProject();
					settings.sceneValidationState =
						hasErrors ? ArtStagingSettings.ValidationState.CriticalFail : (hasWarnings ? ArtStagingSettings.ValidationState.HasWarnings : ArtStagingSettings.ValidationState.Valid);
					settings.playableDirectorOK = this.CheckPlayableDirector();

					settings.Save();
				}

				boxRect.x += 20;
				boxRect.width = 180;
				if (settings.playableDirectorOK == false) {
					boxRect = GUILayoutUtility.GetRect(20, 18);

					if (GUI.Button(boxRect, "Fix Director")) {
						this.FixPlayableDirector();
						settings.playableDirectorOK = true;
						settings.Save();
					}
				}
			}
			GUILayout.EndHorizontal();

			//MeshCheck
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			{
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 10;
				boxRect.width = INDICATOR_WIDTH;
				EditorGUI.DrawRect(boxRect, settings.GetModelValidState());

				boxRect.x += 20;
				boxRect.width = 180;
				if (GUI.Button(boxRect, "Validate Meshes")) {
					errorMessage = "";
					warningMessage = "";
					DoMeshCheck();
				}
			}
			GUILayout.EndHorizontal();


			if (settings.modelsValidationState == ArtStagingSettings.ValidationState.CriticalFail && needsMeshSplit) {
				GUILayout.Space(10);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 50;
				boxRect.width = 200;
				if (GUI.Button(boxRect, "Export/Split Meshes to FBX")) {
					needsMeshSplit = false;
					var sceneRoot = FindSceneInstance(settings.rootPrefabSource);
					Debug.Log($"Scene object : {sceneRoot}", sceneRoot);
					SplitMeshes(sceneRoot);
					errorMessage = "";
					warningMessage = "";
					DoMeshCheck();
				}
			}

			if (badFileFormats) {
				GUILayout.Space(10);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 10;
				boxRect.width = INDICATOR_WIDTH;
				EditorGUI.DrawRect(boxRect, settings.GetColliderValidState());

				boxRect.x += 20;
				boxRect.width = 180;
				if (GUI.Button(boxRect, "Convert Meshes files to FBX")) {
					badFileFormats = false;
					ConvertLooseMeshesToFBX(settings.rootPrefabSource);
					errorMessage = "";
					warningMessage = "";
					DoMeshCheck();
				}
			}

			if (settings.colliderValidationState != ArtStagingSettings.ValidationState.Valid && (settings.modelsValidationState == ArtStagingSettings.ValidationState.Valid ||
			                                                                                     settings.modelsValidationState == ArtStagingSettings.ValidationState.HasWarnings)) {
				GUILayout.Space(10);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 50;
				boxRect.width = 200;
				if (GUI.Button(boxRect, "Check Collision")) {
					bool checkResult = CheckColliders();
					if (checkResult == false) {
						settings.colliderValidationState = ArtStagingSettings.ValidationState.CriticalFail;
					}
					else {
						settings.colliderValidationState = ArtStagingSettings.ValidationState.Valid;
						errorMessage = "";
					}

					settings.Save();
				}
			}

			if (settings.colliderValidationState == ArtStagingSettings.ValidationState.Valid && settings.skyboxValidationState != ArtStagingSettings.ValidationState.Valid) {
				GUILayout.Space(10);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 50;
				boxRect.width = 200;
				if (GUI.Button(boxRect, "Check Skybox")) {
					bool checkResult = CheckSkybox();
					if (checkResult == false) {
						settings.skyboxValidationState = ArtStagingSettings.ValidationState.CriticalFail;
						warningMessage = "Create a custom skybox local to prefab. Scene Skybox will not transfer to MOR.";
					}
					else {
						settings.skyboxValidationState = ArtStagingSettings.ValidationState.Valid;
						warningMessage = "";
					}

					settings.Save();
				}
			

				if (settings.skyboxValidationState == ArtStagingSettings.ValidationState.NotValidated || settings.skyboxValidationState == ArtStagingSettings.ValidationState.HasWarnings ||
				    settings.skyboxValidationState == ArtStagingSettings.ValidationState.CriticalFail) {
					GUILayout.Space(10);

					boxRect = GUILayoutUtility.GetRect(20, 18);
					boxRect.x += 50;
					boxRect.width = 200;
					if (GUI.Button(boxRect, "Dismiss Skybox check")) {
						settings.skyboxValidationState = ArtStagingSettings.ValidationState.Valid;
					}

					if (settings.skyboxValidationState == ArtStagingSettings.ValidationState.CriticalFail) {
						GUILayout.Space(10);
						boxRect = GUILayoutUtility.GetRect(20, 18);
						boxRect.x += 50;
						boxRect.width = 200;
						if (GUI.Button(boxRect, "Create Local skybox")) {
							CreateSkybox(FindSceneInstance(settings.rootPrefabSource));
							settings.skyboxValidationState = ArtStagingSettings.ValidationState.Valid;
							warningMessage = null;
						}
					}
					else if (settings.skyboxValidationState == ArtStagingSettings.ValidationState.HasWarnings) {
						GUILayout.Space(10);
						boxRect = GUILayoutUtility.GetRect(20, 18);
						boxRect.x += 50;
						boxRect.width = 200;
						if (GUI.Button(boxRect, "ConvertSkybox")) {
							this.ChangeSkyboxShaderToMORLightmap();
						}
					}
				}
			}



			//Texture Check.
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			{
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 10;
				boxRect.width = INDICATOR_WIDTH;
				EditorGUI.DrawRect(boxRect, settings.GetTextureValidState());

				boxRect.x += 20;
				boxRect.width = 180;
				if (GUI.Button(boxRect, "Check Textures")) {
					textureSize = -1;
					CheckTextures(settings.rootPrefabSource);
				}

				if (settings.textureValidationState == ArtStagingSettings.ValidationState.CriticalFail && largeFileSize) {
					GUI.Label(boxRect, "Files must be under 100MB. Check console.");
				}
			}
			GUILayout.EndHorizontal();


			if (settings.textureValidationState != ArtStagingSettings.ValidationState.CriticalFail &&
			    settings.textureValidationState != ArtStagingSettings.ValidationState.NotValidated &&
			    settings.modelsValidationState != ArtStagingSettings.ValidationState.CriticalFail &&
			    settings.modelsValidationState != ArtStagingSettings.ValidationState.NotValidated &&
			    settings.sceneValidationState != ArtStagingSettings.ValidationState.NotValidated &&
			    settings.sceneValidationState != ArtStagingSettings.ValidationState.CriticalFail
			   ) {
				//Material Check
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				{
					boxRect = GUILayoutUtility.GetRect(20, 18);
					boxRect.x += 10;
					boxRect.width = INDICATOR_WIDTH;
					EditorGUI.DrawRect(boxRect, settings.GetMaterialValidState());

					boxRect.x += 20;
					boxRect.width = 180;
					if (GUI.Button(boxRect, "Check Materials")) {
						CheckMaterials();
					}
				}
				GUILayout.EndHorizontal();

				if (settings.materialValidationState == ArtStagingSettings.ValidationState.CriticalFail) {
					GUILayout.Space(10);
					boxRect = GUILayoutUtility.GetRect(20, 18);
					boxRect.x += 50;
					boxRect.width = 200;
					if (GUI.Button(boxRect, "Extract Materials from Models")) {
						ExtractEmbeddedMaterials(settings.rootPrefabSource);
						CheckMaterials();
					}
				}
				/*if (settings.textureValidationState == ArtStagingSettings.ValidationState.CriticalFail && largeFileSize) {
					GUI.Label(boxRect, "Files must be under 100MB. Check console.");
				}*/
			}

			if (settings.materialValidationState == ArtStagingSettings.ValidationState.Valid || settings.materialValidationState == ArtStagingSettings.ValidationState.HasWarnings) {
				//LightProbe Check.
				GUILayout.Space(5);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				GUI.Label(boxRect, "Reflection Probes");
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				{
					boxRect = GUILayoutUtility.GetRect(20, 18);
					boxRect.x += 10;
					boxRect.width = INDICATOR_WIDTH;
					EditorGUI.DrawRect(boxRect, settings.GetLightProbeValidState());

					boxRect.x += 20;
					boxRect.width = 180;
					if (GUI.Button(boxRect, "Use Scene Reflections")) {
						settings.lightProbeValidationState = ArtStagingSettings.ValidationState.HasWarnings;
						settings.Save();
					}

					boxRect.x += 190;
					boxRect.width = 180;
					if (GUI.Button(boxRect, "Assign Custom Probes")) {
						AssignReflectionProbes(settings.rootPrefabSource);
						settings.lightProbeValidationState = ArtStagingSettings.ValidationState.Valid;
						settings.Save();
					}
				}
				GUILayout.EndHorizontal();

				if (settings.lightProbeValidationState == ArtStagingSettings.ValidationState.Valid) {
					
					{
						GUILayout.Space(10);
						boxRect = GUILayoutUtility.GetRect(20, 18);
						
						boxRect.x += 20;
						boxRect.width = 220;
						if (GUI.Button(boxRect, "Set Reflection Probes to Custom")) {
							ChangeProbesToCustom();
							//settings.lightProbeValidationState = ArtStagingSettings.ValidationState.HasWarnings;
						}
					}
					
				}
			}

			GUILayout.Space(20);
			//Warning and Error boxes
			if (string.IsNullOrEmpty(errorMessage) == false) {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
			}

			if (string.IsNullOrEmpty(warningMessage) == false) {
				EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
			}

			if (settings.sceneValidationState == ArtStagingSettings.ValidationState.Valid) {
				//Setup an area for setting up localized lightmap settings, analogous to StoreLighmapControl(sic)
				GUILayout.Space(20);
				boxRect = GUILayoutUtility.GetRect(20, 8);
				boxRect.height = 1;
				EditorGUI.DrawRect(boxRect, Color.black);

				GUILayout.Space(8);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 20;
				boxRect.width = 270;
				GUI.Label(boxRect, "Localize Lightmaps");

				GUILayout.Space(20);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 40;
				boxRect.width = 270;
				if (GUI.Button(boxRect, "Store Lightmap Settings on Objects")) {
					StoreLightmapApply();
				}

				GUILayout.Space(12);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 40;
				boxRect.width = 270;
				if (GUI.Button(boxRect, "Set shaders to MOR Lightmap")) {
					SetShadersToMORLightmap();
				}
				
				
				EditorGUILayout.HelpBox("MOR Lightmap material is Standard analogous, but works in conjunction with 'StoreLightmapOffset' script to " +
				                        "apply lightmaps on a placeable prefab. Offsets are applied at runtime, so editor window display may appear incorrect.", MessageType.Info);
				GUILayout.BeginHorizontal();
				settings.inEditorMORLightmapApply = GUILayout.Toggle(settings.inEditorMORLightmapApply, "Apply MOR Lightmaps outside of play mode");
				if (settings.inEditorMORLightmapApply) {
					
					bool buttonPressed = GUILayout.Button("Update MOR Lightmap Display");
					if (this.inEditorLightmapApply == false || buttonPressed) {
						inEditorLightmapApply = true; //Store that we've done this on a local variable that likely gets reset on load etc.
						ApplyMORLightmap();
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(12);
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 40;
				boxRect.width = 270;
				if (GUI.Button(boxRect, "Make MOR Placeable Prefab Variant")) {
					MakeMORPlaceablePrefabVariant();
				}

				EditorGUILayout.HelpBox("This will make a variant prefab which will have dynamic shadowcasting disabled as an override, as that is in the lightmap " +
				                        "and will handle disabling lights etc.", MessageType.None);
				
				
				/*boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 40;
				boxRect.width = 270;
				if (GUI.Button(boxRect, "CalculateUsedTexturesSize")) {
					
				}
				boxRect = GUILayoutUtility.GetRect(20, 18);
				boxRect.x += 40;
				boxRect.width = 270;
				if (GUI.Button(boxRect, "CalculateUsedModelSize")) {
					
				}
				*/
				if (textureSize == -1) {
					CalculateUsedTexturesSize();
					CalculateUsedModelSize();
				}

				GUILayout.BeginHorizontal();
				boxRect = GUILayoutUtility.GetRect(INDICATOR_WIDTH, 18);
				boxRect.x += 10;
				boxRect.width = INDICATOR_WIDTH;
				float texInMB = (float)textureSize / BYTES_TO_MEGABYTES2;
				EditorGUI.DrawRect(boxRect, GetColorForTextureFileSizeLimits((int)texInMB));
				boxRect.width = 350;
				boxRect.x += 10;
				GUI.Label( boxRect,$"  Approximate Texture usage : {texInMB : 0.00} MB in {textureCount} textures");
				GUILayout.EndHorizontal();		
				
				//GUILayout.BeginHorizontal();
				boxRect = GUILayoutUtility.GetRect(INDICATOR_WIDTH, 9);
				boxRect.x += 30;
				boxRect.width = INDICATOR_WIDTH;
				DrawBoxTextureMeter((int)texInMB,boxRect);
				//GUILayout.EndHorizontal();
				
				CalculateUsedAudioSize();
				GUILayout.Label( $"\tLightmaps - {lightmapCount}");
				GUILayout.Label($"\tLargest Texture Dimension {largestDimension}\n");
				GUILayout.Label( $"  Approximate Model usage : {((float)modelSize / BYTES_TO_MEGABYTES2) : 0.00} MB");
				GUILayout.Label( $"  Approximate Audio usage (compressed): {((float)audioSize / BYTES_TO_MEGABYTES) : 0.00} MB");
				GUILayout.Label( $"                            (original): {((float)audioSizeRaw / BYTES_TO_MEGABYTES) : 0.00} MB");
			}
			//AddMORComponentsToRootPrefab(settings.rootPrefabSource);
		}

		private void ApplyMORLightmap(){
			GameObject diskPrefab = settings.rootPrefabSource;
			GameObject scenePrefab = FindSceneInstance(diskPrefab);
			var lightmapOffsets = scenePrefab.GetComponentsInChildren<StoreLightmapOffset>();
			foreach (var offseter in lightmapOffsets) {
				offseter.ApplyMaterialPropertyBlockSettings();
			}
		}
		
		
		
		Color orange = new Color(1f,0.5f,0f);
		private bool inEditorLightmapApply = false;
		private void DrawBoxTextureMeter(int texInMB,Rect boxRect){
			
			boxRect.width = INDICATOR_WIDTH * 2;
			int offset = INDICATOR_WIDTH * 2;
			int halfOffset = INDICATOR_WIDTH;
			float multiplier = 1;
			float offMult = 0.5f;			
			
			//This makes the first 100MB tripple size so it's more 'Logarithmic' in display to encourace less texture useage
			
			EditorGUI.DrawRect(boxRect, Color.green);
			multiplier = texInMB < 33 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.green*multiplier);
			multiplier = texInMB < 65 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.green*multiplier);
			multiplier = texInMB < 90 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.green*multiplier);
			
			
			// Linear from here on
			multiplier = texInMB < 106 ? offMult : 1f;
			boxRect.width = INDICATOR_WIDTH+1;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.cyan * multiplier);
			boxRect.width = INDICATOR_WIDTH;
			multiplier = texInMB < 155 ? offMult : 1f;
			boxRect.x += halfOffset;			EditorGUI.DrawRect(boxRect, Color.yellow * multiplier);
			
			boxRect.width = INDICATOR_WIDTH * 2;
			multiplier = texInMB < 200 ? offMult : 1f;
			boxRect.x += halfOffset;			EditorGUI.DrawRect(boxRect, Color.yellow * multiplier);
			multiplier = texInMB < 300 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, orange * multiplier);
			multiplier = texInMB < 400 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.red * multiplier);
			multiplier = texInMB < 500 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.red * multiplier);
			multiplier = texInMB < 600 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.red * multiplier);
			multiplier = texInMB < 700 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.red * multiplier);
			multiplier = texInMB < 800 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.red * multiplier);
			multiplier = texInMB < 900 ? offMult : 1f;
			boxRect.x += offset;			EditorGUI.DrawRect(boxRect, Color.magenta * multiplier);

		}
		private Color GetColorForTextureFileSizeLimits(int texInMB){
			Color textureSizeIndicatorColor = Color.green;
			if (texInMB > 1000) {
				textureSizeIndicatorColor = Color.magenta;
			}else if (texInMB > 400) {
				textureSizeIndicatorColor = Color.red;
			}else if (texInMB > 300) {
				textureSizeIndicatorColor = orange;
			}
			else if (texInMB > 155) {
				textureSizeIndicatorColor = Color.yellow;
			}else if (texInMB > 106) {
				textureSizeIndicatorColor = Color.cyan;
			}
			return textureSizeIndicatorColor;
		}
		private int textureSize = -1;
		private int textureCount = 0;
		private int modelSize = -1;
		private int lightmapCount = -1;
		private const int BYTES_TO_MEGABYTES =1048576;
		private const int BYTES_TO_MEGABYTES2 = 2 * BYTES_TO_MEGABYTES;
		private Vector2 largestDimension;
		public void CalculateUsedTexturesSize(){
			GameObject diskPrefab = settings.rootPrefabSource;
			var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(diskPrefab));
			int fileSize = 0;
			
			var texturesBySize = new Dictionary<Vector2Int, int>();
			lightmapCount = 0;
			largestDimension = Vector2.zero;
			textureCount = 0;
			
			foreach (var dependency in dependencies) {
				if (AssetDatabase.GetMainAssetTypeAtPath(dependency) == typeof(Texture2D)) {
					var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dependency);
					if (Path.GetFileName(dependency).StartsWith("Lightmap")) {
						lightmapCount += 1;
					}
					else {
						int thisSize = tex.width * tex.height;
						int biggestYet =(int) (largestDimension.x * largestDimension.y);
						if (thisSize > biggestYet) {
							largestDimension = new Vector2(tex.width,tex.height);
						}

						var key = new Vector2Int(tex.width, tex.height);
						if (texturesBySize.ContainsKey(key) == false) {
							texturesBySize[key] = 1;
						}
						else {
							texturesBySize[key] += 1;
						}
					}

					textureCount += 1;
					var texMemSize = Profiler.GetRuntimeMemorySizeLong(tex);
					//Debug.Log($"{(texMemSize/(2*1048576f)) : 0.00}MB - {dependency}");
					fileSize += (int)texMemSize;
				}
			}
			
			textureSize = fileSize;
			foreach (var key in texturesBySize.Keys) {
				Debug.Log($"Textures {key} - {texturesBySize[key]}");
				if (key != largestDimension) {
					continue;
				}
				foreach (var dependency in dependencies) {
					if (AssetDatabase.GetMainAssetTypeAtPath(dependency) == typeof(Texture2D)) {
						if (Path.GetFileName(dependency).StartsWith("Lightmap") == false) {
							var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dependency);
							var matchKey = new Vector2(tex.width, tex.height);
							if (matchKey == key) {
								Debug.Log($"	tex - {dependency}", tex);
							}
						}
					}
				}
			}
			//Debug.Log($"Total Texture Size: {(fileSize/(2*1048576f)) : 0.00}MB");
		}
		
		public void CalculateUsedModelSize(){
			GameObject diskPrefab = settings.rootPrefabSource;
			var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(diskPrefab));
			int fileSize = 0;
			foreach (var dependency in dependencies) {
				if (AssetDatabase.GetMainAssetTypeAtPath(dependency) == typeof(GameObject)) {
					var tex = AssetDatabase.LoadAssetAtPath<GameObject>(dependency);
					var info =new  FileInfo(dependency);
					//var texMemSize = Profiler.GetRuntimeMemorySizeLong(tex);
					var texMemSize = info.Length;
					//Debug.Log($"{(texMemSize/(1048576f)) : 0.00}MB - {dependency}");
					fileSize += (int)texMemSize;
				}
			}
			modelSize = fileSize;
			//Debug.Log($"Total Model Size: {(fileSize/(1048576f)) : 0.00}MB");
		}


		public void CalculateUsedAudioSize(){
			GameObject diskPrefab = settings.rootPrefabSource;
			var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(diskPrefab));
			int fileSize = 0;
			int fileSizeR = 0;
			foreach (var dependency in dependencies) {
				if (AssetDatabase.GetMainAssetTypeAtPath(dependency) == typeof(AudioClip))
				{
					var importer = AudioImporter.GetAtPath(dependency);
					var atype = importer.GetType();
					BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
					                         | BindingFlags.Static;
					var field = atype.GetProperty("origSize",bindFlags);

					var sizeO = (int)field.GetValue(importer);
					//Debug.Log($"{dependency} original size = {sizeO}");
					var field2 = atype.GetProperty("compSize",bindFlags);
					var sizeC = (int)field2.GetValue(importer);
					//Debug.Log($"\tcomp size = {sizeC}");

					var texMemSize = sizeC;
					//Debug.Log($"{(texMemSize/(1048576f)) : 0.00}MB - {dependency}");
					fileSize += (int)texMemSize;
					fileSizeR += sizeO;
				}
			}
			audioSize = fileSize;
			audioSizeRaw = fileSizeR;
			//Debug.Log($"Total Audio Size: {(fileSize/(1048576f)) : 0.00}MB");
		}

		private int audioSize = 0;
		private int audioSizeRaw = 0;
		public void MakeMORPlaceablePrefabVariant(){
			GameObject diskPrefab = settings.rootPrefabSource;
			//GameObject scenePrefab = FindSceneInstance(diskPrefab);
			GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(diskPrefab);
			string prefabPath = AssetDatabase.GetAssetPath(diskPrefab);
			string newPath = Path.Combine(Path.GetDirectoryName(prefabPath), Path.GetFileNameWithoutExtension(prefabPath) + "_MORArtwork.prefab");
			
			//Turn off shadowcasting on objects, as this should be handled by the lightmap renderer.
			MeshRenderer[] allRenderers = prefabInstance.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer meshRenderer in allRenderers)
			{
				SerializedObject serialized = new SerializedObject(meshRenderer);
				SerializedProperty prop = serialized.FindProperty("m_CastShadows");
				prop.intValue = (int)UnityEngine.Rendering.ShadowCastingMode.Off;
				serialized.ApplyModifiedProperties();
			}
			//Add override to disable all lights
			Light[] lights = prefabInstance.GetComponentsInChildren<Light>(true);
			foreach (Light light in lights) {
				SerializedObject serializedObject = new SerializedObject(light);
				var prop = serializedObject.FindProperty("m_Enabled");
				prop.boolValue = false;
				serializedObject.ApplyModifiedProperties();
			}
			//EditorUtility.SetDirty();
			var newPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstance, newPath,InteractionMode.AutomatedAction);
			settings.MORPlaceablePrefab = newPrefab;
			DestroyImmediate(prefabInstance);
		}
		
		
		
		public void StoreLightmapApply(){
			GameObject diskPrefab = settings.rootPrefabSource;
			GameObject scenePrefab = FindSceneInstance(diskPrefab);
			var lightmapControl = scenePrefab.GetComponentInChildren<StoreLighmapControl>();
			if (lightmapControl == null) {
				AddMORComponentsToRootPrefab(diskPrefab);
				lightmapControl = scenePrefab.GetComponentInChildren<StoreLighmapControl>();
			}
			lightmapControl.AddAndUpdateComponentsToLightmapped();//Do tha thing
		}
		public void SetShadersToMORLightmap(){
			GameObject diskPrefab = settings.rootPrefabSource;
			GameObject scenePrefab = FindSceneInstance(diskPrefab);
			var lightmapControl = scenePrefab.GetComponentInChildren<StoreLighmapControl>();
			if (lightmapControl == null) {
				AddMORComponentsToRootPrefab(diskPrefab);
				lightmapControl = scenePrefab.GetComponentInChildren<StoreLighmapControl>();
			}
			lightmapControl.ChangeShadersToMORLightmap();//Do tha thing
		}
		


		public static Bounds GetBounds(GameObject obj,bool includeInactive = false) {
			Bounds bounds = new Bounds();

			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(includeInactive);
			if (renderers.Length == 0) {
				return bounds;
			}
			bounds = renderers[0].bounds;
			if (renderers.Length < 2) {
				return bounds;
			}
			//Encapsulate for all renderers
			for (int index = 1; index < renderers.Length; index++) {
				Renderer renderer = renderers[index];
				bounds.Encapsulate(renderer.bounds);
			}

			return bounds;
		}

		
		
		public void CreateSkybox(GameObject rootScenePrefab) {
			Bounds prefabBounds = GetBounds(rootScenePrefab);
			
			GameObject newSkybox = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			newSkybox.name = "MOR_LocalSkybox";
			newSkybox.tag = "NewSkybox";
			Collider skyCollider = newSkybox.GetComponent<Collider>();
			DestroyImmediate(skyCollider);
			MeshRenderer skyRenderer = newSkybox.GetComponent<MeshRenderer>();
			MeshFilter skyFilter = newSkybox.GetComponent<MeshFilter>();
			string skyboxMeshPath = SKYBOX_MODEL_PATH_MOR;
			if (SceneManager.GetActiveScene().name != "MuseumHigh") {
				skyboxMeshPath = SKYBOX_MODEL_PATH_STAGING;
			}

			
			Mesh skyMesh = AssetDatabase.LoadAssetAtPath<Mesh>(skyboxMeshPath);
			if (skyMesh != null) {
				skyFilter.sharedMesh = skyMesh;
			}

			skyRenderer.sharedMaterial = RenderSettings.skybox; //TODO : Make sure this switches to a MOR skybox shader if it isn't already.
			var shaderName = skyRenderer.sharedMaterial.shader.name;
			if (shaderName != "_MOR/360 Video Wipe") {
				settings.skyboxValidationState = ArtStagingSettings.ValidationState.HasWarnings;
				warningMessage = "Try using '360 Video Wipe as skybox shader. (set to Mono). See MOR/Art/Shaders/Skybox for an example";
			}

			Transform newSkyboxTransform = newSkybox.transform;
			newSkyboxTransform.position = prefabBounds.center;
			float radius = (prefabBounds.max.magnitude + BOUNDS_EXTRA_FOR_SKYBOX);//magnitude will be distance to furthest corner we'd sweep a radius from (plus a little extra)
			radius = (float)Math.Round(radius, 10, MidpointRounding.AwayFromZero);
			newSkyboxTransform.localScale = new Vector3(radius, radius, radius); //TODO: Get a bounds for the area.
			newSkyboxTransform.SetParent(rootScenePrefab.transform);
			string path = AssetDatabase.GetAssetPath(settings.rootPrefabSource);
			EditorUtility.SetDirty(rootScenePrefab);
			PrefabUtility.ApplyAddedGameObject(newSkybox, path, InteractionMode.AutomatedAction);
		}

		
		public void ChangeSkyboxShaderToMORLightmap() {
			
			var shader = Shader.Find("_MOR/360 Video Wipe");
			if (shader == null) {
				Debug.LogError("Failed to find _MOR/60 Video Wipe shader");
				return;
			}
			
			var mat = RenderSettings.skybox;
			if (mat == null) {
				Debug.LogWarning("No skybox assigned (Lighting>Environment>Skybox Material");
				return;
			}

			if (mat.shader != null && mat.shader.name == "_MOR/360 Video Wipe") {
				Debug.Log("Material already has the MOR shader applied");
				return;
			}
			var originalPath = AssetDatabase.GetAssetPath(mat);
			string pathRoot = Path.GetFileNameWithoutExtension(originalPath);
			pathRoot =Path.Combine( Path.GetDirectoryName(originalPath), pathRoot + "_MORSkybox.mat");
			var newMat = Instantiate(mat);
			if (newMat.shader != shader) {
				newMat.shader = shader;
				newMat.SetInt("_MonoMode", 1);
				newMat.SetInt("_StencilComp", 0);
			}			
			AssetDatabase.CreateAsset(newMat, pathRoot);
			Debug.Log($"Saving new material as :  {pathRoot}");
			var localSkybox = GameObject.FindWithTag("LocalSkybox");
			if (localSkybox == null) {
				Debug.LogWarning("No Local Skybox. New Material not applied");
				return;
			}
		}



		/// <summary>
		/// Skyboxes might need to be created for other dimension.
		/// </summary>
		/// <returns></returns>
		bool CheckSkybox() {
			if (settings.isThroughPortal == false) {
				return true;
			}

			Material sky = RenderSettings.skybox;
			if (sky == null) {
				return true;
			}

			return false;
		}

		private void OnInspectorUpdate() {
			Repaint();
		}

		/// <summary>
		/// see if there are teleport areas for moving around.
		/// </summary>
		/// <returns></returns>
		public bool CheckColliders() {
			if (settings.isThroughPortal == false) {
				return true;
			}
			GameObject diskPrefab = settings.rootPrefabSource;
			GameObject scenePrefab = FindSceneInstance(diskPrefab);
			Collider[] colliders  = scenePrefab.GetComponentsInChildren<Collider>();
			if (colliders.Length == 0) {
				settings.colliderValidationState = ArtStagingSettings.ValidationState.CriticalFail;
				errorMessage = "No Collision in scene. Add collision to floors and walls.";
				return false;
			}
			
			TeleportArea[] teleportAreas = settings.rootPrefabSource.GetComponentsInChildren<TeleportArea>();
			if (teleportAreas != null && teleportAreas.Length > 0) {
				return true;
			}
			settings.colliderValidationState = ArtStagingSettings.ValidationState.CriticalFail;
			errorMessage = "No Teleport Areas set. Please add a 'Teleport Area' modifier to floor colliders.";
			
			Debug.LogError($"No Teleport Areas set. Please add a 'Teleport Area' modifier to floor colliders.",colliders[0]);
			return false;
		}

		/// <summary>
		/// Make sure playable director settings are correct if applicable
		/// </summary>
		/// <returns></returns>
		public bool CheckPlayableDirector() {
			GameObject diskPrefab = settings.rootPrefabSource;
			PlayableDirector d = diskPrefab.GetComponentInChildren<PlayableDirector>();
			if (d == null) {
				return true;
			}

			if (d.timeUpdateMode != DirectorUpdateMode.DSPClock) {
				Debug.LogWarning("Playable Director should be on DSP mode for audio match and VR timing", d.gameObject);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Adjust to best setting for MOR
		/// </summary>
		public void FixPlayableDirector() {
			GameObject diskPrefab = settings.rootPrefabSource;
			PlayableDirector d = diskPrefab.GetComponentInChildren<PlayableDirector>();
			if (d == null) {
				return;
			}

			if (d.timeUpdateMode != DirectorUpdateMode.DSPClock) {
				SerializedObject o = new SerializedObject(d);
				var prob = o.FindProperty(nameof(d.timeUpdateMode));
				prob.intValue = (int)DirectorUpdateMode.DSPClock;
				o.ApplyModifiedProperties();
				PrefabUtility.SavePrefabAsset(diskPrefab);
			}
		}

		/// <summary>
		/// Set reflection probes to lock in probes from this scene so that objects don't try to use the MOR probes.
		/// </summary>
		public void ChangeProbesToCustom() {
			GameObject prefab = settings.rootPrefabSource;
			GameObject scenePrefab = FindSceneInstance(prefab);
			if (scenePrefab == null) {
				return;
			}

			ReflectionProbe[] probes = scenePrefab.GetComponentsInChildren<ReflectionProbe>(true);
			foreach (ReflectionProbe probe in probes) {
				ReflectionProbe prefabProbe = PrefabUtility.GetCorrespondingObjectFromSource(probe);
				ReflectionProbeMode type = prefabProbe.mode;

				/*
				SerializedObject s = new SerializedObject(prefabProbe);
				string customProbeProp = "m_CustomBakedTexture";
				string normalProbeProp = "m_BakedTexture";
				
				var customProbe = s.FindProperty(customProbeProp);
				var normalProbe = s.FindProperty(normalProbeProp);
				Debug.Log(customProbe?.objectReferenceValue?.name);
				Debug.Log(normalProbe?.objectReferenceValue?.name);
				Type ftype = probe.GetType();
				var propInfo = ftype.GetField(customProbeProp);
				if (propInfo != null) {
					var getProp = (Texture2D)propInfo.GetValue(probe);
					Debug.Log(getProp);
				}
				var propInfo2 = ftype.GetField(normalProbeProp);
				if (propInfo2 != null) {
					var getProp2 = (Texture2D)propInfo.GetValue(probe);
					Debug.Log(getProp2);
				}
				*/
				//probe.bakedTexture;

				Debug.Log(probe.bakedTexture);
				if (probe.customBakedTexture == null && probe.bakedTexture != null) {
					/*var p = s.FindProperty("m_CustomBakedTexture");
					if (p != null) {
						p.objectReferenceValue = prefabProbe.bakedTexture;
						Debug.Log("Set baked texture");
					}*/
					prefabProbe.customBakedTexture = probe.bakedTexture;
				}

				if (type != ReflectionProbeMode.Custom) {
					/*var p = s.FindProperty(nameof(probe.mode));
					if (p != null) {
						p.intValue = (int)ReflectionProbeMode.Custom;
					}*/
					prefabProbe.mode = ReflectionProbeMode.Custom;
					prefabProbe.renderDynamicObjects = true;
				}

				//s.ApplyModifiedProperties();
				EditorUtility.SetDirty(prefabProbe);
			}

			PrefabUtility.SavePrefabAsset(prefab);
		}

		public void AssignReflectionProbes(GameObject diskPrefab) {
			if (diskPrefab == null) {
				return;
			}

			GameObject scenePrefab = FindSceneInstance(diskPrefab);
			MeshRenderer[] meshes = scenePrefab.GetComponentsInChildren<MeshRenderer>(true);
			int i = 0;
			ReflectionProbe[] probes = scenePrefab.GetComponentsInChildren<ReflectionProbe>(true);
			settings.lightProbeValidationState = ArtStagingSettings.ValidationState.Valid;
			errorMessage = "";
			foreach (MeshRenderer meshRenderer in meshes) {
				MeshRenderer prefabRenderer = PrefabUtility.GetCorrespondingObjectFromSource(meshRenderer);

				SerializedObject s = new SerializedObject(prefabRenderer);
				SerializedProperty prop = s.FindProperty("m_LightProbeUsage");
				prop.intValue = (int)LightProbeUsage.Off;

				prefabRenderer.lightProbeUsage = LightProbeUsage.Off;
				meshRenderer.lightProbeUsage = LightProbeUsage.Off;

				if (prefabRenderer.reflectionProbeUsage == ReflectionProbeUsage.Off) {
					Debug.Log($"skip off probes : {prefabRenderer.name}", meshRenderer.gameObject);
					continue;
				}

				prop = s.FindProperty("m_ReflectionProbeUsage");

				/*if (prop == null) {
					Debug.Log($"m_ReflectionProbeUsage not a property we could find", meshRenderer.gameObject);
					continue;
				}*/

				prop.intValue = (int)ReflectionProbeUsage.Simple;
				if (prefabRenderer.reflectionProbeUsage != ReflectionProbeUsage.Simple) {
					prefabRenderer.reflectionProbeUsage = ReflectionProbeUsage.Simple;
					meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
				}

				if (prefabRenderer.probeAnchor != null) {
					//Debug.Log($"skip probe assigned  :  {meshRenderer.name}",meshRenderer.gameObject);
					continue;
				}

				if (meshRenderer.probeAnchor != null) {
					prefabRenderer.probeAnchor = meshRenderer.probeAnchor;
				}

				ReflectionProbe probe = GetStrongestProbe(meshRenderer); //use scene renderer to find scene probe.

				if (probe == null) {
					float minDistance = float.MaxValue;
					foreach (ReflectionProbe reflectionProbe in probes) {
						float distance = Vector3.Distance(meshRenderer.bounds.center, reflectionProbe.center);
						if (distance < minDistance) {
							minDistance = distance;
							probe = reflectionProbe;
						}
					}

					if (probe == null) {
						Debug.Log($"Probe null {i++}", meshRenderer.gameObject);
						EditorUtility.SetDirty(meshRenderer);
						AssetDatabase.SaveAssets();
						continue;
					}
				}

				ReflectionProbe diskProbe = PrefabUtility.GetCorrespondingObjectFromSource(probe);
				if (diskProbe == null) {
					Debug.Log($"Reflection probe not part of prefab.", probe.gameObject);
					this.settings.lightProbeValidationState = ArtStagingSettings.ValidationState.HasWarnings;
					errorMessage = $"Custom Reflection Probe must be part of Prefab. = {probe}";
					continue;
				}
				prop = s.FindProperty("m_ProbeAnchor");
				if (prop == null || prop.propertyType != SerializedPropertyType.ObjectReference) {
					
					Debug.Log($"m_ProbeAnchor not a property we could find. {prop} - type = {(prop==null?"":prop.propertyType.ToString())}", meshRenderer.gameObject);
					continue;
				}

				//string path = AssetDatabase.GetAssetPath(this);
				//GameObject assetRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer.gameObject);
				prop.objectReferenceValue = diskProbe.transform;
				prefabRenderer.probeAnchor = diskProbe.transform;
				//PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction );
				EditorUtility.SetDirty(prefabRenderer);
				s.ApplyModifiedPropertiesWithoutUndo();

				//PrefabUtility.ApplyPropertyOverride(prop,path,InteractionMode.AutomatedAction );
				//PrefabUtility.ApplyObjectOverride(meshRenderer,path,InteractionMode.AutomatedAction);
			}

			//if (type != PrefabAssetType.NotAPrefab) {
			PrefabUtility.SavePrefabAsset(diskPrefab);
			//}
			Selection.activeObject = diskPrefab;
			AssetDatabase.SaveAssets();
		}


		public ReflectionProbe GetStrongestProbe(Renderer mRenderer) {
			ReflectionProbe probe = null;
			List<ReflectionProbeBlendInfo> probes = new List<ReflectionProbeBlendInfo>();
			mRenderer.GetClosestReflectionProbes(probes);
			float biggestWeight = -1;
			foreach (ReflectionProbeBlendInfo probeInfo in probes) {
				//if (probeInfo.probe.transform.root != mRenderer.transform.root) {
				//Debug.Log($"probe not on same root : {probeInfo.probe.transform.root}  : {mRenderer.transform.root}");
				//	continue;
				//}

				if (probeInfo.weight > biggestWeight) {
					probe = probeInfo.probe;
					biggestWeight = probeInfo.weight;
				}
			}

			return probe;
		}

		/// <summary>
		/// find materials on models for extraction.
		/// </summary>
		private void CheckMaterials() {
			GameObject sourcePrefab = settings.rootPrefabSource;
			MeshRenderer[] renderers = sourcePrefab.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer meshRenderer in renderers) {
				/*ReceiveGI giFlag = meshRenderer.receiveGI;
				if (giFlag != ReceiveGI.Lightmaps) {
					continue;
				}*/
				// I had thought to leave non-lightmapped objects out of this, but we likely want to set ambient and directional light settigns on material
				// so need MOR material anyways.
				Material[] materials = meshRenderer.sharedMaterials;
				foreach (Material material in materials) {
					string modelPath = AssetDatabase.GetAssetPath(material);
					//If source is in an FBX we need to extract it
					if (modelPath.EndsWith(".fbx") == false) {
						continue;
					}

					Shader shader = material.shader;
					if (shader.name != "Standard") {
						continue; //I don't know if embedded materials can be non-unity standard, but check anyways.
					}

					settings.materialValidationState = ArtStagingSettings.ValidationState.CriticalFail;
					settings.Save();
					Debug.LogError($"Lightmapped Materials must be extracted from model. {material.name}", material);

					return;
				}
			}

			settings.materialValidationState = ArtStagingSettings.ValidationState.Valid;
			settings.Save();
		}

		/// <summary>
		/// Lightmapping changes require materials that we can change shaders on, so extract materials referenced on fbx etc. 
		/// </summary>
		/// <param name="sourcePrefab"></param>
		public void ExtractEmbeddedMaterials(GameObject sourcePrefab) {
			MeshRenderer[] meshes = sourcePrefab.GetComponentsInChildren<MeshRenderer>(true);
			List<Material> newMaterials = new List<Material>();
			List<string> assetPaths = new List<string>();
			AssetDatabase.StartAssetEditing();
			try {
				foreach (MeshRenderer mesh in meshes) {
					Material meshMaterial = mesh.sharedMaterial;
					string modelPath = AssetDatabase.GetAssetPath(meshMaterial);
					//If source is in an FBX we need to extract it
					if (modelPath.EndsWith(".fbx") == false) {
						continue;
					}

					//search for ones we've already done this pass
					int matIndex = assetPaths.IndexOf(modelPath);
					if (matIndex >= 0) {
						mesh.sharedMaterial = newMaterials[matIndex];
						continue;
					}

					string sourceFilename = meshMaterial.name;
					string outPath = Path.Combine(Path.GetDirectoryName(modelPath) ?? string.Empty, $"{sourceFilename}.mat");
					//If material exists on disk in same location, use that asset instead of creating a new one
					if (File.Exists(outPath) == false) {
						var newMat = Instantiate(meshMaterial);
						AssetDatabase.CreateAsset(newMat, outPath);
					}

					Material diskMaterial = AssetDatabase.LoadAssetAtPath<Material>(outPath);
					//Store in list for future iterations
					newMaterials.Add(diskMaterial);
					assetPaths.Add(outPath);
					//Assign
					mesh.sharedMaterial = diskMaterial;
					Debug.Log(outPath, mesh.gameObject);

					// ReSharper disable once AccessToStaticMemberViaDerivedType
					ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(modelPath);
					importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
					importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
				}

				AssetDatabase.StopAssetEditing();
			}
			catch (Exception e) {
				Debug.LogError(e);
				AssetDatabase.StopAssetEditing();
			}
		}

		/// <summary>
		/// Verify existence of MOR components on root which are needed
		/// </summary>
		// ReSharper disable once InconsistentNaming
		private void RootHasMORComponents() {
			if (settings == null) {
				return;
			}

			GameObject root = settings.rootPrefabSource;
			if (root == null) {
				componentsOnRoot = false;
				return;
			}

			Artwork artwork = root.GetComponent<Artwork>();
			if (artwork == null) {
				componentsOnRoot = false;
				return;
			}

			StoreLighmapControl lightmapStorage = root.GetComponent<StoreLighmapControl>();
			if (lightmapStorage == null) {
				componentsOnRoot = false;
				return;
			}

			componentsOnRoot = true;
		}

		/// <summary>
		/// Add MOR components as needed on prefab root directly.
		/// </summary>
		/// <param name="root"></param>
		// ReSharper disable once InconsistentNaming
		public void AddMORComponentsToRootPrefab(GameObject root) {
			Artwork artwork = root.GetComponent<Artwork>();
			if (artwork == null) {
				//artwork = 
				root.AddComponent<Artwork>();
			}

			StoreLighmapControl lightmapStorage = root.GetComponent<StoreLighmapControl>();
			if (lightmapStorage == null) {
				//lightmapStorage = 
				root.AddComponent<StoreLighmapControl>();
			}

			EditorUtility.SetDirty(root);
			PrefabUtility.SavePrefabAsset(root);
		}


		/// <summary>
		/// Validate all meshes.
		/// </summary>
		public void DoMeshCheck() {
			string[] resultWarnings = CheckMeshIntegrity(settings.rootPrefabSource, otherDimension: settings.isThroughPortal);
			bool hasErrors = string.IsNullOrEmpty(resultWarnings[0]) == false;
			bool hasWarnings = string.IsNullOrEmpty(resultWarnings[1]) == false;
			settings.modelsValidationState =
				hasErrors ? ArtStagingSettings.ValidationState.CriticalFail : (hasWarnings ? ArtStagingSettings.ValidationState.HasWarnings : ArtStagingSettings.ValidationState.Valid);
			settings.Save();
			if (hasErrors) {
				errorMessage = "Meshes have errors. See Console for details.";
				Debug.LogError(resultWarnings[0]);
			} else {
				errorMessage = "";
			}

			if (hasWarnings) {
				warningMessage = "Meshes have warnings. See Console for details.";
				Debug.LogWarning(resultWarnings[1]);
			} else {
				warningMessage = "";
			}
		}

		/// <summary>
		/// Find first instance of prefab on disc in the current scene.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public GameObject FindSceneInstance(GameObject root) {
			string rootPath = AssetDatabase.GetAssetPath(root);
			//Debug.Log(rootPath,root);
			GameObject sceneObj = null;
			GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			//GameObject[] allObjects = FindObjectsOfType<GameObject>();
			foreach (GameObject gameObject in rootObjects) {
				string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
				//Debug.Log(rootPath);
				if (rootPath == prefabPath) {
					sceneObj = gameObject;
					break;
				}
			}

			return sceneObj;
		}

		/// <summary>
		/// Check for scene/environment level issues  
		/// </summary>
		/// <returns></returns>
		protected (bool, bool) ValidateProject() {
			bool hasWarnings = false;
			bool hasErrors = false;
			// Check for prefab root in scene :
			if (settings.rootPrefabSource == null) {
				hasErrors = true;
				errorMessage = "Store all your relevant scene object in a prefab and assign it here.";
				Debug.LogError("No Prefab set for your scene. Please collect all your assets under one root and prefab it.");
			}

			//Check for un-applied Overrides
			GameObject root = settings.rootPrefabSource;
			GameObject rootInstance = FindSceneInstance(root);
			if (rootInstance == null) {
				Debug.LogWarning("Referenced Prefab not found in scene");
				return (hasErrors, false);
			}

			PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(rootInstance);
			List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(rootInstance);
			if (overrides != null && overrides.Count > 0) {
				hasWarnings = true;
				warningMessage = "Root has un-applied overrides";
				/*Debug.Log("MODIFICATIONS");
				foreach (PropertyModification modification in modifications) {
					Debug.Log($"{modification.propertyPath} {modification.target}",modification.target);
				}*/
				Debug.Log("OVERRIDES");
				foreach (var modification in overrides) {
					Debug.Log($"{modification.coupledOverride} {modification.GetAssetObject()}", modification.instanceObject);
				}
			} else {
				warningMessage = "";
				//Debug.Log($"No property modification {modifications} - {overrides}");
			}


			//Check Mesh File sources +  Material counts

			//Check Texture File Sizes

			// Movies ???


			//TODO : Mor 'optional' MOR setup which users can try doing if they want, but can be left for MOR Staff
			//Lightmaps : setup materials and bindings.

			//Skybox setup

			return (hasErrors, hasWarnings);
		}


		protected GameObject GetRootPrefab() {
			GameObject root = null;
			GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (GameObject rootObject in rootObjects) {
				if (Array.IndexOf(morRootGameObjects, rootObject.name) != -1) {
					continue;
				}
				//Object not in whitelist

				PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(rootObject);
				if (prefabType == PrefabAssetType.NotAPrefab) {
					nonPrefabbedObjects.Add(rootObject);
					continue;
				}

				if (root == null) {
					root = rootObject;
				}
			}

			if (nonPrefabbedObjects != null && nonPrefabbedObjects.Count > 0) {
				warningMessage = "Non MOR objects in scene and not prefabbed.";
			}

			return PrefabUtility.GetCorrespondingObjectFromSource(root);
		}


		public static string[] CheckMeshIntegrity(GameObject g, string assetPath = "", bool otherDimension = false) {
			needsMeshSplit = false;
			Renderer[] renderers = g.GetComponentsInChildren<Renderer>(true);
			int vertexCountTotal = 0;
			string errors = "";
			string warnings = "";
			foreach (Renderer renderer in renderers) {
				Mesh renderMesh;
				if (renderer is SkinnedMeshRenderer) {
					continue; //TODO: pending confidence in skinned mesh splitting.
					//renderMesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
				} else if (renderer is MeshRenderer) {
					MeshFilter mFilter = renderer.GetComponent<MeshFilter>();
					renderMesh = mFilter.sharedMesh;
				} else {
					continue;
				}

				if (renderMesh == null) {
					continue;
				}

				int vertexCount = renderMesh.vertexCount;

				vertexCountTotal += vertexCount;
				if (vertexCount > ushort.MaxValue) {
					warnings += ($"MOR - Mesh has too many verts for 16bit indexing `{vertexCount}` - {renderer.name} Consider splitting into multiple parts : model = {assetPath}  part = {g.name}\n");
				}

				if (renderer.sharedMaterials.Length > 1) {
					needsMeshSplit = true;
					errors += ($" Multi-Material Renderer : Please have 1 material per object. {renderer.gameObject.name}\n");
				}

				string filepath = AssetDatabase.GetAssetPath(renderMesh);
				string extension = Path.GetExtension(filepath);
				if (string.IsNullOrEmpty(extension)) {
					continue;
				}

				if (Array.IndexOf(NonFbxFiletypes, extension.ToLower()) >= 0) {
					if (HasHighDimensionUVs(renderMesh)) {
						//.fbx doesn't support uv's higher than Vector2
						continue;
					}

					errors += ($"INVALID FILETYPE '{extension}': please export all models as .fbx : {filepath}\nGameObject/Export To FBX... in menu bar\n");
					badFileFormats = true;
				} else if (extension == ".asset") {
					if (HasHighDimensionUVs(renderMesh)) {
						//.fbx doesn't support uv's higher than Vector2
						continue;
					}

					errors += $"Mesh in .asset file. Export to .fbx : {filepath} - {renderMesh.name}";
				} else if (string.CompareOrdinal(".fbx", extension.ToLower()) != 0) { //wait, what filetype is this?
					errors += ($"INVALID FILETYPE '{extension}': please export all models as .fbx : {filepath}\nGameObject/Export To FBX... in menu bar\n");
					badFileFormats = true;
				}
			}

			if (vertexCountTotal > VERT_COUNT_MAX_PC) {
				if (vertexCountTotal > VERT_COUNT_MAX_PC * (otherDimension ? 3.5f : 1.2f)) {
					errors += ($"Model has too many vertices, please simplify the model :{g.name} model - {assetPath}  size = {vertexCountTotal} vertices.\n");
				} else {
					warnings += ($"Model likely has too many vertices, please simplify the model :{g.name} model - {assetPath}  size = {vertexCountTotal} vertices.\n");
				}
			}


			return new[] { errors, warnings };
		}

		private static bool HasHighDimensionUVs(Mesh renderMesh) {
			VertexAttributeDescriptor[] vAttributes = renderMesh.GetVertexAttributes();
			foreach (VertexAttributeDescriptor attr in vAttributes) {
				int attributeID = (int)attr.attribute;
				if (attributeID < (int)VertexAttribute.TexCoord0 || attributeID > (int)VertexAttribute.TexCoord7) {
					continue;
				}

				if (attr.dimension > 2) {
					// FBX can't store more than Vector2 uv's, so don't convert to .fbx
					return true;
				}
			}

			return false;
		}


		protected static T[] GetMeshPropertyArray<T>(T[] sourceArray, int firstIndex, int numVerts) {
			T[] outArray = new T[numVerts];
			for (int j = 0; j < numVerts; j++) {
				int sourceIndex = j + firstIndex;
				outArray[j] = sourceArray[sourceIndex];
			}

			return outArray;
		}


		public static void SplitMeshes(GameObject scenePrefab) {
			GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(scenePrefab);
			if (sourcePrefab == null) {
				Debug.Log("Not part of a prefab for export.",scenePrefab);
				return;
			}
			string rootSourcePath = AssetDatabase.GetAssetPath(sourcePrefab);

			MeshRenderer[] meshes = scenePrefab.GetComponentsInChildren<MeshRenderer>(true);
			List<Mesh> meshSources = new List<Mesh>();
			List<string> splitFileSources = new List<string>();
			foreach (MeshRenderer sceneMeshRenderer in meshes) {
				if (sceneMeshRenderer.sharedMaterials.Length < 2) {
					continue;
				}

				MeshFilter meshFilter = sceneMeshRenderer.GetComponent<MeshFilter>();
				Mesh sourceMesh = meshFilter.sharedMesh;
				if (HasHighDimensionUVs(sourceMesh)) {
					//For now skip, but look into flagging to export to .asset instead.
					continue;
				}

				string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
				Debug.Log(sourcePath, sceneMeshRenderer.gameObject);

				//copy out of hierarchy for target duplication
				GameObject sourceObject = Instantiate(sceneMeshRenderer.gameObject, sceneMeshRenderer.transform.parent);
				Transform sourceObjectTransform = sourceObject.transform;
				if (sourceObjectTransform.childCount > 0) { //Clear Children.
					for (int i = sourceObjectTransform.childCount - 1; i >= 0; i--) {
						Transform child = sourceObjectTransform.GetChild(i);
						DestroyImmediate(child.gameObject);
					}
				}

				//info for Colliders
				MeshCollider meshCollider = sceneMeshRenderer.GetComponent<MeshCollider>();
				bool hasMeshCollider = meshCollider != null && meshCollider.sharedMesh == sourceMesh;
				Collider otherCollider = hasMeshCollider ? null : sceneMeshRenderer.GetComponent<Collider>();
				bool hasOtherCollider = otherCollider != null;

				int subMeshCount = sourceMesh.subMeshCount;
				Object[] newObjects = new Object[subMeshCount];
				MeshFilter[] newFilters = new MeshFilter[subMeshCount];


				//Create Destination Objects
				for (int i = 0; i < subMeshCount; i++) {
					GameObject newObject = Instantiate(sourceObject, sceneMeshRenderer.transform); //Need to make sure no children copied so don't parent yet
					MeshRenderer newRenderer = newObject.GetComponent<MeshRenderer>();
					newFilters[i] = newObject.GetComponent<MeshFilter>();
					newRenderer.sharedMaterials = new[] { sceneMeshRenderer.sharedMaterials[i] };
					newObjects[i] = newObject; //add to out array.
				}


				int indexInList = meshSources.IndexOf(sourceMesh);
				string resultPath;
				if (indexInList >= 0) { //We've already exported this before.
					resultPath = splitFileSources[indexInList];
				} else { //Make new meshes, and export to fbx.


					Vector3[] sourceVerts = sourceMesh.vertices;
					Vector2[] sourceUvs0 = sourceMesh.uv;
					int[] sourceFaces = sourceMesh.triangles;
					if (sourceFaces.Length == 0) {
						Debug.LogError($"SubMesh had no faces", meshFilter.gameObject);
						continue;
					}

					if (sourceFaces.Length % 3 != 0) {
						Debug.LogError($"SubMesh faces not multiple of 3? {sourceFaces.Length} ", meshFilter.gameObject);
						continue;
					}

					Mesh[] newMeshes = new Mesh[subMeshCount];
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
						if (indexCount == 0 || indexCount % 3 != 0) {
							Debug.LogWarning($"SubMesh face count is strange : {indexCount} = {sceneMeshRenderer.name}");
						}

						MeshFilter newFilter = newFilters[i];
						//Make new mesh based off of old.
						Mesh newMesh = new Mesh();
						newMeshes[i] = newMesh;
						newMesh.name = $"{sceneMeshRenderer.name}{i:00}";


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
					
					string outPath = Path.Combine(Path.GetDirectoryName(sourcePath), $"{sourceFilename}_{sourceMesh.name}_split.fbx");
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
				MeshFilter[] importMeshes = newFBX.GetComponentsInChildren<MeshFilter>(true);
				for (int i = 0; i < subMeshCount; i++) {
					GameObject newObject = (GameObject)newObjects[i];

					newObject.transform.SetParent(sceneMeshRenderer.transform, false);
					newObject.transform.localPosition = Vector3.zero;
					newObject.transform.localRotation = Quaternion.identity;
					newObject.transform.localScale = Vector3.one;
					if (i >= importMeshes.Length) {
						Debug.LogError($"Wrong number of meshes {importMeshes.Length} - index = {i}", sceneMeshRenderer.gameObject);
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

					EditorUtility.SetDirty(scenePrefab);
					PrefabUtility.ApplyAddedGameObject(newObject, rootSourcePath, InteractionMode.AutomatedAction);
					//Debug.Log(rootSourcePath);
				}

				//Clean up the meshes we just exported and no longer need, as we're getting them from file now.
				MeshFilter original = PrefabUtility.GetCorrespondingObjectFromSource(meshFilter);
				GameObject prefabObject = meshFilter.gameObject;
				DestroyImmediate(meshFilter);
				PrefabUtility.ApplyRemovedComponent(prefabObject, original, InteractionMode.AutomatedAction);
				StoreLightmapOffset lmOffset = sceneMeshRenderer.GetComponent<StoreLightmapOffset>();

				MeshRenderer originalMeshRenderer = PrefabUtility.GetCorrespondingObjectFromSource(sceneMeshRenderer);
				DestroyImmediate(sceneMeshRenderer);
				PrefabUtility.ApplyRemovedComponent(prefabObject, originalMeshRenderer, InteractionMode.AutomatedAction);
				if (hasMeshCollider) {
					MeshCollider originalMeshCollider = PrefabUtility.GetCorrespondingObjectFromSource(meshCollider);
					DestroyImmediate(meshCollider);
					PrefabUtility.ApplyRemovedComponent(prefabObject, originalMeshCollider, InteractionMode.AutomatedAction);
				}

				if (lmOffset != null) {
					StoreLightmapOffset originalOffset = PrefabUtility.GetCorrespondingObjectFromSource(lmOffset);
					DestroyImmediate(lmOffset);
					PrefabUtility.ApplyRemovedComponent(prefabObject, originalOffset, InteractionMode.AutomatedAction);
				}

				DestroyImmediate(sourceObject); //kill temp object used for copying
			}
		}

		public void ConvertLooseMeshesToFBX(GameObject root) {
			MeshRenderer[] meshes = root.GetComponentsInChildren<MeshRenderer>(true);
			List<Mesh> meshSources = new List<Mesh>();
			List<string> splitFileSources = new List<string>();

			foreach (MeshRenderer meshRenderer in meshes) {
				MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				Mesh sourceMesh = meshFilter.sharedMesh;
				if (sourceMesh == null) {
					continue;
				}

				string modelPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
				if (string.IsNullOrEmpty(modelPath)) {
					Debug.Log($"Null model path on object {meshRenderer.name}", meshRenderer.gameObject);
				}

				string extension = Path.GetExtension(modelPath);
				//If source is in an FBX we need to extract it
				if (string.IsNullOrEmpty(extension) || extension.ToLower().EndsWith(".fbx")) {
					continue;
				}

				if (HasHighDimensionUVs(sourceMesh)) {
					continue;
				}

				Debug.Log($"Mesh asset not in FBX file : {Path.GetExtension(modelPath)} - {modelPath}", meshRenderer.gameObject);
				string sourceFilename = Path.GetFileNameWithoutExtension(modelPath);
				string outPath = Path.Combine(Path.GetDirectoryName(modelPath) ?? string.Empty, $"{sourceFilename}_{sourceMesh.name}.fbx");

				Mesh inMesh;
				int index = splitFileSources.IndexOf(outPath);
				if (index >= 0) { //Done this one already
					inMesh = meshSources[index];
				} else { //New one this pass
					if (File.Exists(outPath) == false) {
						//Create new file and load
						outPath = ModelExporter.ExportObject(outPath, meshRenderer.gameObject);
						Debug.Log($"Exporter - {outPath}");
						if (string.IsNullOrEmpty(outPath)) {
							continue;
						}
					}

					GameObject meshGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(outPath);
					if (meshGameObject == null) {
						continue;
					}

					MeshFilter inFilter = meshGameObject.GetComponentInChildren<MeshFilter>(true);
					inMesh = inFilter.sharedMesh;
					meshSources.Add(inMesh);
					splitFileSources.Add(outPath);
				}

				MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
				if (meshCollider != null && meshCollider.sharedMesh == meshFilter.sharedMesh) {
					meshCollider.sharedMesh = inMesh;
				}

				meshFilter.sharedMesh = inMesh;
			}
		}


		public void CheckTextures(GameObject root) {
			Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
			long fileSizeTotal = 0;
			List<Material> usedMaterial = new List<Material>(128);
			List<Texture> usedTextures = new List<Texture>(128);
			settings.textureValidationState = ArtStagingSettings.ValidationState.Valid;

			foreach (Renderer renderer in renderers) {
				if (!(renderer is MeshRenderer)) {
					continue;
				}

				Material material = renderer.sharedMaterial;
				//check each material only once.
				if (usedMaterial.Contains(material)) {
					continue;
				}

				usedMaterial.Add(material);

				int[] textureProperties = material.GetTexturePropertyNameIDs();
				foreach (int textureProperty in textureProperties) {
					Texture tex = material.GetTexture(textureProperty);
					if (usedTextures.Contains(tex) || tex == null) {
						continue;
					}

					usedTextures.Add(tex);
					string filepath = AssetDatabase.GetAssetPath(tex);
					if (string.IsNullOrEmpty(filepath)) {
						Debug.Log($"Null filepath on texture : {tex.name}", tex);
						continue;
					}

					FileInfo info = new FileInfo(filepath);
					long size = info.Length;
					//greater than 100MB
					if (size > 100_000_000) {
						largeFileSize = true;
						settings.textureValidationState = ArtStagingSettings.ValidationState.CriticalFail;
						Debug.LogError("Files must be under 100MB on disk", tex);
					}

					fileSizeTotal += size;
				}
			}

			if (fileSizeTotal > 500_000_000) {
				if (settings.textureValidationState != ArtStagingSettings.ValidationState.CriticalFail) {
					settings.textureValidationState = ArtStagingSettings.ValidationState.HasWarnings;
				}

				Debug.LogWarning("Total file size exceeds 500MB. Please reduce some file sizes");
			}

			settings.Save();
		}

		static ArtStagingEditor() {
			EditorApplication.update += Update;
		}

		static void Update() {
			bool show;
			var sceneName = SceneManager.GetActiveScene().name;
			show = sceneName != "MuseumHigh";
			if (show) {
				Init();
			}


			EditorApplication.update -= Update;
		}
	}
}