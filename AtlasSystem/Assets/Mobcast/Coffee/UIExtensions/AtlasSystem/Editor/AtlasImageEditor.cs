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
	[CustomEditor(typeof(AtlasImage), true)]
	[CanEditMultipleObjects]
	public class AtlasImageEditor : ImageEditor
	{
		/// <summary>スプライトプレビュー.</summary>
		SpritePreview preview = new SpritePreview();

		/// <summary>[SerializedProperty]アトラス(m_Atlas).</summary>
		protected SerializedProperty spAtlas;

		/// <summary>[SerializedProperty]スプライト名(m_SpriteName).</summary>
		SerializedProperty spSpriteName;

		/// <summary>[SerializedProperty]スプライトタイプ(m_Type).</summary>
		SerializedProperty spType;

		/// <summary>[SerializedProperty]アスペクト比を保持するか(m_PreserveAspect).</summary>
		SerializedProperty spPreserveAspect;

		/// <summary>スプライトタイプによるアニメーションブール.</summary>
		AnimBool animShowType;

		/// <summary>
		/// インスペクタ有効コールバック.
		/// </summary>
		protected override void OnEnable()
		{
			if (!target)
				return;
			
			base.OnEnable();
			spAtlas = serializedObject.FindProperty("m_Atlas");
			spSpriteName = serializedObject.FindProperty("m_SpriteName");
			spType = serializedObject.FindProperty("m_Type");
			spPreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

			animShowType = new AnimBool(spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue));
			animShowType.valueChanged.AddListener(new UnityAction(base.Repaint));
		}

		/// <summary>
		/// インスペクタGUIコールバック.
		/// Inspectorウィンドウを表示するときにコールされます.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			using (new EditorGUI.DisabledGroupScope(true))
			{
				EditorGUILayout.PropertyField(m_Script);
			}

			//アトラスとスプライトを表示.
			DrawAtlasPopupLayout(new GUIContent("Atlas"), new GUIContent("-"), spAtlas);
			EditorGUI.indentLevel++;
			DrawSpritePopup(spAtlas.objectReferenceValue as Atlas, spSpriteName);
			EditorGUI.indentLevel--;

			//Imageインスペクタの再現. ▼ ここから ▼.
			AppearanceControlsGUI();
			RaycastControlsGUI();

			animShowType.target = spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue);
			if (EditorGUILayout.BeginFadeGroup(animShowType.faded))
				this.TypeGUI();
			EditorGUILayout.EndFadeGroup();

			Image.Type imageType = (Image.Type)spType.intValue;
			base.SetShowNativeSize(imageType == Image.Type.Simple || imageType == Image.Type.Filled, false);

			if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
			{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PropertyField(spPreserveAspect);
				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.EndFadeGroup();
			base.NativeSizeButtonGUI();
			//Imageインスペクタの再現. ▲ ここまで ▲.


			serializedObject.ApplyModifiedProperties();

			//プレビューを更新.
			AtlasImage image = target as AtlasImage;
			preview.sprite = (target as AtlasImage).overrideSprite;
			preview.color = image ? image.canvasRenderer.GetColor() : Color.white;
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


		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<Atlas> onChange = null, params GUILayoutOption[] option)
		{
			DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
		}

		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, Atlas atlas, UnityAction<Atlas> onChange = null, params GUILayoutOption[] option)
		{
			DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
		}


		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="rect">描画範囲の矩形.</param>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<Atlas> onSelect = null)
		{
			DrawAtlasPopup(rect, label, nullLabel, atlas.objectReferenceValue as Atlas, obj =>
				{
					atlas.objectReferenceValue = obj;
					if (onSelect != null)
						onSelect(obj as Atlas);
					atlas.serializedObject.ApplyModifiedProperties();
				});
		}

		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="rect">描画範囲の矩形.</param>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, Atlas atlas, UnityAction<Atlas> onSelect = null)
		{
			rect = EditorGUI.PrefixLabel(rect, label);
			if (GUI.Button(rect, atlas ? new GUIContent(atlas.name) : nullLabel, EditorStyles.popup))
			{
				GenericMenu gm = new GenericMenu();
				//nullボタン.
				gm.AddItem(nullLabel, !atlas, () => onSelect(null));

				//プロジェクト内のアセットを、フィルタを使って全検索.
				foreach (string path in AssetDatabase.FindAssets ("t:" + typeof(Atlas).Name).Select (x => AssetDatabase.GUIDToAssetPath (x)))
				{
					string displayName = Path.GetFileNameWithoutExtension(path);
					gm.AddItem(
						new GUIContent(displayName),
						atlas && (atlas.name == displayName),
						x => onSelect(x == null ? null : AssetDatabase.LoadAssetAtPath((string)x, typeof(Atlas)) as Atlas),
						path
					);
				}

				gm.DropDown(rect);
			}
		}

		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		public static void DrawSpritePopup(Atlas atlas, SerializedProperty spriteName)
		{
			DrawSpritePopup(new GUIContent(spriteName.displayName, spriteName.tooltip), atlas, spriteName);
		}

		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		public static void DrawSpritePopup(GUIContent label, Atlas atlas, SerializedProperty spriteName)
		{
			DrawSpritePopup(
				label,
				atlas,
				string.IsNullOrEmpty(spriteName.stringValue) ? "-" : spriteName.stringValue,
				name =>
				{
					if (spriteName == null)
						return;
					spriteName.stringValue = name;
					spriteName.serializedObject.ApplyModifiedProperties();
				}
			);
		}


		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onChange">変更された時のコールバック.</param>
		public static void DrawSpritePopup(GUIContent label, Atlas atlas, string spriteName, UnityAction<string> onChange)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			EditorGUI.BeginDisabledGroup(!atlas);
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.PrefixLabel(label);

			Texture2D tex = atlas ? atlas.atlasTexture : null;

			string assetLabel = tex ? tex.name.Replace(" ", "_") : "";

			if (GUILayout.Button(string.IsNullOrEmpty(spriteName) ? "-" : spriteName, "minipopup"))
			{
				//オブジェクトピッカーで表示させるオブジェクトを制限するために、アトラステクスチャに対して一時的にラベルを設定します.
				//オブジェクトピッカーには、アトラス内スプライトのみが表示されます.
				AssetDatabase.SetLabels(tex, AssetDatabase.GetLabels(tex).Union(new []{ assetLabel }).ToArray());
				EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + assetLabel, controlID);
			}

			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();

			//現在のオブジェクトピッカーであれば、イベント処理.
			if (controlID == EditorGUIUtility.GetObjectPickerControlID())
			{
				string commandName = Event.current.commandName;
				//選択オブジェクト更新イベント
				if (commandName == "ObjectSelectorUpdated")
				{
					Object picked = EditorGUIUtility.GetObjectPickerObject();
					onChange(picked ? picked.name : "");
				}
				//クローズイベント
				else if (commandName == "ObjectSelectorClosed")
				{
					//アトラステクスチャの一時的なラベルを除外します.
					AssetDatabase.SetLabels(tex, AssetDatabase.GetLabels(tex).Except(new []{ assetLabel }).ToArray());
				}
			}
		}
	}
}