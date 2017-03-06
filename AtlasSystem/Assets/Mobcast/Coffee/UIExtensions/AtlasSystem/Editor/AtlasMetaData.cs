using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mobcast.Coffee.UI;

namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// アトラスメタデータクラス.
	/// アトラス内のスプライトや、追加/更新対象のスプライト情報を格納します.
	/// </summary>
	public class AtlasMetaData
	{
		/// <summary>スプライト更新モード.</summary>
		public enum Mode
		{
			/// <summary>アトラス内に含まれている.</summary>
			Include,
			/// <summary>アトラスに追加する.</summary>
			Add,
			/// <summary>アトラスのテクスチャを更新する.</summary>
			Update,
			/// <summary>アトラスから削除する.</summary>
			Delete,
			/// <summary>無視される.</summary>
			Ignore,
		};

		/// <summary>追加されたメタデータか.</summary>
		//public bool isAdd { get; set; }

		/// <summary>無視されるメタデータか.</summary>
		public bool isIgnore { get; set; }

		/// <summary>アトラスから削除されるメタデータか.</summary>
		public bool isDelete { get; set; }

		/// <summary>アトラスに含まれているスプライト.</summary>
		public Sprite spriteInAtlas { get; set; }

		/// <summary>プロジェクトビューで選択済みのスプライト.</summary>
		public Sprite selectedSprite { get; set; }

		/// <summary>プロジェクトビューで選択済みのテクスチャ.</summary>
		public Texture2D selectedTexture { get; set; }

		/// <summary>パッキングに利用されるスプライト.</summary>
		public Sprite spriteForPacking { get { return selectedSprite ?? spriteInAtlas; } }

		/// <summary>スプライトメタデータ.ボーダーやUV</summary>
		public SpriteMetaData spriteMetaData = new SpriteMetaData();

		/// <summary>アトラスに含まれているか.</summary>
		public bool isInclude{ get { return !isDelete && !isIgnore && !isUnused; } }

		/// <summary>変更されるか.</summary>
		public bool isChanged { get { return !isIgnore && mode != Mode.Include; } }

		/// <summary>利用できないメタデータか.</summary>
		public bool isUnused{ get { return !(selectedTexture || spriteForPacking); } }


		/// <summary>画像リソースパス毎の、画像オリジナルに相当するテクスチャ.</summary>
		public static Dictionary<string,Texture2D> rawTextures = new Dictionary<string, Texture2D>();

		/// <summary>
		/// 画像リソースパスにある、画像オリジナルに相当するテクスチャを生成・取得します.
		/// この方法で得られたテクスチャは、元のサイズかつARGB32でインポートされています.
		/// </summary>
		/// <param name="path">画像リソースパス.</param>
		Texture2D GetRawTexture(string path)
		{
			//画像リソースパスごとに、テクスチャをカタログ化します.
			if (!rawTextures.ContainsKey(path))
			{
				//Texture2D.LoadImageメソッドで、バイト配列からテクスチャをロードします.
				//この方法で得られたテクスチャは、元のサイズ＆ARGB32でインポートされます.
				rawTextures[path] = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				rawTextures[path].LoadImage(File.ReadAllBytes(path));

				//ディレイコールでカタログの消去を登録します.
				//アトラス作業時に生成したテクスチャは、自動的に破棄されます.
				EditorApplication.delayCall += rawTextures.Clear;
			}
			return rawTextures[path];
		}

		/// <summary>
		/// パッキングに利用するテクスチャを生成・取得します.
		/// インポート済みのテクスチャの場合、CompressedやMaxSize等インポート設定によってテクスチャが劣化します.
		/// この劣化を防ぐため、テクスチャリソースパスが示す画像ファイルから直接テクスチャをロードし、劣化のないテクスチャを取得します.
		/// 回転パラメータを持たないスプライトにも対応しています.
		/// </summary>
		/// <returns>The texture for packing.</returns>
		public Texture2D GetRawTextureForPacking()
		{
			//インポート済みのテクスチャ.
			string resourcePath = AssetDatabase.GetAssetPath(selectedTexture ?? spriteForPacking.texture);
			Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath);

			//画像リソースパスにある、画像オリジナルに相当するテクスチャ.
			Texture2D rawTexture = GetRawTexture(resourcePath);

			//マルチスプライト以外であれば、クロップ(切り抜き)の必要は無い.オリジナルのテクスチャを返す.
			if ((AssetImporter.GetAtPath(resourcePath) as TextureImporter).spriteImportMode != SpriteImportMode.Multiple)
			{
				return rawTexture;
			}

			//Sprite.rectは「インポートされたテクスチャ」におけるRectに相当するため、一度正規化(uv)し、「オリジナルのテクスチャ」に相当するRectに変換する.
			int x = Mathf.RoundToInt((spriteForPacking.rect.x / importedTexture.width) * rawTexture.width);
			int y = Mathf.RoundToInt((spriteForPacking.rect.y / importedTexture.height) * rawTexture.height);
			int width = Mathf.RoundToInt((spriteForPacking.rect.width / importedTexture.width) * rawTexture.width);
			int height = Mathf.RoundToInt((spriteForPacking.rect.height / importedTexture.height) * rawTexture.height);

			//「オリジナルのテクスチャ」から、切り抜かれたテクスチャを生成し、返す.
			var croppedTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			croppedTexture.SetPixels(rawTexture.GetPixels(x, y, width, height));
			croppedTexture.Apply();
			return croppedTexture;
		}

		/// <summary>現在の更新モード.</summary>
		public Mode mode
		{
			get
			{
				if (isIgnore)
					return Mode.Ignore;
				else if (isDelete)
					return Mode.Delete;
				else if (spriteInAtlas && (selectedTexture || selectedSprite))
					return Mode.Update;
				else if (selectedTexture || selectedSprite)
					return Mode.Add;
				else if (spriteInAtlas)
					return Mode.Include;
				else
					return Mode.Delete;
			}
		}
	}
}
