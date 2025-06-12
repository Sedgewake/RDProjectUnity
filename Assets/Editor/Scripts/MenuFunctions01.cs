using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MenuFunctions01 : ScriptableObject 
{
	[MenuItem("Assets/Duplicate")]
	public static void Duplicate ()
	{
		if (Selection.activeObject != null)
		{
			string asset_path = AssetDatabase.GetAssetPath(Selection.activeObject);
			string directory = Path.GetDirectoryName(asset_path);
			string asset_name = Path.GetFileNameWithoutExtension(asset_path);
			string asset_name_full = Path.GetFileName(asset_path);
			string extension = asset_name_full.Replace(asset_name, "");
			int number = 1;
			string new_path = directory+"/"+asset_name+"_d"+number.ToString()+extension;
			while (File.Exists(new_path))
			{
				number++;
				new_path = directory+"/"+asset_name+"_d"+number.ToString()+extension;
				System.Threading.Thread.Sleep(50);
			}
			AssetDatabase.CopyAsset(asset_path, new_path);
			asset_path = null;
			directory = null;
			asset_name = null;
			asset_name_full = null;
			extension = null;
			new_path = null;
		}
	}
}