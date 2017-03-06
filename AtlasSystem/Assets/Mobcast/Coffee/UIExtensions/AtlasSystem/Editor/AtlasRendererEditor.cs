using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using System.IO;
using System.Linq;
using Mobcast.Coffee.UI;

namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// AtlasImage用 インスペクタEditor.
	/// Imageのインスペクタを継承しています.
	/// Atlasの選択にはポップアップを、スプライトの選択には専用のセレクタを実装しています.
	/// スプライトプレビュー機能により、スプライトのボーダーをインスペクタから変更できます.
	/// </summary>
	[CustomEditor(typeof(AtlasRenderer), true)]
	[CanEditMultipleObjects]
	public class AtlasRendererEditor : Editor
	{
		/// <summary>スプライトプレビュー.</summary>
		SpritePreview preview = new SpritePreview();

		/// <summary>[SerializedProperty]アトラス(m_Atlas).</summary>
		SerializedProperty spAtlas;

		/// <summary>[SerializedProperty]スプライト名(m_SpriteName).</summary>
		SerializedProperty spSpriteName;

		/// <summary>
		/// インスペクタ有効コールバック.
		/// </summary>
		void OnEnable()
		{
			spAtlas = serializedObject.FindProperty("m_Atlas");
			spSpriteName = serializedObject.FindProperty("m_SpriteName");
		}

		/// <summary>
		/// インスペクタGUIコールバック.
		/// Inspectorウィンドウを表示するときにコールされます.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			//アトラスとスプライトを表示.
			AtlasImageEditor.DrawAtlasPopupLayout(new GUIContent("Atlas"), new GUIContent("-"), spAtlas, atlas =>
			{
				if (!atlas || !atlas.Contains(spSpriteName.stringValue))
					spSpriteName.stringValue = "";
			});
			EditorGUI.indentLevel++;
			AtlasImageEditor.DrawSpritePopup(spAtlas.objectReferenceValue as Atlas, spSpriteName);
			EditorGUI.indentLevel--;

			serializedObject.ApplyModifiedProperties();

			//プレビューを更新.
			AtlasRenderer atlasRenderer = target as AtlasRenderer;
			preview.sprite = atlasRenderer.cachedSpriteRenderer.sprite;
			preview.color = atlasRenderer.cachedSpriteRenderer.color;
		}

		public override bool HasPreviewGUI()
		{
			return true;
		}

		/// <summary>
		/// オブジェクトプレビューのタイトルを返します.
		/// </summary>
		public override GUIContent GetPreviewTitle()
		{
			return preview.GetPreviewTitle();
		}

		/// <summary>
		/// インタラクティブなカスタムプレビューを表示します.
		/// </summary>
		public override void OnPreviewGUI(Rect rect, GUIStyle background)
		{
			preview.OnPreviewGUI(rect);
		}

		/// <summary>
		/// オブジェクトプレビューの上部にオブジェクト情報を示します。
		/// </summary>
		public override string GetInfoString()
		{
			return preview.GetInfoString();
		}

		/// <summary>
		/// プレビューのヘッダーを表示します.
		/// </summary>
		public override void OnPreviewSettings()
		{
			preview.OnPreviewSettings();
		}


	}
}