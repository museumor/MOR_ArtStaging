using System.Collections.Generic;
using UnityEngine.XR;

namespace MOR.Industries {
	public class MultiPlatform : Singleton<MultiPlatform> {
		public enum VrApis {
			Unknown,
			SteamVR,
			Oculus,
			Viveport
		}

		public enum ControllerTypes {
			Unknown,
			ViveWand,
			OculusTouch,
			OculusTouchSteamVr,
			Knuckles,
			Cosmos,
			RiftS,
			RiftSSteamVr,
			Quest,
			UnknownOculus
		}

		public enum RoomSize {
			Unknown,
			Standing,
			RoomScale
		}


		public bool IsInitialized { get; private set; } = false;
		public VrApis VrApi { get; private set; } = VrApis.Unknown;

		// Assume we have focus on startup.  It would be nicer if we could detect this.
		bool hasFocus = true;

		public bool HasFocus {
			get { return hasFocus; }
		}

		public RoomSize roomSize {
			get {
				if (XRDevice.GetTrackingSpaceType() == TrackingSpaceType.RoomScale) {
					return RoomSize.RoomScale;
				}
				else {
					return RoomSize.Standing;
				}
			}
		}

		private static List<XRNodeState> _nodeStates = new List<XRNodeState>();

		private bool HeadDetected(){
			InputTracking.GetNodeStates(_nodeStates);

			foreach (XRNodeState nodeState in _nodeStates) {
				if (nodeState.nodeType == XRNode.Head) {
					return true;
				}
			}
			return false;
		}
	}
}