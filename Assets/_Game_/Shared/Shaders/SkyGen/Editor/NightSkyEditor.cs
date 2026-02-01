using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NightSkyEditor : ShaderGUI
{
	int pointer = 0;
	MaterialProperty[] properties;

	MaterialProperty ActualProperty => properties[pointer];
	void NextMaterialProperty() => pointer++;
	bool HasNextProperty => pointer < properties.Length - 1;

	void SkipToProperty(string propertyLabel)
	{
		while (HasNextProperty)
		{
			if (ActualProperty.name == propertyLabel)
			{
				break;
			}
			else NextMaterialProperty();
		}
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		pointer = 0;
		this.properties = properties;
		while(HasNextProperty)
		{
			if ((ActualProperty.flags & MaterialProperty.PropFlags.HideInInspector) == 0)
			{
				if (ActualProperty.type == MaterialProperty.PropType.Texture)
				{
					materialEditor.TexturePropertySingleLine(new GUIContent(ActualProperty.name), ActualProperty);
				}
				else
				{
					materialEditor.ShaderProperty(ActualProperty, ActualProperty.displayName);
				}
			}

			if (ActualProperty.name == "_EnableBackgroundNoise")
			{
				if (ActualProperty.floatValue < 0.5f) SkipToProperty("_NoiseMaskParams2");
			}

			if (ActualProperty.name == "_EnableMoon")
			{
				if (ActualProperty.floatValue < 0.5f) SkipToProperty("_Sun");
			}
			
			NextMaterialProperty();
		}
	}
}
