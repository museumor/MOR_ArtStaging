using System.IO;
using UnityEditor;
using UnityEngine;

namespace MOR.Museum {
	public class ExportArtworkPackage : MonoBehaviour {



		public static void CountProjectVertices(){
			GameObject g = GameObject.Find("YOUR_ART_HERE");
			Validator.CheckVertexCount(g);
		}

		[MenuItem("MOR/Export Package %#e")]
		public static void ExportPackage(){

			//ResourceValidation.

			ExportPackageOptions exportPackageOptions = ExportPackageOptions.Recurse;
			string exportFilename = "testname.unitypackage"; //TODO : set up somewhere to set this... prefab name? folder name? hmm....
			string projectPath = Directory.GetCurrentDirectory();
			exportFilename = Path.Combine(projectPath, exportFilename);
			AssetDatabase.ExportPackage("Assets/YOUR_ASSETS_HERE", exportFilename, exportPackageOptions);
			EditorUtility.RevealInFinder(exportFilename);
		}
	}
}