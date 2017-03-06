using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using Mobcast.Coffee.UI;


namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// Atlas用 インスペクタEditor.
	/// </summary>
	[CustomEditor(typeof(Atlas), true)]
	public class AtlasEditor : Editor
	{
		Vector2 m_Scroll;
		ReorderableList ro;
		SpritePreview preview = new SpritePreview();
		GUIContent content = new GUIContent();

		public void OnEnable()
		{
			ro = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Values"), false, true, false, false);
			ro.drawElementCallback = (rect, index, isActive, isFocus) =>
			{
				rect.height -= 2;
				var sp = ro.serializedProperty.GetArrayElementAtIndex(index);

				content.text = sp.objectReferenceValue ? sp.objectReferenceValue.name : "";
				EditorGUI.PropertyField(rect, sp, content);
			};
			ro.drawHeaderCallback = (rect) => GUI.Label(rect, string.Format("Included Sprites ({0})", ro.count));
			ro.onSelectCallback = (list) =>
			{
				var sp = ro.serializedProperty.GetArrayElementAtIndex(list.index);
				preview.sprite = sp.objectReferenceValue as Sprite;
			};
			ro.elementHeight = 18;
		}

		/// <summary>
		/// インスペクタGUIコールバック.
		/// Inspectorウィンドウを表示するときにコールされます.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();


			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
			GUI.enabled = true;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AtlasTexture"));

			//アトラスエディタを表示.
			if (GUILayout.Button("Open Atlas Editor"))
			{
				AtlasMakerWindow.OpenFromAtlas(target as Atlas);
			}
			//アトラスエディタを表示.
			if (GUILayout.Button("Update"))
			{
				(target as Atlas).UpdateTexture();
			}
			ro.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// オブジェクトプレビューが可能な場合はtrueを返します.
		/// </summary>
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
