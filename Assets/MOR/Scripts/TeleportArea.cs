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

}