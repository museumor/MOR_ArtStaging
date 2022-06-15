using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
/// Our version of TeleportArea with Dimension knowledge
/// </summary>
namespace MOR.Industries {
	[AddComponentMenu("MOR/Teleport Area MOR")]
	public class TeleportArea : MonoBehaviour {
		public UnityEvent onEnter;
		public UnityEvent onExit;
		
		[Serializable]
		public class RoomFloatSettings
		{
			public bool freeFloating = false;
			[ReadOnly]public float floatSpeedOverride = 0;
			[ReadOnly]public float floatAccelerationOverride = 0;
			[ReadOnly]public float floatDecelerationOverride = 0;
			[Tooltip("In global space")]
			public float floatMaxHeight = 0;
			[ReadOnly]public float featherFallSpeed = 0;
		}
		[Serializable]
		public class RoomGraphicsSettings
		{
			//was using nullable types for this but they don't serialize
			public bool overrideFog = false;
			public bool fog;
			public bool overrideFogDensity = false;
			public float fogDensity;
			public bool overrideFogColour = false;
			public Color fogColour = Color.white;

			public bool overrideAmbientLight = false;
			[ColorUsageAttribute(false,true)]public Color ambientLight;
			[ReadOnly]public Color floorLightColor = Color.white;
			public enum PostProcessRoomSetting { Default, DisableAll, Custom }

			[ReadOnly]public PostProcessRoomSetting postProcessSetting;
			[ReadOnly]public UnityEngine.Rendering.PostProcessing.PostProcessProfile customProfile;

			[ReadOnly]public float farClipOverride = 0;
		}
		
		
		
		public bool overrideGlobalFog;
		[ConditionalField("overrideGlobalFog")]
		public Color localFogColor;
		
		public bool overrideRoomGraphicsSettings = false;
		[ConditionalField("overrideRoomGraphicsSettings")]
		public RoomGraphicsSettings roomGraphicsOverride;

		public bool overrideRoomFloatSettings = false;
		[ConditionalField("overrideRoomFloatSettings")]
		public RoomFloatSettings roomFloatOverride;
		

		//public bool overrideShadowFarDistance;
		//[ConditionalField("overrideShadowFarDistance")]
		//public float shadowFarDistanceOverride = 4;
		
		//[Space(10)] public bool stickToSurface;
		//public bool StickToSurface;// => CompareTag(Teleport.STICK_TO_SURFACE_TAG);

	}
#if UNITY_EDITOR

	[CustomEditor(typeof(TeleportArea))]
	[CanEditMultipleObjects]
	public class TeleportAreaEditor : Editor {

		public override void OnInspectorGUI() {
			TeleportArea teleportArea = (TeleportArea) target;
			//Leaving this check here in case we decide to remove the automatic layer setting below.
			/*string warning = WarningsCheck(teleportArea);
			if (string.IsNullOrEmpty( warning)==false) {
			    EditorGUILayout.HelpBox(warning,MessageType.Warning);
			}*/
			//Automatically set layer
			//if (!teleportArea.disalowTeleport) {
				//teleportArea.gameObject.layer = LayerMask.NameToLayer("FloorOther");
				teleportArea.gameObject.layer = LayerMask.NameToLayer("Floor");

			//}

			//Set tag if applicable.


			DrawDefaultInspector();
		}
	}
#endif
}