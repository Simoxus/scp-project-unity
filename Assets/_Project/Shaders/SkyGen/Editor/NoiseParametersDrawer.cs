using UnityEditor;
using UnityEngine;

public class NoiseParametersDrawer : MaterialPropertyDrawer
{
	struct Range
	{
		public float min;
		public float max;
		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}

	//x - scale, y - iterations, z - alpha mod, w - size mod
	private static readonly Range scaleRange = new Range(0.001f, 3.0f);
	private static readonly Range iterationsRange = new Range(1.0f, 10.0f);
	private static readonly Range alphaModRange = new Range(0.1f, 1.0f);
	private static readonly Range sizeModRange = new Range(0.5f, 4.0f);

	public void MoveToNextProperty(ref Rect position)
	{
		position.yMin += EditorGUIUtility.singleLineHeight;
		position.yMax += position.yMin + EditorGUIUtility.singleLineHeight;
		position.size = new Vector2(position.size.x, EditorGUIUtility.singleLineHeight);
	}

	public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		float scale = prop.vectorValue.x;
		int iterations = (int)prop.vectorValue.y;
		float alphaMod = prop.vectorValue.z;
		float sizeMod = prop.vectorValue.w;

		float padding = 3.0f;
		Color transparentColor = EditorGUIUtility.isProSkin ? (Color)new Color32(56, 56, 56, 255) : (Color)new Color32(194, 194, 194, 255);
		EditorGUI.DrawRect(new Rect(position.min - Vector2.one * padding, position.size + Vector2.one * padding * 2.0f), transparentColor * 0.5f);
		EditorGUI.DrawRect(new Rect(position.min - Vector2.one * (padding - 1.0f), position.size + Vector2.one * (padding - 1.0f) * 2.0f), transparentColor * 1.2f);
		EditorGUI.LabelField(position, prop.displayName);

		EditorGUI.BeginChangeCheck();
		MoveToNextProperty(ref position);

		position.xMin += 40;
		scale = EditorGUI.Slider(position, "Rescale", scale, scaleRange.min, scaleRange.max);
		MoveToNextProperty(ref position);
		iterations = EditorGUI.IntSlider(position, "Iterations", iterations, (int)iterationsRange.min, (int)iterationsRange.max);
		MoveToNextProperty(ref position);
		alphaMod = EditorGUI.Slider(position, "Transparency mod", alphaMod, alphaModRange.min, alphaModRange.max);
		MoveToNextProperty(ref position);
		sizeMod = EditorGUI.Slider(position, "Layer size mod", sizeMod, sizeModRange.min, sizeModRange.max);

		if (EditorGUI.EndChangeCheck())
		{
			prop.vectorValue = new Vector4(scale, iterations, alphaMod, sizeMod);
		}
	}

	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	{
		return EditorGUIUtility.singleLineHeight * 5;
	}
}
