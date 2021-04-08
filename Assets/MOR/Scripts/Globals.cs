
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// All Awake and OnEnable > All Start > All Update
/// FixedUpdate (multiple per frame for physics) > Update > LateUpdate
/// 
/// Set Edit > Project Settings > Script Execution Order: Globals (meaning fileManager + config) must load before Default Time, eg SteamManager.
/// </summary>
namespace MOR.Industries {
	public class Globals : Singleton<Globals> {
#if UNITY_EDITOR
		public const bool RUNNING_IN_EDITOR = true;
#else
        public const bool RUNNING_IN_EDITOR = false;
#endif
		public static bool applicationQuitting = false;

		public Camera mainCamera;


		public static bool preloadComplete = false;
		private bool _initialized = false;

		public static bool quitting = false;


		public float startTime = 0;

		public static bool initialized {
			get { return instance._initialized; }
		}

		public static UnityEvent OnUpdate = new UnityEvent();

		private Transform _playerHead;
		//private float lastCheckTime = 0;
		//private float checkFrequency = 1f;


		//public static Transform playerHead { get { return instance._playerHead; } }
		public static Transform playerHead => InstanceNull || instance.mainCamera == null ? null : instance.mainCamera.transform;

		public static MORPlayer player {
			get { return MORPlayer.instance; }
		}

		public Transform trackingOriginTransform {
			get { return transform; }
		}

		private float initialEyeResolution;

		private void Awake(){
			Shader.EnableKeyword("TBT_LINEAR_TARGET"); //TODO : Put this in a smarter place?? Needs setting in BUILD
		}

		private void Start(){
			// wait 1 second for physics objects to settle and VR to start
			//Util.RunLater(Initialize, 1);
			startTime = Time.time;
		}


		private void Initialize(){
			if (mainCamera == null || !mainCamera.isActiveAndEnabled) {
				Debug.LogError("Main camera is not setup or enabled. Fix!");
			}

			_initialized = true;
		}

		public override void OnDestroy(){
			base.OnDestroy();
		}

		void OnApplicationQuit(){
			Debug.Log("Exiting application");
			quitting = true;
		}
	}
}