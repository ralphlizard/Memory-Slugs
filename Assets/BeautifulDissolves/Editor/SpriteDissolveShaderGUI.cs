using System;
using UnityEngine;

namespace UnityEditor
{
	internal class SpriteDissolveShaderGUI : ShaderGUI
	{
		protected static class Styles
		{
			public static string emptyTootip = "";
			public static GUIContent spriteText = new GUIContent("Sprite Texture", "");
			public static GUIContent spriteColorText = new GUIContent("Tint", "");
			public static GUIContent pixelSnapText = new GUIContent("Pixel Snap", "");
			public static GUIContent dissolveMapText = new GUIContent("Dissolve Map", "Dissolve Map (RGB)");
			public static GUIContent directionMapText = new GUIContent("Direction Map", "Direction of Dissolve (RGB)");
			public static GUIContent dissolveAmountText = new GUIContent("Dissolve Amount", "Dissolve Amount");
			public static GUIContent substituteText = new GUIContent("Substitute Texture", "Substitute Texture (RGB)");
			public static GUIContent substituteColorText = new GUIContent("Substitute Color", "");
			public static GUIContent outerEdgeText = new GUIContent("Outer Edge", "");
			public static GUIContent innerEdgeText = new GUIContent("Inner Edge", "");
			public static GUIContent edgeThicknessText = new GUIContent("Thickness", "");
			public static GUIContent edgeThicknessSoloText = new GUIContent("Edge Thickness", "");
			public static GUIContent blendColorsText = new GUIContent("Edge Color Blending", "");
			public static GUIContent burnInText = new GUIContent("Dissolve Glow", "");
			public static GUIContent burnColorText = new GUIContent("Glow Color", "");
			public static GUIContent burnIntensity = new GUIContent("Glow Intensity", "");
			public static GUIContent tiling = new GUIContent("Tiling", "");
			public static GUIContent glowFollowText = new GUIContent("Follow-Through", "");

			public static GUIContent edgeColorRampText = new GUIContent("Edge Color Ramp", "Edge Color Ramp (RGB)");
			public static GUIContent useEdgeColorRampText = new GUIContent("Use Edge Color Ramp", "");

			public static string whiteSpaceString = " ";
			public static string dissolveSettings = "Dissolve Settings";
			public static string advancedSettings = "Advanced Settings";
		}

		MaterialProperty spriteMap = null;
		MaterialProperty spriteColor = null;
		MaterialProperty pixelSnap = null;
		MaterialProperty dissolveMap = null;
		MaterialProperty directionMap = null;
		protected MaterialProperty dissolveAmount = null;
		MaterialProperty substituteMap = null;
		MaterialProperty substituteColor = null;
		MaterialProperty outerEdgeColor = null;
		MaterialProperty innerEdgeColor = null;
		MaterialProperty outerEdgeThickness = null;
		MaterialProperty innerEdgeThickness = null;
		MaterialProperty colorBlend = null;
		MaterialProperty dissolveGlow = null;
		MaterialProperty glowColor = null;
		MaterialProperty glowIntensity = null;
		protected MaterialProperty tilingX = null;
		protected MaterialProperty tilingY = null;
		MaterialProperty glowFollow = null;

		MaterialProperty useEdgeColorRamp = null;
		MaterialProperty edgeColorRamp = null;

		protected MaterialEditor m_MaterialEditor;

		bool m_FirstTimeApply = true;
		bool m_AdvancedSettings = false;
		protected Vector2 m_Tiling = new Vector2(1f, 1f);

		public void FindProperties (MaterialProperty[] props)
		{
			spriteMap = FindProperty ("_MainTex", props);
			spriteColor = FindProperty ("_Color", props);
			pixelSnap = FindProperty ("PixelSnap", props, false);
			dissolveMap = FindProperty ("_DissolveMap", props);
			directionMap = FindProperty ("_DirectionMap", props);
			dissolveAmount = FindProperty ("_DissolveAmount", props);
			substituteMap = FindProperty ("_SubTex", props, false);
			substituteColor = FindProperty("_SubCol", props, false);
			outerEdgeColor = FindProperty ("_OuterEdgeColor", props);
			innerEdgeColor = FindProperty ("_InnerEdgeColor", props);
			outerEdgeThickness = FindProperty ("_OuterEdgeThickness", props);
			innerEdgeThickness = FindProperty ("_InnerEdgeThickness", props);
			colorBlend = FindProperty ("_ColorBlending", props);
			dissolveGlow = FindProperty ("_DissolveGlow", props);
			glowColor = FindProperty ("_GlowColor", props);
			glowIntensity = FindProperty ("_GlowIntensity", props);
			tilingX = FindProperty ("_TilingX", props);
			tilingY = FindProperty ("_TilingY", props);
			glowFollow = FindProperty("_GlowFollow", props, false);

			useEdgeColorRamp = FindProperty("_UseEdgeColorRamp", props);
			edgeColorRamp = FindProperty("_EdgeColorRamp", props);
		}

		public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
		{
			FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
			m_MaterialEditor = materialEditor;
			Material material = materialEditor.target as Material;

			ShaderPropertiesGUI (material);

			// Make sure that needed keywords are set up if we're switching some existing
			// material to a standard shader.
			if (m_FirstTimeApply)
			{
				SetMaterialKeywords (material);
				m_FirstTimeApply = false;
			}
		}

		public void ShaderPropertiesGUI (Material material)
		{
			// Use default labelWidth
			EditorGUIUtility.labelWidth = 0f;

			// Detect any changes to the material
			EditorGUI.BeginChangeCheck();
			{
				// Primary properties
				DoAlbedoArea();

				// Dissolve settings
				GUILayout.Label(Styles.dissolveSettings, EditorStyles.boldLabel);
				DoDissolveArea(material);

			}
			if (EditorGUI.EndChangeCheck())
			{
				SetMaterialKeywords(material);
			}
		}

		public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);

			if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
				return;
			SetMaterialKeywords(material);
		}

		public virtual void ShowDissolveMap(Material material)
		{
			m_MaterialEditor.ShaderProperty(dissolveAmount, Styles.dissolveAmountText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
		}

		public virtual void DoDissolveArea(Material material)
		{
			m_MaterialEditor.TexturePropertySingleLine(Styles.dissolveMapText, dissolveMap);

			ShowDissolveMap(material);

			m_MaterialEditor.TexturePropertySingleLine(Styles.directionMapText, directionMap);

			if (substituteMap != null) {
				m_MaterialEditor.TexturePropertySingleLine(Styles.substituteText, substituteMap);
			}

			if (substituteColor != null) {
				m_MaterialEditor.ColorProperty(substituteColor, Styles.substituteColorText.text);
			}

			m_MaterialEditor.ShaderProperty(dissolveGlow, Styles.burnInText.text);

			if (material.IsKeywordEnabled("_DISSOLVEGLOW_ON")) {
				if (glowFollow != null) {
					m_MaterialEditor.ShaderProperty(glowFollow, Styles.glowFollowText.text);
				}
				m_MaterialEditor.ColorProperty(glowColor, Styles.burnColorText.text);
				m_MaterialEditor.FloatProperty(glowIntensity, Styles.burnIntensity.text);
			}

			if (!material.IsKeywordEnabled("_EDGECOLORRAMP_USE")) {
				m_MaterialEditor.ShaderProperty(colorBlend, Styles.blendColorsText.text);
				m_MaterialEditor.ColorProperty(outerEdgeColor, Styles.outerEdgeText.text);
				if (!material.IsKeywordEnabled("_COLORBLENDING_ON")) {
					m_MaterialEditor.ShaderProperty(outerEdgeThickness, Styles.edgeThicknessText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
				}
				m_MaterialEditor.ColorProperty(innerEdgeColor, Styles.innerEdgeText.text);
				if (!material.IsKeywordEnabled("_COLORBLENDING_ON")) {
					m_MaterialEditor.ShaderProperty(innerEdgeThickness, Styles.edgeThicknessText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
				} else {
					m_MaterialEditor.ShaderProperty(innerEdgeThickness, Styles.edgeThicknessSoloText.text);
				}
			} else {
				m_MaterialEditor.TexturePropertySingleLine(Styles.edgeColorRampText, edgeColorRamp);
				m_MaterialEditor.ShaderProperty(innerEdgeThickness, Styles.edgeThicknessSoloText.text);
			}

			EditorGUILayout.Space();
			EditorGUI.indentLevel++;
			m_AdvancedSettings = EditorGUILayout.Foldout(m_AdvancedSettings, Styles.advancedSettings);
			if (m_AdvancedSettings) {
				EditorGUI.indentLevel++;
				EditorGUILayout.Space();
				DoAdvancedArea(material);
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
		}

		public virtual void DoAlbedoArea()
		{
			m_MaterialEditor.TexturePropertySingleLine(Styles.spriteText, spriteMap, spriteColor);

			EditorGUI.indentLevel += (MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
			m_Tiling.x = tilingX.floatValue;
			m_Tiling.y = tilingY.floatValue;
			EditorGUI.BeginChangeCheck();
			m_Tiling = EditorGUILayout.Vector2Field(Styles.tiling, m_Tiling);
			if (EditorGUI.EndChangeCheck()) {
				tilingX.floatValue = m_Tiling.x;
				tilingY.floatValue = m_Tiling.y;

			}
			EditorGUI.indentLevel -= (MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);

			if (pixelSnap != null) {
				m_MaterialEditor.ShaderProperty(pixelSnap, Styles.pixelSnapText.text);
			}
		}

		void DoAdvancedArea(Material material)
		{
			m_MaterialEditor.ShaderProperty(useEdgeColorRamp, Styles.useEdgeColorRampText.text);
		}

		static void SetMaterialKeywords(Material material)
		{
			// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
			// (MaterialProperty value might come from renderer material property block)

			if (material.HasProperty("_SubTex")) {
				SetKeyword (material, "_SUBMAP", material.GetTexture ("_SubTex"));
			}

			SetKeyword (material, "_DISSOLVEMAP", material.GetTexture ("_DissolveMap"));
			SetKeyword (material, "_DIRECTIONMAP", material.GetTexture ("_DirectionMap"));
		}

		static void SetKeyword(Material m, string keyword, bool state)
		{
			if (state)
				m.EnableKeyword (keyword);
			else
				m.DisableKeyword (keyword);
		}
	}

} // namespace UnityEditor
