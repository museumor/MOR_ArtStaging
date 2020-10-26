using UnityEngine;

namespace MOR.Industries {
	public class MORRealtime : Singleton<MORRealtime>{
		public static double serverTime
		{
		get
		{
			//if ((!Globals.applicationQuitting) && (MORRealtime.instance.realtimeFound) && (MORRealtime.instance.realtime.connected)) {
				
				//return MORRealtime.instance.realtime.room.time;
			//}
			//else {
				// ???? - I kind of want this error in here, but we've got some art pieces that are trying to use this before we've connected, so we probably need to fix that first
				//Util.LogWarning("MORRealtime: Trying to get time from a non-existant room");
				return Time.realtimeSinceStartup;
			//}
		}
	}
}

}