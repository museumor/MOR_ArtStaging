using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MOR.Museum {
	public class Validator : MonoBehaviour {
		/// <summary>
		/// Handles output and display of errors and warnings resulting from validation.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="warnings"></param>
		public static void DisplayResults(string errors, string warnings){
			if (string.IsNullOrEmpty(warnings) == false) {
				var list = warnings.Split('\n');
				foreach (string err in list) {
					Debug.LogWarning($"MOR - {err}");
				}
			}

			if (string.IsNullOrEmpty(errors) == false) {
				var list = errors.Split('\n');
				foreach (string err in list) {
					Debug.LogError($"MOR - {err}");
				}

				EditorUtility.DisplayDialog("MOR IMPORT WARNING", $"Check Console window for Errors related to asset validation.", "Gotcha");
			}
		}


		private static readonly string[] nonFBXfiletypes = {"blend", "mb", "ma", "max", "c4d", ".lxo"};

		private static int VERT_COUNT_MAX_PC = 1_000_000;
		//private static int VERT_COUNT_MAX_MOBILE = 80_000;

		public static string[] CheckVertexCount(GameObject g, string assetPath = ""){
			var renderers = g.GetComponentsInChildren<Renderer>(true);
			int vertexCountTotal = 0;
			string errors = "";
			string warnings = "";
			foreach (Renderer renderer in renderers) {
				int vertexCount = 0;
				if (renderer is SkinnedMeshRenderer) {
					vertexCount = ((SkinnedMeshRenderer) renderer).sharedMesh.vertexCount;
				}
				else {
					MeshFilter mFilter = renderer.GetComponent<MeshFilter>();
					Mesh mesh = mFilter.sharedMesh;
					vertexCount = mesh.vertexCount;
				}

				vertexCountTotal += vertexCount;
				if (vertexCount > UInt16.MaxValue) {
					warnings += ($"MOR - Mesh has too many verts for 16bit indexing `{vertexCount}` - Consider splitting into multiple parts : model = {assetPath}  part = {g.name}");
				}

				if (renderer.sharedMaterials.Length > 1) {
					errors += ($" Multi-Material Renderer : Please have 1 material per object. {renderer.gameObject.name}\n");
				}
			}

			if (vertexCountTotal > VERT_COUNT_MAX_PC) {
				errors += ($"Model likely has too many vertices, please simplify the model : model - {assetPath}  size = {vertexCountTotal} vertices");
			}

			return new[] {errors, warnings};
		}

		/// <summary>
		/// Verify filetype by extension to avoid use of 3d editor files as resources, such as direct blender, max or maya files.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public static bool CheckExtension(string extension){
			return Array.IndexOf(nonFBXfiletypes, extension.ToLower()) >= 0;
		}

		//Copied from Tiltbrush 'FBX Utils'
		/// Returns true if the file might be a binary-format FBX
		static bool IsBinaryFbx(string path){
			try {
				using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
				using (var reader = new BinaryReader(file)) {
					return ReadHeader(reader);
				}
			}
			catch (Exception) {
				return false;
			}
		}

		/// Returns true if the header was read properly and looks like a binary fbx
		static bool ReadHeader(BinaryReader reader){
			string firstTwenty = Encoding.ASCII.GetString(reader.ReadBytes(20));
			if ((firstTwenty != "Kaydara FBX Binary  ")
			    || (reader.ReadByte() != 0x00)
			    || (reader.ReadByte() != 0x1a)
			    || (reader.ReadByte() != 0x00)) {
				return false;
			}

			reader.ReadUInt32(); // Version - unneeded
			return true;
		}

		/// <summary>
		/// Checks for errors related to model filetypes
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public static string Validate_Model_Filetype_Error(string assetPath){
			string[] fileSplit = assetPath.Split('.');
			string fileExtension = fileSplit[fileSplit.Length - 1];
			string result = "";
			if (CheckExtension(fileExtension)) {
				result = ($"INVALID FILETYPE : please export all models as .fbx : {assetPath}/n");
			}
			else if (String.CompareOrdinal("fbx", fileExtension.ToLower()) != 0) { //wait, what filetype is this?
				result = ($"INVALID FILETYPE : please export all models as .fbx : {assetPath}/n");
			}

			return result;
		}

		/// <summary>
		/// Checks for warnings related to model filetypes
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public static string Validate_Model_Filetype_Warning(string assetPath){
			string[] fileSplit = assetPath.Split('.');
			string result = "";
			if (!IsBinaryFbx(assetPath)) {
				result = ($"MOR - Ascii .fbx detected. Export as Binary .fbx is preferred. {assetPath}/n");
			}

			return result;
		}
	}
}