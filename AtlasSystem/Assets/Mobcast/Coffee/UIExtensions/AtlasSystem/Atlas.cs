using UnityEngine;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// スプライト用アトラス.
	/// アトラステクスチャとスプライト名を管理します.
	/// アトラスはAtlasImageコンポーネントから利用します.
	/// </summary>
	public class Atlas : ScriptableObject, ISerializationCallbackReceiver
	{
		/// <summary>アトラステクスチャ.</summary>
		public Texture2D atlasTexture { get { return m_AtlasTexture; } }
		[SerializeField] Texture2D m_AtlasTexture = null;

		/// <summary>スプライト名/スプライト変換辞書.</summary>
		Dictionary<string, Sprite> dicSprites = new Dictionary<string, Sprite>();

		/// <summary>スプライトリスト.シリアライズ/デシリアライズに利用され、dicSpriteを生成します.</summary>
		public List<Sprite> sprites { get { return m_Values; } }
		[SerializeField] List<Sprite> m_Values = new List<Sprite>();
		[SerializeField] List<string> m_Keys = new List<string>();

		/// <summary>
		/// アトラスにスプライトが含まれているかどうかを返します.
		/// </summary>
		/// <param name="spriteName">スプライト名.</param>
		public bool Contains(string spriteName)
		{
			return string.IsNullOrEmpty(spriteName) ? false : dicSprites.ContainsKey(spriteName);
		}

		/// <summary>
		/// スプライトを取得します.
		/// </summary>
		/// <param name="spriteName">スプライト名.</param>
		public Sprite GetSprite(string spriteName)
		{
			return Contains(spriteName) ? dicSprites[spriteName] : null;
		}

		private Atlas() { }

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			dicSprites = new Dictionary<string, Sprite>(Mathf.Max(m_Keys.Count, m_Values.Count));
			for (int i = 0; i < m_Values.Count && i < m_Keys.Count; i++)
			{
				if (!string.IsNullOrEmpty(m_Keys[i]) && m_Values[i] != null)
					dicSprites[m_Keys[i]] = m_Values[i];
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// アトラスに割り当てられたテクスチャを更新します.
		/// </summary>
		public void UpdateTexture()
		{
			//アトラステクスチャ内のスプライトを全てシリアライズ.
			m_Values = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(m_AtlasTexture))
					.OfType<Sprite>()
					.ToList();
			m_Keys = m_Values
				.Select(x => x.name)
				.ToList();
			dicSprites.Clear();
			EditorUtility.SetDirty(this);
			OnAfterDeserialize();
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// アトラスに割り当てられたテクスチャを更新します.
		/// </summary>
		/// <param name="atlasTexture">アトラスに割り当てるテクスチャ(MultipleSprite).</param>
		public void UpdateTexture(Texture2D atlasTexture)
		{
			m_AtlasTexture = atlasTexture;
			UpdateTexture();
		}
#endif
	}
}