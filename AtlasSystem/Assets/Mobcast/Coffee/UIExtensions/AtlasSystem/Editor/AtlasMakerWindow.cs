using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Mobcast.Coffee.UI;
using Mode = Mobcast.CoffeeEditor.UI.AtlasMetaData.Mode;

namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// アトラス作成ウィンドウ.
	/// </summary>
	public class AtlasMakerWindow : EditorWindow
	{
		AtlasMaker atlasMaker;
		bool isSelectedAtlas;
		Vector2 m_ScrollView;

		//---- ▼ GUIキャッシュ ▼ ----
		static GUIStyle styleButton;
		static GUIStyle styleHeader;
		static Texture2D textureDelete;
		static Texture2D textureAdd;
		static Texture2D textureUpdate;
		static Texture2D textureInclude;
		static Texture2D textureIgnore;
		static Texture2D textureTrash;
		static Texture2D textureReflash;
		static GUIContent contentAdd = new GUIContent ("Add");
		static GUIContent contentUpdate = new GUIContent ("Update");
		static GUIContent contentInclude = new GUIContent ("Include");
		static GUIContent contentIgnore = new GUIContent ("Ignore");
		static GUIContent contentDelete = new GUIContent ("Delete");
		static GUIContent tmpContent = new GUIContent ();
		static bool cached;

		static void CacheGUI ()
		{
			if (cached)
				return;
			cached = true;

			textureDelete = EditorGUIUtility.FindTexture ("sv_label_6");
			textureAdd = EditorGUIUtility.FindTexture ("sv_label_3");
			textureUpdate = EditorGUIUtility.FindTexture ("sv_label_1");
			textureInclude = EditorGUIUtility.FindTexture ("sv_label_0");
			textureIgnore = EditorGUIUtility.FindTexture ("sv_label_7");

			textureTrash = EditorGUIUtility.FindTexture ("d_TreeEditor.Trash");
			textureReflash = EditorGUIUtility.FindTexture ("Refresh");

			styleButton = new GUIStyle ("minibutton");
			styleButton.normal.textColor = 
				styleButton.hover.textColor = 
					styleButton.active.textColor = 
						styleButton.focused.textColor = Color.white;

			styleButton.fixedHeight = 15;
			styleButton.fixedWidth = 80;
			styleButton.fontSize = 11;
			styleButton.fontStyle = FontStyle.Bold;
			styleButton.border = new RectOffset (6, 6, 6, 6);

			styleHeader = new GUIStyle ("IN BigTitle");
			styleHeader.margin = new RectOffset ();
		}



		/// <summary>
		/// アトラスメーカーウィンドウを表示します.
		/// </summary>
		[MenuItem ("Coffee/UI/Atlas Maker")]
		static void OpenFromMenu ()
		{
			EditorWindow.GetWindow<AtlasMakerWindow> ();
		}

		/// <summary>
		/// アトラスを指定して、アトラスメーカーを開きます.
		/// </summary>
		public static void OpenFromAtlas (Atlas atlas)
		{
			var window = EditorWindow.GetWindow<AtlasMakerWindow> ();
			window.atlasMaker.ChangeCurrentAtlas (atlas);
		}


		void OnEnable ()
		{
			titleContent.text = "Atlas Maker";
			atlasMaker = new AtlasMaker ();
			Selection.selectionChanged += atlasMaker.OnSelectionChanged;
			Selection.selectionChanged += Repaint;
			Selection.selectionChanged ();
		}

		void OnDisable ()
		{
			Selection.selectionChanged -= atlasMaker.OnSelectionChanged;
			Selection.selectionChanged -= Repaint;
			atlasMaker = null;
		}

		void UpdateAtlas ()
		{
			//作業アトラスを選択していない(new ...)場合は、新規アトラスを作成します(Create).
			atlasMaker.targetAtlas = atlasMaker.targetAtlas ?? CreateAtlas ();

			//アトラスを更新して保存.
			atlasMaker.UpdateAtlas ();
		}

		/// <summary>
		/// アトラスメーカーを描画します.
		/// </summary>
		void OnGUI ()
		{
			//プロジェクトビューでアトラスを削除した時、targetAtlasをnullに設定し、Createモードに更新します.
			if (isSelectedAtlas && !atlasMaker.targetAtlas)
				atlasMaker.ChangeCurrentAtlas (null);
			isSelectedAtlas = atlasMaker.targetAtlas;

			CacheGUI ();
			using (new GUILayout.HorizontalScope (styleHeader)) {
				//作業アトラスの選択.
				//プロジェクト内に存在するアトラスをポップアップ表示します.
				//作業アトラスを選択した場合は、更新モードになります(Update).
				//作業アトラスを選択しない(new ...)場合は、新規アトラス作成モードになります(Create).
				EditorGUIUtility.labelWidth = 50;
				AtlasImageEditor.DrawAtlasPopupLayout (
					new GUIContent ("Atlas"), 
					new GUIContent ("Create new Atlas"), 
					atlasMaker.targetAtlas,
					selected => atlasMaker.targetAtlas = selected,
					GUILayout.MaxWidth (position.width * 0.5f)
				);

				//アトラス内のスプライト間隔.
				atlasMaker.padding = Mathf.Clamp (EditorGUILayout.IntField ("Padding", atlasMaker.padding, GUILayout.MaxWidth (70)), 0, 10);

				//正方形にアトラスをリサイズ.
				atlasMaker.squared = EditorGUILayout.Toggle ("Squared", atlasMaker.squared, GUILayout.MaxWidth (70));

				GUILayout.FlexibleSpace ();
				//アトラス更新/作成ボタン.アトラス更新をトリガします.
				using (new EditorGUI.DisabledGroupScope (!atlasMaker.metaDatas.Any (x => x.isChanged))) {
					if (GUILayout.Button (atlasMaker.targetAtlas ? "Update" : "Create", GUILayout.Width (50)))
						EditorApplication.delayCall += UpdateAtlas;
				}
			}

			//アトラス内スプライト、及び選択中のスプライトリストを表示します.
			if (0 < atlasMaker.metaDatas.Count) {
				using (var svs = new EditorGUILayout.ScrollViewScope (m_ScrollView)) {
					m_ScrollView = svs.scrollPosition;
					foreach (var m in atlasMaker.metaDatas)
						Draw (GUILayoutUtility.GetRect (50, 20, GUILayout.ExpandWidth (true)), m);
				}
			}
			//アトラス内にスプライトがない/選択中のスプライトがない場合、ヘルプを表示します.
			else {
				EditorGUILayout.HelpBox ("Please select one or more textures in the Project View window.", MessageType.Info);
			}
			return;
		}

		/// <summary>新しいAtlasアセットを生成します.</summary>
		public static Atlas CreateAtlas ()
		{
			string typeName = typeof(Atlas).Name;
			string prefsKey = "CreateScriptableObject_" + typeName + "_" + Application.dataPath.GetHashCode();
			string dir = EditorPrefs.GetString (prefsKey, "Assets");
			if (string.IsNullOrEmpty (dir) || !Directory.Exists (dir) || !dir.Contains (Application.dataPath))
				dir = "Assets";
			
			string assetPath = Path.Combine (dir, "New " + typeName + ".asset").Replace (Application.dataPath, "Assets");
			assetPath = AssetDatabase.GenerateUniqueAssetPath (assetPath);

			//assetファイル保存ダイアログを開く.
			//キャンセルした、またはアセットフォルダ外を選択したら何もしない.
			assetPath = EditorUtility.SaveFilePanel ("Create New " + typeName, Path.GetDirectoryName(assetPath), Path.GetFileName (assetPath), "asset");

			if (assetPath.Length == 0)
				return null;
			else if (!assetPath.Contains (Application.dataPath)) {
				EditorUtility.DisplayDialog ("Out of 'Assets' directory", "Select file in 'Assets' directory.", "OK");
				return null;
			}


			//アセット生成して保存.
			dir = Path.GetDirectoryName (assetPath);
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);
			
			assetPath = assetPath.Replace (Application.dataPath, "Assets");
			ScriptableObject asset = ScriptableObject.CreateInstance (typeof(Atlas));
			AssetDatabase.CreateAsset (asset, assetPath);
			AssetDatabase.SaveAssets ();
			EditorPrefs.SetString (prefsKey, dir);

			return asset as Atlas;
		}

		/// <summary>GUIスタイル用テクスチャ.</summary>
		Texture2D GetTextureGUIStyle (Mode mode)
		{
			switch (mode) {
				case Mode.Add:
					return textureAdd;
				case Mode.Update:
					return textureUpdate;
				case Mode.Include:
					return textureInclude;
				case Mode.Ignore:
					return textureIgnore;
				case Mode.Delete:
					return textureDelete;
				default:
					return null;
			}
		}

		/// <summary>GUIスタイル用テクスチャ.</summary>
		GUIContent GetGUIContent (Mode mode)
		{
			switch (mode) {
				case Mode.Add:
					return contentAdd;
				case Mode.Update:
					return contentUpdate;
				case Mode.Include:
					return contentInclude;
				case Mode.Ignore:
					return contentIgnore;
				case Mode.Delete:
					return contentDelete;
			}
			return GUIContent.none;
		}

		/// <summary>
		/// アトラス内のスプライト項目を描画します.
		/// </summary>
		public void Draw (Rect rect, AtlasMetaData meta)
		{
			var r = rect;
			tmpContent.text = meta.spriteMetaData.name;
			r.Set (rect.x + 5, rect.y + 3, rect.width - 120, rect.height - 3);
			GUI.Label (r, tmpContent);

			styleButton.normal.background = 
				styleButton.hover.background = 
					styleButton.active.background = 
						styleButton.focused.background = GetTextureGUIStyle (meta.mode);

			r.Set (rect.x + rect.width - 100, rect.y + 3, 100, rect.height - 3);
			GUI.Label (r, GetGUIContent (meta.mode), styleButton);

			r.Set (rect.x + rect.width - 20, rect.y + 3, 24, 24);
			if (GUI.Button (r, meta.isDelete ? textureReflash : textureTrash, EditorStyles.label)) {
				meta.isDelete = !meta.isDelete;
			}

			r.Set (rect.x, rect.y + rect.height, rect.width, 1);
			GUI.Label (rect, GUIContent.none, "sv_iconselector_sep");
		}
	}
}
