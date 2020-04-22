using System;
using System.IO;
using Boo.Lang;
using UnityEditor;
using UnityEngine;

public class ResourceValidation : AssetPostprocessor {
	private string[] nonFBXfiletypes = {"obj", "blend", "mb", "ma","max","c4d",".lxo"};

	private bool CheckExtension(string extension){
		return Array.IndexOf(nonFBXfiletypes, extension.ToLower()) >= 0 ;
	}
	
	void OnPreprocessModel(){
		string[] fileSplit = assetPath.Split('.');
		string fileExtension = fileSplit[fileSplit.Length - 1];
		if (CheckExtension(fileExtension)) {
			Debug.LogError($"MOR - INVALID FILETYPE : please export all models as .fbx : {assetPath}");
		} else if (String.CompareOrdinal("fbx", fileExtension.ToLower()) != 0 ) { //wait, what filetype is this?
			Debug.LogError($"MOR - INVALID FILETYPE : please export all models as .fbx : {assetPath}");
		} else if (!IsBinaryFbx(assetPath)) {
			Debug.LogWarning($"MOR - Ascii .fbx detected. Export as Binary .fbx is preferred. {assetPath}");
		}
	}

	private void OnPostprocessModel(GameObject g){
		var renderers = g.GetComponentsInChildren<Renderer>(true);
		List<string> errors = new List<string>();
		foreach (Renderer renderer in renderers) {
			if (renderer.sharedMaterials.Length > 1) {
				Debug.LogError($"MOR - Multi-Material Renderer : Please have 1 material per object. {renderer.gameObject.name}");		
			}
		}
		if(errors.Count > 0) {
			EditorUtility.DisplayDialog("MOR IMPORT WARNING", $"Check Console window for Errors related to asset validation.", "Gotcha");
		}
		
	}


	//Copied from Tiltbrush 'FBX Utils'
	/// Returns true if the file might be a binary-format FBX
	static bool IsBinaryFbx(string path) {
		try {
			using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
			using (var reader = new BinaryReader(file)) {
				return ReadHeader(reader);
			}
		} catch (Exception) {
			return false;
		}
	}  
	/// Returns true if the header was read properly and looks like a binary fbx
	static bool ReadHeader(BinaryReader reader) {
		string firstTwenty = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(20));
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
	/// https://stackoverflow.com/questions/37111511/getting-file-size-in-kb
	/// </summary>
	static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
	static string SizeSuffix(Int64 value)
	{
		if (value < 0) { return "-" + SizeSuffix(-value); }
		int i = 0;
		decimal dValue = (decimal)value;
		while (Math.Round(dValue / 1024) >= 1)
		{
			dValue /= 1024;
			i++;
		}

		return $"{dValue:n1} {SizeSuffixes[i]}";
	}	
	
	
	
	
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths){
		string rootPath = Directory.GetCurrentDirectory();
		foreach (var asset in importedAssets) {
			string assetPath = Path.Combine(rootPath, asset);
			// get the file attributes for file or directory
			FileAttributes attr = File.GetAttributes(assetPath);
			if (attr.HasFlag(FileAttributes.Directory)) {
				continue; //Don't process directores
			}

			FileInfo FileVol = new FileInfo(assetPath);
			Int64 sizeInBytes = (Int64) (FileVol).Length;
			if (sizeInBytes >= 100 * 1024 * 1024) { //greater than 100MB
				//Error
				Debug.LogError($"MOR - File Size Over 100MB. {SizeSuffix(sizeInBytes)}  :  {asset}");			
			}else if (sizeInBytes > 50 * 1024 * 1024) {
				//Warning
				Debug.LogWarning($"MOR - File Size quite large. {SizeSuffix(sizeInBytes)} : {asset}");
			}

		}
	}
}