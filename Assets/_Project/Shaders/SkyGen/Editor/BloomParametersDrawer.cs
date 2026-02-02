using UnityEditor;
using UnityEngine;

public class BloomParametersDrawer : MaterialPropertyDrawer
{
	public void MoveToNextProperty(ref Rect position)
	{
		position.yMin += EditorGUIUtility.singleLineHeight;
		position.yMax += position.yMin + EditorGUIUtility.singleLineHeight;
		position.size = new Vector2(position.size.x, EditorGUIUtility.singleLineHeight);
	}

	public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	{
		float min = prop.vectorValue.y;
		float max = prop.vectorValue.x;
		float strength = prop.vectorValue.z;
		float shapeModulation = prop.vectorValue.w;

		float padding = 3.0f;
		Color transparentColor = EditorGUIUtility.isProSkin ? (Color)new Color32(56, 56, 56, 255) : (Color)new Color32(194, 194, 194, 255);
		EditorGUI.DrawRect(new Rect(position.min - Vector2.one * padding, position.size + Vector2.one * padding * 2.0f), transparentColor * 0.5f);
		EditorGUI.DrawRect(new Rect(position.min - Vector2.one * (padding - 1.0f), position.size + Vector2.one * (padding - 1.0f) * 2.0f), transparentColor * 1.2f);
		EditorGUI.LabelField(position, label);
		position.xMin += 40.0f;

		EditorGUI.BeginChangeCheck();

		MoveToNextProperty(ref position);
		EditorGUI.MinMaxSlider(position, "Min and max radius", ref min, ref max, -2.0f, 30.0f);
		MoveToNextProperty(ref position);
		strength = EditorGUI.Slider(position, "Strength", strength, -0.4f, 1.4f);
		MoveToNextProperty(ref position);
		shapeModulation = EditorGUI.Slider(position, "Shape mod", shapeModulation, 0.1f, 20.0f);

		if (EditorGUI.EndChangeCheck())
		{
			prop.vectorValue = new Vector4(max, min, strength, shapeModulation);
		}
	}

	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	{
		return EditorGUIUtility.singleLineHeight * 4;
	}
}
