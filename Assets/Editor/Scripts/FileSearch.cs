// v1.0 With multtiple extensions
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class FileSearch : EditorWindow
{
	private string inputFolder;
	private string extensions = "txt;cs;js;cginc;shader";
	private string[] ext_split;
	private string keyword;
	private bool recursive = false;
	private bool ignoreCase = false;
	
	[MenuItem("Tools/Utility/File Search")]
	public static void Init()
	{
		FileSearch window = GetWindow<FileSearch>();
		window.titleContent = new GUIContent("File Search");
	}
	
	void OnGUI ()
	{
		EditorGUILayout.LabelField("Folder");
		inputFolder = EditorGUILayout.TextField(inputFolder);
		if (GUILayout.Button("Browse", GUILayout.Width(70)))
		{
			inputFolder = EditorUtility.OpenFolderPanel("Select folder", "", "");
		}
		extensions = EditorGUILayout.TextField("Extensions", extensions);
		keyword = EditorGUILayout.TextField("Keyword", keyword);
		recursive = EditorGUILayout.Toggle("Recursive", recursive);
		ignoreCase = EditorGUILayout.Toggle("Ignore Case", ignoreCase);
		if (GUILayout.Button("Search", GUILayout.Width(120), GUILayout.Height(35)))
		{
			if (!string.IsNullOrEmpty(inputFolder))
			{
				if (extensions.Contains(" "))
				{
					extensions = extensions.Replace(" ", "");
				}
				ext_split = extensions.Split(';');				
				ProcessFolder(inputFolder);
			}
		}
	}
	
	void ProcessFolder (string folder_path)
	{
		string[] files = Directory.GetFiles(folder_path);
		for (int i = 0; i < files.Length; i++)
		{
			bool mark = false;
			for (int i5 = 0; i5 < ext_split.Length; i5++)
			{
				if (files[i].Contains("."+ext_split[i5]))
				{
					mark = true;
					break;
				}
			}
			if (mark)
			{
				string[] file_lines = File.ReadAllLines(files[i]);
				for (int i2 = 0; i2 < file_lines.Length; i2++)
				{
					if (ignoreCase)
					{
						if (file_lines[i2].ToLower().Contains(keyword.ToLower()))
						{
							Debug.Log(files[i]+" <=> "+"line:"+(i2 + 1).ToString()+" <=> "+file_lines[i2]);
						}
					}
					else
					{
						if (file_lines[i2].Contains(keyword))
						{
							Debug.Log(files[i]+" <=> "+"line:"+(i2 + 1).ToString()+" <=> "+file_lines[i2]);
						}						
					}
				}
			}
		}
		if (recursive)
		{
			string[] folders = Directory.GetDirectories(folder_path);
			for (int i3 = 0; i3 < folders.Length; i3++)
			{
				ProcessFolder(folders[i3]);
			}
		}
	}
}