using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using UnityEditor.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mobcast.Coffee.UI;


namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// アトラス作成/更新クラス.
	/// 作業中のアトラスを管理します.
	/// </summary>
	public class AtlasMaker
	{
		/// <summary>メタデータリスト.</summary>
		public List<AtlasMetaData> metaDatas = new List<AtlasMetaData>();

		/// <summary>作業アトラス.</summary>
		public Atlas targetAtlas
		{
			get{ return m_TargetAtlas; }
			set
			{
				if (m_TargetAtlas != value)
				{
					ChangeCurrentAtlas(value);
				}
			}
		}

		Atlas m_TargetAtlas;

		/// <summary>作業中のアトラステクスチャ.</summary>
		public Texture2D targetAtlasTexture{ get { return m_TargetAtlas ? m_TargetAtlas.atlasTexture : null; } }

		/// <summary>アトラス内のスプライト間隔.</summary>
		public int padding = 2;

		/// <summary>アトラステクスチャを正方形に変形する.</summary>
		public bool squared = true;

		/// <summary>
		/// 作業アトラスを変更します.
		/// これは、AtlasMakerWindowに反映されます.
		/// </summary>
		/// <param name="atlas">アトラス.</param>
		/// <param name="forceChanged">アトラスに変更がある場合はtrue.</param>
		public void ChangeCurrentAtlas(Atlas atlas, bool forceChanged = true)
		{
			//作業中のアトラスメタデータから、スプライトの参照を破棄します.
			foreach (var meta in metaDatas)
			{
				if (forceChanged)
				{
					meta.spriteInAtlas = null;
					meta.isDelete = false;
				}
				meta.isIgnore = (meta.selectedTexture == targetAtlasTexture);
			}

			//アトラスを変更.アトラステクスチャがある場合は、アトラステクスチャを更新.
			m_TargetAtlas = atlas;
			if (targetAtlasTexture)
			{
				m_TargetAtlas.UpdateTexture(targetAtlasTexture);

				//アトラステクスチャ内のスプライトを全て取得.
				string assetPath = AssetDatabase.GetAssetPath(targetAtlasTexture);
				TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
				List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToList();
				
				//スプライトシートの内容を取得.
				foreach (var spriteSheet in textureImporter.spritesheet)
				{
					AtlasMetaData atlasMetaData = GetOrAddMetaData(sprites.Find(sprite => sprite.name == spriteSheet.name), false);
					if (forceChanged)
					{
						atlasMetaData.isDelete = false;
					}
				}
			}

			//メタデータリストから、スプライトもテクスチャも指定されていないメタデータを除外.
			metaDatas.RemoveAll(meta => meta.isUnused);
		}

		/// <summary>
		/// 選択オブジェクト変更コールバック.
		/// プロジェクトウィンドウでオブジェクトの選択を変更した時にコールされます.
		/// </summary>
		public void OnSelectionChanged()
		{
			//アトラスが選択されていたら、ターゲットアトラスを更新.
			var atlas = Selection.objects.OfType<Atlas>().FirstOrDefault();
			if (atlas)
			{
				ChangeCurrentAtlas(atlas);
			}

			//作業中のアトラスメタデータから、スプライトの参照を破棄.
			foreach (var meta in metaDatas)
			{
				meta.selectedTexture = null;
				meta.selectedSprite = null;

			}

			//プロジェクトウィンドウで選択しているテクスチャをすべて取得(DeapAsset).アトラステクスチャは除外する.
			var selectedImages = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets)
			                 .Select(x => x as Texture2D);

			//選択したテクスチャについて、すべてのアトラスメタデータを作成.
			foreach (var texture in selectedImages)
			{
				//選択したテクスチャのアトラスメタデータを取得、もしくは作成.
				string path = AssetDatabase.GetAssetPath(texture);
				AtlasMetaData atlasMetaData = GetOrAddMetaData(texture, false);

				//multipleSpriteの場合、すべてのスプライトをリストに追加.
				if (!atlasMetaData.isIgnore && (AssetImporter.GetAtPath(path) as TextureImporter).spriteImportMode == SpriteImportMode.Multiple)
				{
					//multipleSprite自身はリストから除外.
					metaDatas.Remove(atlasMetaData);

					//multipleSpriteに含まれる全てのスプライトを追加.
					foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath (path).OfType<Sprite> ())
					{
						GetOrAddMetaData(sprite, true);
					}
				}
			}
				
			//メタデータリストから、スプライトもテクスチャも指定されていない(=使用されない)メタデータを除外.
			metaDatas.RemoveAll(meta => meta.isUnused);

			//メタデータリストをスプライト名とモードで並び替え.
			metaDatas = metaDatas
					.OrderBy(x => x.mode == AtlasMetaData.Mode.Add || x.mode == AtlasMetaData.Mode.Ignore)
					.ThenBy(x => x.spriteMetaData.name)
					.ToList();
		}

		/// <summary>
		/// アトラスメタデータを取得、または生成します.
		/// すでに同じ名前のメタデータがある場合、そちらを取得します.
		/// </summary>
		/// <param name="image">アトラスメタデータに紐づくSprite、もしくはTexture.</param>
		/// <param name="spriteOverride">入力されたオブジェクトがSpriteの場合、overrideSpriteとしてマークします.</param>
		AtlasMetaData GetOrAddMetaData(Object image, bool spriteOverride = false)
		{
			if (!image)
				return null;

			//既に同じ名前のメタデータがある場合は取得.ない場合は新規作成.
			AtlasMetaData atlasMetaData = metaDatas.FirstOrDefault(meta => meta.spriteMetaData.name == image.name);
			if (atlasMetaData == null)
			{
				atlasMetaData = new AtlasMetaData();
				atlasMetaData.spriteMetaData.name = image.name;
				metaDatas.Add(atlasMetaData);
			}

			//入力されたオブジェクトがTextureの場合.
			if (image is Texture2D)
			{
				atlasMetaData.selectedTexture = image as Texture2D;

				//atlasTextureと同じオブジェクトの場合は無視(isIgnore)する.
				atlasMetaData.isIgnore = (atlasMetaData.selectedTexture == targetAtlasTexture);
			}
			//入力されたオブジェクトがSpriteの場合.
			else if (image is Sprite)
			{
				var sprite = image as Sprite;
				if (spriteOverride)
					atlasMetaData.selectedSprite = sprite;
				else
					atlasMetaData.spriteInAtlas = sprite;
				atlasMetaData.isIgnore = false;

				atlasMetaData.spriteMetaData.border = sprite.border;
				atlasMetaData.spriteMetaData.pivot = sprite.pivot;
			}
			return atlasMetaData;
		}



		/// <summary>
		/// アトラスを更新して保存.
		/// </summary>
		public void UpdateAtlas()
		{
			if (!targetAtlas)
				return;

			try
			{
				ChangeCurrentAtlas(targetAtlas, false);

				//アトラステクスチャの出力パスを取得します.
				string outPath = AssetDatabase.GetAssetPath(targetAtlas.atlasTexture ? targetAtlas.atlasTexture.GetInstanceID() : targetAtlas.GetInstanceID());
				outPath = Path.ChangeExtension(outPath, "png");

				//アトラスに残留・追加・更新されるメタデータを抽出します.
				//ステータスがDeleteやIgnoreであるメタデータは含まれません.
				List<AtlasMetaData> includingAtlasMetaDatas = metaDatas
					.Where(meta => meta.isInclude)
					.ToList();


				//#### step 1 : 素材テクスチャの生成 ####
				//パッキングに利用するテクスチャは、画像ファイルから直接ロードされるため、インポートによる劣化はありません.
				//これにより、多重インポートによるアトラス画像の劣化を回避します.
				int count = includingAtlasMetaDatas.Count;
				Texture2D[] texturesForPacking = includingAtlasMetaDatas
					.Select((meta, index) =>
					{
						EditorUtility.DisplayProgressBar("Update atlas (step 1/4)", "Create raw texture : " + meta.spriteMetaData.name, 0.5f * index / count);
						return meta.GetRawTextureForPacking();
					})
					.ToArray();


				//#### step 2 : テクスチャのパッキング ####
				EditorUtility.DisplayProgressBar("Update atlas (step 2/4)", "Packing texture", 0.5f);

				//素材となるテクスチャ全てを1枚のテスクチャにパッキングし、アトラステクスチャを作成します.
				Texture2D packedTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				int maxSize = Mathf.Min(SystemInfo.maxTextureSize, 2048);
				Rect[] rects = packedTexture.PackTextures(texturesForPacking, padding, maxSize);

				//アトラステクスチャに保存されているスプライトメタデータを、パッキング矩形配列から更新します.
				SpriteMetaData[] includingSpriteMetaDatas = includingAtlasMetaDatas
					.Select(atlasMeta => atlasMeta.spriteMetaData)
					.ToArray();

				//正規化されたRectから、SpriteMetaData用のRectに変換します.
				for (int i = 0; i < rects.Length; i++)
				{
					includingSpriteMetaDatas[i].rect = new Rect(
						Mathf.Round(rects[i].x * packedTexture.width),
						Mathf.Round(rects[i].y * packedTexture.height),
						Mathf.Round(rects[i].width * packedTexture.width),
						Mathf.Round(rects[i].height * packedTexture.height)
					);
				}

				//squaredが指定されている場合、サイズを正方形に整形します.
				if (squared)
				{
					packedTexture = GetSquaredTexture(packedTexture);
				}


				//#### step 3 : アトラステクスチャのエクスポート ####
				EditorUtility.DisplayProgressBar("Update atlas (step 3/4)", "Export texture : " + Path.GetFileName(outPath), 0.7f);

				//パッキングしたテクスチャをpngにエンコードし、画像ファイルとして上書き出力します.
				bool existAtlasTextureFile = File.Exists(outPath);
				File.WriteAllBytes(outPath, packedTexture.EncodeToPNG());

				//インポーター生成のため、初回だけ強制インポートを実行します.
				if (!existAtlasTextureFile)
				{
					AssetDatabase.ImportAsset(outPath);
					AssetDatabase.Refresh();
				}


				//#### step 4 : アトラステクスチャのインポートとアトラスの更新 ####
				EditorUtility.DisplayProgressBar("Update atlas (step 4/4)", "Reimort atlas", 1.0f);

				//アトラステクスチャのスプライトシートを更新します.
				TextureImporter textureImporter = AssetImporter.GetAtPath(outPath) as TextureImporter;
				textureImporter.textureType = TextureImporterType.Sprite;
				textureImporter.spriteImportMode = SpriteImportMode.Multiple;
				textureImporter.isReadable = false;
				textureImporter.spritesheet = includingSpriteMetaDatas;
				textureImporter.SaveAndReimport();
				AssetDatabase.Refresh();

				//アトラスを更新します.
				targetAtlas.UpdateTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(outPath));
				ChangeCurrentAtlas(targetAtlas);
				OnSelectionChanged();

				//未使用アセットをアンロードします.
				AssetDatabase.SaveAssets();
				EditorUtility.UnloadUnusedAssetsImmediate();
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>
		/// 1辺がPOT(2のべき乗)ピクセルである正方形テクスチャを返します.
		/// </summary>
		/// <param name="texture">テクスチャ.</param>
		Texture2D GetSquaredTexture(Texture2D texture)
		{
			int w = texture.width;
			int h = texture.height;

			//元のピクセルを保存.
			Color[] pixelColors = texture.GetPixels(0, 0, w, h);

			//新しいサイズのテクスチャを生成.
			int squaredSize = Mathf.Max(w, h);
			texture = new Texture2D(squaredSize, squaredSize, TextureFormat.ARGB32, false);

			//全ピクセルを透明色でクリア.
			Color clear = new Color(0, 0, 0, 0);
			for (int y = 0; y < squaredSize; y++)
				for (int x = 0; x < squaredSize; x++)
					texture.SetPixel(x, y, clear);

			//元のピクセルをペースト.
			texture.SetPixels(0, 0, w, h, pixelColors);
			return texture;
		}
	}
}
