using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

using Mobcast.Coffee.UI;
using System.IO;


namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// アトラステクスチャインポーター.
	/// アトラステクスチャが更新された時、そのアトラスも更新します.
	/// </summary>
	public class AtlasTextureImporter : AssetPostprocessor
	{
		/// <summary>
		/// アトラステクスチャパスからアトラスに変換するための辞書.
		/// </summary>
		static Dictionary<string,Atlas> atlases
		{
			get
			{
				//アトラステクスチャパス・アトラス変換辞書を生成する.
				if (s_Atlases == null)
				{
					s_Atlases = new Dictionary<string, Atlas>();

					//利用可能なアトラス一覧を取得.
					var availableAtlases = AssetDatabase.FindAssets("t:" + typeof(Atlas).Name)
						.Select(x => AssetDatabase.LoadAssetAtPath<Atlas>(AssetDatabase.GUIDToAssetPath(x)))
						.Where(x => x.atlasTexture != null);

					//辞書に変換.
					foreach (var atlas in availableAtlases)
					{
						s_Atlases[AssetDatabase.GetAssetPath(atlas.atlasTexture)] = atlas;
					}

					//1フレーム後に辞書を破棄.
					EditorApplication.delayCall += () => s_Atlases = null;
				}
				return s_Atlases;
			}
		}

		static Dictionary<string,Atlas> s_Atlases;

		/// <summary>
		/// Raises the postprocess texture event.
		/// </summary>
		void OnPostprocessTexture(Texture2D texture)
		{
			//アトラスが見つかったら更新.
			Atlas targetAtlas;
			if (atlases.TryGetValue(assetPath, out targetAtlas))
			{
				targetAtlas.UpdateTexture();
			}
		}

		/// <summary>
		/// 選択中のオブジェクトからアトラスを作成(Valid).
		/// </summary>
		[MenuItem("Assets/Coffee/Create Atlas", true)]
		static bool CreateAtlasValid(MenuCommand command)
		{
			return Selection.objects.All(x => x is Texture2D);
		}

		/// <summary>
		/// 選択中のオブジェクトからアトラスを作成.
		/// </summary>
		[MenuItem("Assets/Coffee/Create Atlas")]
		static void CreateAtlas(MenuCommand command)
		{
			//テクスチャを元に、アトラスを生成.保存パスはテクスチャパスと同様.
			foreach (var texture in Selection.objects.OfType<Texture2D> ())
			{
				var path = AssetDatabase.GetAssetPath(texture);
				var importer = AssetImporter.GetAtPath(path) as TextureImporter;

				//すでに、この画像からアトラスを作成済みの場合、無視.
				if (atlases.ContainsKey(path))
				{
					Debug.LogErrorFormat("`{0}` is already refered by `{1}`.", texture, atlases[path]);
					continue;
				}
				//MultipleSpriteではない場合、無視.
				else if (importer.spriteImportMode != SpriteImportMode.Multiple)
				{
					Debug.LogErrorFormat("`{0}` is not multiple sprite.", texture);
					continue;
				}

				//アトラスを新規作成.
				string atlasPath = AssetDatabase.GenerateUniqueAssetPath(Path.ChangeExtension(path, "asset"));
				Atlas asset = ScriptableObject.CreateInstance<Atlas>();
				asset.UpdateTexture(texture);
				AssetDatabase.CreateAsset(asset, atlasPath);
			}
			
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

		}
	}
}