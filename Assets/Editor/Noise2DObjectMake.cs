using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Noise2DObjectMake
{
	[MenuItem("Assets/Create/Noise2DObject")]
	public static void CreateNoise2DObject()
	{
		Noise2DObject noise_obj = ScriptableObject.CreateInstance<Noise2DObject>();
		noise_obj.ResetObject();

		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.Length == 0)
		{
			path = "Assets";
		}
		else if (System.IO.Path.GetExtension(path).Length > 0)
		{
			path = path.Replace("/" + System.IO.Path.GetFileName(path), "");
		}

		path = AssetDatabase.GenerateUniqueAssetPath(path + "/noise_obj.asset");
		AssetDatabase.CreateAsset(noise_obj, path);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = noise_obj;
	}
}

