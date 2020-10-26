using MOR.Industries;
using UnityEngine;

//[ExecuteInEditMode]
public class GlobalShaderVariables : MonoBehaviour {
	private static readonly int ServerTimeProp = Shader.PropertyToID("_ServerTime");

	private void Awake() {
		//MOREventManager.Listen(MOREvents.SERVER_CONNECT, SetServerTime);
		SetServerTime();
	}

	private void OnDisable() {
		//MOREventManager.Remove(MOREvents.SERVER_CONNECT,SetServerTime);
	}
	private const double WRAP_POINT = (9*60*60)/3;//~read 9 hours is the limit for floats, so wrap over one third of that as we * 3 , so (9*60*60)/3
	/*private void Update() {
		Vector4 times;
		serverTimeOffset += Time.deltaTime;
		serverTimeOffset = serverTimeOffset % WRAP_POINT;
		float time = (float)(serverTimeOffset);
		times.y = time;
		sentTime = times.y;
		times.x = (float)(time / 20.0);
		times.z = (float)((time * 2.0) % float.MaxValue);
		times.w = (float)((time * 3.0) % float.MaxValue);
		Shader.SetGlobalVector(ServerTimeProp, times);
	}*/
	private double serverTimeOffset = 0;
	private float sentTime;
	private void SetServerTime(params object[] args) {
		double setTime = Time.timeSinceLevelLoad;
		if(Application.isPlaying) {
			setTime -= MORRealtime.serverTime%WRAP_POINT;
		}
		serverTimeOffset = setTime;
		Shader.SetGlobalFloat(ServerTimeProp, (float) serverTimeOffset);
	}
}