using UnityEngine;
using System.Collections;

namespace Mobcast.Coffee.UI
{
	[RequireComponent(typeof(SpriteRenderer))]
	[ExecuteInEditMode]
	public class AtlasRenderer : MonoBehaviour
	{

		/// <summary>キャッシュ済みSpriteRendererコンポーネント.</summary>
		public SpriteRenderer cachedSpriteRenderer
		{
			get
			{
				if (m_CachedSpriteRenderer == null) m_CachedSpriteRenderer = GetComponent<SpriteRenderer>();
				return m_CachedSpriteRenderer;
			}
		}

		SpriteRenderer m_CachedSpriteRenderer;


		/// <summary>スプライト名.アトラス内に同じ名前のSpriteがない場合、AtlasImageはデフォルトスプライトを表示します.</summary>
		public string spriteName
		{
			get { return m_SpriteName; }
			set
			{
				if (m_SpriteName != value)
				{
					m_SpriteName = value;
					hasChanged = true;
				}
			}
		}

		[SerializeField] private string m_SpriteName;

		/// <summary>アトラス.AtlasMakerで作成されたアトラスアセットを取得・設定します.</summary>
		public Atlas atlas
		{
			get { return m_Atlas; }
			set
			{
				if (m_Atlas != value)
				{
					m_Atlas = value;
					hasChanged = true;
				}
			}
		}

		[SerializeField] private Atlas m_Atlas;

		/// <summary>最後に更新されたスプライト名.</summary>
		string lastSpriteName = "";

		int lastSpriteInstanceId = 0;
		bool hasChanged = true;

		protected AtlasRenderer() : base(){}

		/// <summary>
		/// Sets the material dirty.
		/// </summary>
		void OnWillRenderObject()
		{
			//スプライト名が変更された場合、アトラスからスプライトを取得.
			if (hasChanged)
			{
				Sprite sprite = atlas ? atlas.GetSprite(spriteName) : null;
				cachedSpriteRenderer.sprite = sprite;
				m_SpriteName = sprite ? sprite.name : "";
				lastSpriteInstanceId = sprite ? sprite.GetInstanceID() : 0;
				hasChanged = false;
			}
			//sprite側で変更された場合、spriteNameに反映.
			//アトラス内にない場合、スプライト名は空白になります.
			else
			{
				Sprite sprite = cachedSpriteRenderer.sprite;
				int spriteInstanceId = sprite ? sprite.GetInstanceID() : 0;
				if (lastSpriteInstanceId != spriteInstanceId)
				{
					m_SpriteName = atlas && sprite && atlas.Contains(sprite.name) ? sprite.name : "";
					lastSpriteInstanceId = spriteInstanceId;
				}
			}

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.EditorUtility.SetDirty(cachedSpriteRenderer);
			#endif
		}
#if UNITY_EDITOR
		void OnValidate()
		{
			hasChanged = true;

			if (!Application.isPlaying)
				UnityEditor.EditorUtility.SetDirty(cachedSpriteRenderer);
		}
#endif
	}
}
