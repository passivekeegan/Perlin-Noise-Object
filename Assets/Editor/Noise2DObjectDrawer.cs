using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Noise2DObject))]
public class Noise2DObjectDrawer : PropertyDrawer
{
	private const float IMAGE_SIZE = 80f;
	private const float LINE_HEIGHT = 16f;
	private const float PADDING = 4f;
	private const float VERTICAL_SPACE = 10f;
	private const float HORIZONTAL_SPACE = 4f;

	private int _layer_select = 0;
	private int _preview_mode = 0;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Noise2DObject noise_obj = fieldInfo.GetValue(property.serializedObject.targetObject) as Noise2DObject;
		
		EditorGUI.BeginProperty(position, label, property);
		float y = position.y + PADDING;
		float x = position.x;
		float width = position.width - PADDING;
		float height = position.height - (2 * PADDING);

		float img_size = IMAGE_SIZE;
		if (IMAGE_SIZE > (width - VERTICAL_SPACE) * 0.35f)
		{
			img_size = (width - VERTICAL_SPACE) * 0.35f;
		}
		float img_x = x + width - img_size;

		//right column
		Rect right_rect = new Rect(img_x, y, img_size, img_size);
		Texture2D preview_img = Texture2D.blackTexture;
		if (noise_obj != null)
		{
			preview_img = noise_obj.GenerateNoiseImage(128, 128);
		}
		EditorGUI.DrawPreviewTexture(right_rect, preview_img);

		//left column
		Rect left_rect = new Rect(x, y, width - img_size - VERTICAL_SPACE, LINE_HEIGHT);
		label.text = "Perlin Noise Object";
		EditorGUI.PrefixLabel(left_rect, GUIUtility.GetControlID(FocusType.Passive), label);
		left_rect.y += LINE_HEIGHT + HORIZONTAL_SPACE;
		EditorGUI.PropertyField(left_rect, property, GUIContent.none);
		left_rect.y += LINE_HEIGHT + HORIZONTAL_SPACE;

		float ll_width = 0.5f * left_rect.width;
		float lr_width = left_rect.width - ll_width;
		Rect ll_rect = new Rect(left_rect.x, left_rect.y, ll_width, left_rect.height);
		Rect lr_rect = new Rect(ll_rect.x + ll_width, ll_rect.y, lr_width, ll_rect.height);
		EditorGUI.LabelField(ll_rect, "View:");
		EditorGUI.Popup(lr_rect, 0, new string[] { "Total", "Layers" });

		if (noise_obj != null)
		{
			int[] value_options = new int[noise_obj.layers];
			string[] display_options = new string[noise_obj.layers];
			for (int k = 0;k < display_options.Length; k++)
			{
				display_options[k] = k.ToString();
				value_options[k] = k;
			}

			ll_rect.y += LINE_HEIGHT + HORIZONTAL_SPACE;
			lr_rect.y += LINE_HEIGHT + HORIZONTAL_SPACE;
			EditorGUI.LabelField(ll_rect, "Layer:");
			_layer_select = Mathf.Clamp(EditorGUI.IntPopup(lr_rect, _layer_select, display_options, value_options), 0, noise_obj.layers - 1);
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return IMAGE_SIZE + (2 * PADDING);
	}

	public override bool CanCacheInspectorGUI(SerializedProperty property)
	{
		return false;
	}
}
