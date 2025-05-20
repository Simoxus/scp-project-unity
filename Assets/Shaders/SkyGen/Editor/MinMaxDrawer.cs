using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MinMaxDrawer : MaterialPropertyDrawer
{
	protected readonly float minimumValue;
	protected readonly float maximumValue;

	public MinMaxDrawer(float minimumValue, float maximumValue)
	{
		this.minimumValue = minimumValue;
		this.maximumValue = maximumValue;
	}

	public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		float min = prop.vectorValue.x;
		float max = prop.vectorValue.y;

		EditorGUI.BeginChangeCheck();

		EditorGUI.MinMaxSlider(position, label, ref min, ref max, minimumValue, maximumValue);

		if (EditorGUI.EndChangeCheck())
		{
			if (min < 0.0f) min = 0.0f;
			if (max < 0.0f) max = 0.0f;

			if (min > max)
			{
				float avg = (min + max) * 0.5f;
				min = max = avg;
			}

			prop.vectorValue = new Vector4(min, max, 0.0f, 0.0f);
		}
	}
}
