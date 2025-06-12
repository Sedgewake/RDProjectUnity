using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OpenReference : ScriptableObject
{
	[MenuItem("Help/Reference")]
	public static void OpenScriptingReference ()
	{
		Application.OpenURL("E:/UnityDocumentation/Documentation/en/ScriptReference/30_search.html");
	}
}
