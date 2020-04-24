using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MOR.Museum {


	public class ResourceValidation : AssetPostprocessor {

		void OnPreprocessModel(){
			string errors = "";
			errors += Validator.Validate_Model_Filetype_Error(assetPath);
			string warnings = "";
			warnings += Validator.Validate_Model_Filetype_Warning(assetPath);
			Validator.DisplayResults(errors, warnings);
		}

		private void OnPostprocessModel(GameObject g){
			string[] results = Validator.CheckVertexCount(g);
			Validator.DisplayResults(results[0],results[1]);
		}
		
		
		public static void CountProjectVertices(){
			GameObject g = GameObject.Find("YOUR_ART_HERE");
			Validator.CheckVertexCount(g);
		}



		/// <summary>
		/// https://stackoverflow.com/questions/37111511/getting-file-size-in-kb
		/// </summary>
		static readonly string[] SizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
		static string SizeSuffix(Int64 value){
			if (value < 0) {
				return "-" + SizeSuffix(-value);
			}

			int i = 0;
			decimal dValue = (decimal) value;
			while (Math.Round(dValue / 1024) >= 1) {
				dValue /= 1024;
				i++;
			}
			return $"{dValue:n1} {SizeSuffixes[i]}";
		}

		private void OnPreprocessAsset(){
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
				}
				else if (sizeInBytes > 50 * 1024 * 1024) {
					//Warning
					Debug.LogWarning($"MOR - File Size quite large. {SizeSuffix(sizeInBytes)} : {asset}");
				}

			}
		}
	}
}