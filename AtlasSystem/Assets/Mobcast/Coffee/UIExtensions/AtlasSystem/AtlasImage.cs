using UnityEngine;
using UnityEngine.UI;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Atlasを利用できるImageクラスです.
	/// AtlasはMultipleSpriteを持つアセットで、AtlasImageはその中のSpriteをアセット名からアクセスできます.
	/// </summary>
	[AddComponentMenu("UI/Atlas Image")]
	public class AtlasImage : Image
	{
		/// <summary>スプライト名.アトラス内に同じ名前のSpriteがない場合、AtlasImageはデフォルトスプライトを表示します.</summary>
		public string spriteName { get { return m_SpriteName; } set { if (m_SpriteName != value){m_SpriteName = value; SetAllDirty(); } } }
		[SerializeField] private string m_SpriteName;

		/// <summary>アトラス.AtlasMakerで作成されたアトラスアセットを取得・設定します.</summary>
		public Atlas atlas { get { return m_Atlas; }set { if (m_Atlas != value){m_Atlas = value; SetAllDirty(); } } }
		[SerializeField] private Atlas m_Atlas;

		protected AtlasImage() : base()
        {}

		/// <summary>最後に更新されたスプライト名.</summary>
		string lastSpriteName = "";


		/// <summary>
		/// Sets the material dirty.
		/// </summary>
		public override void SetMaterialDirty()
		{
			//Animationからスプライト変更するための処理.
			//Animationやスクリプトによって、「sprite」を変更した場合、スプライト名に反映.
			if (lastSpriteName == spriteName)
			{
				m_SpriteName = sprite ? sprite.name : "";
			}
			//「スプライト名」が変更された場合、アトラスからスプライトを取得してスプライトに反映.
			else
			{
				sprite = atlas ? atlas.GetSprite(spriteName) : null;
			}

			lastSpriteName = spriteName;

			base.SetMaterialDirty();
		}
	}
}
