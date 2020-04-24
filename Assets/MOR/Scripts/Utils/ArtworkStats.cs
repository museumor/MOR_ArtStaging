using UnityEngine;

/// <summary>
/// This whole script is editor only. Used to display info about an artwork
/// </summary>
namespace MOR.Industries {
	public class ArtworkStats : MonoBehaviour {
		[ReadOnly] public int numCountedVertices = -1;


		void CountMeshVertices() {
#if UNITY_EDITOR
			numCountedVertices = 0;
			MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter mesh in meshes) {
				if (mesh.gameObject.activeInHierarchy == false || mesh.gameObject.activeSelf == false || mesh.GetComponent<MeshRenderer>()?.enabled == false) {
					continue;
				}

				numCountedVertices += mesh.sharedMesh?.vertexCount ?? 0;
			}

			SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer mesh in skins) {
				if (mesh.gameObject.activeInHierarchy == false || mesh.gameObject.activeSelf == false) {
					continue;
				}

				numCountedVertices += mesh.sharedMesh?.vertexCount ?? 0;
			}
#endif
		}

		public void OnDrawGizmosSelected() {
			CountMeshVertices();
		}
	}
}