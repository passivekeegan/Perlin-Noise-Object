using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Noise2DObject))]
public class Noise2DObjectEditor : Editor
{
	private bool _lacunarity_lock;
	private bool _scale_lock;
	private int _preview_mode;
	private int _layer_index;
	private Noise2DObject _obj;
	private Vector2Int _png_dim;
	private Texture2D _preview_total;
	private Texture2D _preview_layer;

	private GUIStyle _preview_style;
	private GUIStyle _layerinf_style;

	private void OnEnable()
	{
		_obj = (Noise2DObject)target;

		//editor controls logic
		_scale_lock = true;
		_lacunarity_lock = true;
		_layer_index = 0;
		_preview_mode = 0;
		_png_dim = new Vector2Int(256, 256);

		//preview textures
		_preview_total = _obj.GenerateNoiseImage(256, 256);
		_preview_layer = _obj.GenerateNoiseImage(_layer_index, 256, 256);
	}

	public override void OnInspectorGUI()
	{
		//bool indicating if preview image should be updated at end
		bool update_previews = false;
		//case target as obj variable
		_obj = (Noise2DObject)target;

		//create styling for elements
		InitializeStyle();

		//draws the layer preview images
		DrawImagePreviews();

		//panel with options for selecting and modifying layers
		update_previews |= DrawLayerControlPanel();

		//panel with options for the noise in the layer
		update_previews |= DrawLayerSettingsPanel();

		//panel with options to generate and save images created
		DrawImageGenerationPanel();

		//update preview image and object
		if (update_previews)
		{
			UpdatePreviewTextures();
			EditorUtility.SetDirty(_obj);
		}
	}

	private void InitializeStyle()
	{
		_preview_style = new GUIStyle();
		_preview_style.alignment = TextAnchor.UpperCenter;
		_preview_style.padding = new RectOffset(0, 0, 0, 0);
		_preview_style.margin = new RectOffset(0, 0, 0, 0);

		_layerinf_style = new GUIStyle(EditorStyles.miniButton);
		_layerinf_style.alignment = TextAnchor.MiddleCenter;
		_layerinf_style.padding = new RectOffset(2, 0, 0, 2);
	}

	private void DrawImagePreviews()
	{
		//Tool bar to select image to preview
		EditorGUI.BeginChangeCheck();
		_preview_mode = GUILayout.Toolbar(_preview_mode, new string[] { "Total Preview", "Layer Preview" }, GUILayout.MinWidth(100));
		if (EditorGUI.EndChangeCheck())
		{
			UpdatePreviewTextures();
		}
		switch (_preview_mode)
		{
			case 1:
				//draw preview image of the layer
				GUILayout.Label(_preview_layer, _preview_style, GUILayout.MaxHeight(256), GUILayout.MinWidth(100));
				break;
			default:
				//draw preview image of the entire noise collection
				GUILayout.Label(_preview_total, _preview_style, GUILayout.MaxHeight(256), GUILayout.MinWidth(200));
				break;
		}
	}

	private bool DrawLayerControlPanel()
	{
		bool update_previews = false;
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("Layers", EditorStyles.largeLabel, GUILayout.Height(20));
		EditorGUILayout.Space();

		//Select layer to change settings
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.LabelField("Layer Select:");
		_layer_index = GUILayout.SelectionGrid(_layer_index, SelectLayerOptions(_obj.layers), 8, GUILayout.MinWidth(100));
		GUILayout.Space(4);
		if (EditorGUI.EndChangeCheck())
		{
			update_previews = true;
		}

		//layer influence section for altering layers contribution to total image
		EditorGUILayout.BeginHorizontal();
		int prev_influence = _obj.GetInfluence(_layer_index);
		int influence = EditorGUILayout.IntField("Layer Influence:", prev_influence, GUILayout.MinWidth(20));
		if (GUILayout.Button("+", _layerinf_style, GUILayout.Width(22), GUILayout.Height(16)))
		{
			influence += 1;
		}
		if (GUILayout.Button("-", _layerinf_style, GUILayout.Width(20), GUILayout.Height(16)))
		{
			influence = Mathf.Max(0, influence - 1);
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4);
		if (prev_influence != influence)
		{
			Undo.RecordObject(_obj, "Update influence of layer " + _layer_index + ".");
			_obj.SetInfluence(_layer_index, influence);
			update_previews = true;
		}

		//add/delete/reset layer section
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Reset", GUILayout.Width(60)))
		{
			Undo.RecordObject(_obj, "Reset noise layer " + _layer_index + ".");
			_obj.ResetLayer(_layer_index);
			update_previews = true;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Add", GUILayout.Width(60)))
		{
			Undo.RecordObject(_obj, "Add noise layer.");
			_obj.Addlayer();
			_layer_index = _obj.layers - 1;
			update_previews = true;
		}
		if (GUILayout.Button("Delete", GUILayout.Width(60)))
		{
			Undo.RecordObject(_obj, "Delete noise layer " + _layer_index + ".");
			_obj.Deletelayer(_layer_index);
			_layer_index = Mathf.Max(0, _layer_index - 1);
			update_previews = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		return update_previews;
	}

	private bool DrawLayerSettingsPanel()
	{
		bool update_previews = false;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		EditorGUILayout.LabelField("Layer Settings", EditorStyles.largeLabel, GUILayout.Height(20));
		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		int octaves = EditorGUILayout.IntSlider("Number Of Octaves:", _obj.GetOctaves(_layer_index), 1, 8);
		GUILayout.Space(4);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(_obj, "Update number of octaves for layer " + _layer_index + ".");
			_obj.SetOctaves(_layer_index, octaves);
			update_previews = true;
		}

		EditorGUI.BeginChangeCheck();
		float persistence = EditorGUILayout.Slider("Persistence:", _obj.GetPersistence(_layer_index), 0f, 1f);
		GUILayout.Space(4);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(_obj, "Update persistence value for layer " + _layer_index + ".");
			_obj.SetPersistence(_layer_index, persistence);
			update_previews = true;
		}

		EditorGUI.BeginChangeCheck();
		Vector2 offset = EditorGUILayout.Vector2Field("Offset:", _obj.GetOffset(_layer_index));
		GUILayout.Space(4);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(_obj, "Update offset values for layer " + _layer_index + ".");
			_obj.SetOffset(_layer_index, offset);
			update_previews = true;
		}

		EditorGUI.BeginChangeCheck();
		Vector2 prev_vector = _obj.GetLacunarity(_layer_index);
		Vector2 next_vector = EditorGUILayout.Vector2Field("Lacunarity:", prev_vector);
		_lacunarity_lock = EditorGUILayout.ToggleLeft("lock lacunarity changes", _lacunarity_lock);
		GUILayout.Space(4);
		if (EditorGUI.EndChangeCheck())
		{
			if (_lacunarity_lock)
			{
				next_vector = ApplyLockChanges(prev_vector, next_vector);
			}
			Undo.RecordObject(_obj, "Update lacunarity values for layer " + _layer_index + ".");
			_obj.SetLacunarity(_layer_index, next_vector);
			update_previews = true;
		}

		EditorGUI.BeginChangeCheck();
		prev_vector = _obj.GetScale(_layer_index);
		next_vector = EditorGUILayout.Vector2Field("Scale:", prev_vector);
		_scale_lock = EditorGUILayout.ToggleLeft("lock scale changes", _scale_lock);
		if (EditorGUI.EndChangeCheck() && next_vector.x != 0 && next_vector.y != 0)
		{
			if (_scale_lock)
			{
				next_vector = ApplyLockChanges(prev_vector, next_vector);
			}
			Undo.RecordObject(_obj, "Update scale values for layer " + _layer_index + ".");
			_obj.SetScale(_layer_index, next_vector);
			update_previews = true;
		}

		EditorGUILayout.EndVertical();
		return update_previews;
	}

	private void DrawImageGenerationPanel()
	{
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("Image Generation", EditorStyles.largeLabel, GUILayout.Height(20));
		EditorGUILayout.Space();

		_png_dim = FixImageDimension(EditorGUILayout.Vector2IntField("Dimensions:", _png_dim));
		GUILayout.Space(4);

		if (GUILayout.Button("Save current preview image as PNG"))
		{
			if (_preview_mode != 0)
			{
				SaveImageAsPNG(_obj.GenerateNoiseImage(_layer_index, _png_dim.x, _png_dim.y), "noise_layer_" + _layer_index);
			}
			else
			{
				SaveImageAsPNG(_obj.GenerateNoiseImage(_png_dim.x, _png_dim.y), "noise_total");
			}
		}
		EditorGUILayout.EndVertical();
	}

	private void SaveImageAsPNG(Texture2D tex, string name)
	{
		string path = EditorUtility.SaveFilePanel("Save Noise Texture as PNG", "", name + ".png", "png");
		if (path.Length != 0)
		{
			byte[] img_bytes = tex.EncodeToPNG();
			if (img_bytes != null && img_bytes.Length > 0)
			{
				System.IO.File.WriteAllBytes(path, img_bytes);
			}
		}
		
	}


	private void UpdatePreviewTextures()
	{
		switch(_preview_mode)
		{
			case 1:
				//generate image of selected layer
				_preview_layer = _obj.GenerateNoiseImage(_layer_index, 256, 256);
				break;
			default:
				//generate image from all layers
				_preview_total = _obj.GenerateNoiseImage(256, 256);
				break;
		}
	}

	
	private Vector2 ApplyLockChanges(Vector2 prev_vector, Vector2 next_vector)
	{
		if (next_vector.x != prev_vector.x)
		{
			next_vector.y = next_vector.x;
		}
		else if (next_vector.y != prev_vector.y)
		{
			next_vector.x = next_vector.y;
		}
		return next_vector;
	}

	private string[] SelectLayerOptions(int count)
	{
		string[] options = new string[count];
		for (int k = 0;k < count;k++)
		{
			options[k] = k.ToString();
		}
		return options;
	}

	private Vector2Int FixImageDimension(Vector2Int dim)
	{
		int x = 4;
		int y = 4;
		for (int k = 32;k < 4096;k *= 2)
		{
			if (k <= dim.x)
			{
				x = k;
			}
			if (k <= dim.y)
			{
				y = k;
			}
		}
		dim.x = x;
		dim.y = y;
		return dim;
	}
}
