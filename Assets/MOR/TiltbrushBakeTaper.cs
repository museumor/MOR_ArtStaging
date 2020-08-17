using System.Collections.Generic;
using EasyButtons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class TiltbrushBakeTaper : MonoBehaviour {
	private string[] taperedMaterials = {"DoubleTaperedFlat", "DoubleTaperedMarker"};
	[Button]
	public void BakeDoublesided(){
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in meshRenderers) {
			GameObject obj = meshRenderer.gameObject;
			
			Material mat = meshRenderer.sharedMaterial;
			if (mat.name != "DoubleTaperedMarker" && mat.name != "DoubleTaperedFlat") {
				continue;
			}

			MeshFilter filter = obj.GetComponent<MeshFilter>();
			Mesh mesh = filter.sharedMesh;

			List<Vector3> vertex = new List<Vector3>();
			mesh.GetVertices(vertex);
			List<Vector2> texcoord0 = new List<Vector2>();
			mesh.GetUVs(0, texcoord0);
			List<Vector3> texcoord1 = new List<Vector3>();
			mesh.GetUVs(1, texcoord1);
			for (int i = 0; i < vertex.Count; i++) {
				float envelope = Mathf.Sin(texcoord0[i].x * Mathf.PI);
				float widthMultiplier = 1 - envelope;
				vertex[i] += -texcoord1[i] * widthMultiplier;
			}

			mesh.SetVertices(vertex);
			Debug.Log($"Set Mesh : {obj.name}");
		}
	}
}