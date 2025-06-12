using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class NamedFolders : ScriptableWizard
{
	public bool Textures = false;
	public bool Materials = false;
	public bool Meshes = false;
	public bool Scripts = false;
	public bool Prefabs = false;
	public bool Sound = false;
	public bool Shaders = false;
	public bool Models = false;
	public string OutputPath;
	private static string current_path = "Assets";

	[MenuItem("Assets/Create/Named Folders")]	
	static void CreateWizard ()
	{
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			current_path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(current_path))
			{
				current_path = Path.GetDirectoryName(current_path);
			}
			break;
		}
		ScriptableWizard.DisplayWizard<NamedFolders>("Create Folders", "Create");
	}
	
	void OnWizardUpdate ()
	{
		OutputPath = current_path;
	}

	void OnWizardCreate ()
	{
		if (Textures)
		{
			AssetDatabase.CreateFolder(OutputPath, "Textures");
		}
		if (Materials)
		{
			AssetDatabase.CreateFolder(OutputPath, "Materials");
		}
		if (Meshes)
		{
			AssetDatabase.CreateFolder(OutputPath, "Meshes");
		}
		if (Scripts)
		{
			AssetDatabase.CreateFolder(OutputPath, "Scripts");
		}
		if (Prefabs)
		{
			AssetDatabase.CreateFolder(OutputPath, "Prefabs");
		}
		if (Sound)
		{
			AssetDatabase.CreateFolder(OutputPath, "Sound");
		}
		if (Shaders)
		{
			AssetDatabase.CreateFolder(OutputPath, "Shaders");
		}
		if (Models)
		{
			AssetDatabase.CreateFolder(OutputPath, "Models");
		}
	}
}
