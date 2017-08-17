using UnityEngine;
using System.Collections;
using UnityEngine.Sprites;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// RendererでAtlasを表示するコンポーネント.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	public class AtlasRenderer : MonoBehaviour
	{
		/// <summary>キャッシュ済みRendererコンポーネント.</summary>
		public Renderer cachedRenderer
		{
			get
			{
				if (m_CachedRenderer == null)
					m_CachedRenderer = GetComponent<Renderer>();
				return m_CachedRenderer;
			}
		}

		Renderer m_CachedRenderer;


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

		/// <summary>最後に更新されたスプライトID.</summary>
		int lastSpriteInstanceId = 0;
		bool hasChanged = true;

		[SerializeField] Sprite sprite;

		protected AtlasRenderer() : base(){}

		MaterialPropertyBlock block;
		int _MainTex;
		int _MainTex_ST;

		/// <summary>
		/// Sets the material dirty.
		/// </summary>
		void OnWillRenderObject()
		{
			// When atlas or sprite name has changed, apply sprite to renderer.
			if (hasChanged)
			{
				ApplySprite(atlas ? atlas.GetSprite(spriteName) : null);
			}
			// When sprite has changed (maybe, by animation), apply sprite to renderer.
			else
			{
				int spriteInstanceId = sprite ? sprite.GetInstanceID() : 0;
				if (lastSpriteInstanceId != spriteInstanceId)
				{
					ApplySprite(sprite);
				}
			}

//			#if UNITY_EDITOR
//			if (!Application.isPlaying)
//			{
//				UnityEditor.EditorUtility.SetDirty(cachedRenderer);
//			}
//			#endif
		}

		void ApplySprite(Sprite sprite)
		{
			this.sprite = sprite;
			m_SpriteName = sprite ? sprite.name : "";
			lastSpriteInstanceId = sprite ? sprite.GetInstanceID() : 0;
			hasChanged = false;

			// When target renderer is SpriteRenderer, apply sprite directly.
			if (cachedRenderer is SpriteRenderer)
			{
				
				(cachedRenderer as SpriteRenderer).sprite = sprite;

			}
//			else if (cachedRenderer is ParticleSystemRenderer)
			else if (cachedRenderer is ParticleSystemRenderer)
			{
//				if (block == null)
//				{
//					block = new MaterialPropertyBlock();
//					if (_MainTex == 0)
//					{
//						_MainTex = Shader.PropertyToID("_MainTex");
//						_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
//					}
//				}
//				Vector4 uv = DataUtility.GetOuterUV(sprite);
//
//				cachedRenderer.GetPropertyBlock(block);
//				block.SetTexture(_MainTex, atlas.atlasTexture);


//				block.SetVector(_MainTex_ST, new Vector4(uv.z - uv.x, uv.w - uv.y, uv.x, uv.y));
//				cachedRenderer.SetPropertyBlock(block);

//				ParticleSystem ps;
//				ParticleSystem.Particle[] p = new ParticleSystem.Particle[3];
//
//				ps.GetParticles(p);
//				p[0].
//
//				Vector4 uv = DataUtility.GetOuterUV(sprite);

//				Mesh[] meshes = new Mesh[ (cachedRenderer as ParticleSystemRenderer).meshCount];
//								List<Vector2> result = new List<Vector2>();
//
//
//				(cachedRenderer as ParticleSystemRenderer).GetMeshes(meshes);
//
//				foreach(var m in meshes)
//				{
//					m.GetUVs(0, result);
//
//					result[0].Set(uv.x, uv.y);
//					result[1].Set(uv.z, uv.y);
//					result[2].Set(uv.x, uv.w);
//					result[3].Set(uv.z, uv.w);
//
//
//					m.SetUVs(0, result);
//				}
//
//
//				(cachedRenderer as ParticleSystemRenderer).SetMeshes(meshes);
//
//				List<Vector2> result = new List<Vector2>();
//
//				(cachedRenderer as ParticleSystemRenderer).mesh.GetUVs(0, result);
//
//				result[0].Set(uv.x, uv.y);
//				result[1].Set(uv.z, uv.y);
//				result[2].Set(uv.x, uv.w);
//				result[3].Set(uv.z, uv.w);
//
//				(cachedRenderer as ParticleSystemRenderer).mesh.SetUVs(0, result);
////				Debug.LogFormat("{0}, {1}, {2}, {3}",uvs[0],uvs[1],uvs[2],uvs[3]);
//
////				(cachedRenderer as ParticleSystemRenderer).set
//
//				(cachedRenderer as ParticleSystemRenderer).EnableVertexStreams(ParticleSystemVertexStreams.UV);
//
			}

			// When target renderer is not SpriteRenderer, apply sprite by MaterialPropertyBlock.
			else
			{
				/*
				if (block == null)
				{
					block = new MaterialPropertyBlock();
					if (_MainTex == 0)
					{
						_MainTex = Shader.PropertyToID("_MainTex");
						_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
					}
				}
				Vector4 uv = DataUtility.GetOuterUV(sprite);

				cachedRenderer.GetPropertyBlock(block);
				block.SetTexture(_MainTex, atlas.atlasTexture);
				*/
//				block.SetVector(_MainTex_ST, new Vector4(uv.z - uv.x, uv.w - uv.y, uv.x, uv.y));
//				cachedRenderer.SetPropertyBlock(block);
			}
		}

		/*
		void Update()
		{
			
			var ps = GetComponent<ParticleSystem>();

			ps.particle(customData, ParticleSystemCustomData.);

			int particleCount = ps.particleCount;
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
			ps.GetParticles(particles);

			for (int i = 0; i < particles.Length; i++)
			{
				Vector3 sPos = mainCam.WorldToScreenPoint(particles[i].position + ps.transform.position);

				// set custom data to 1, if close enough to the mouse
				if (Vector2.Distance(sPos, Input.mousePosition) < minDist)
				{
					customData[i] = new Vector4(1, 0, 0, 0);
				}
				// otherwise, fade the custom data back to 0
				else
				{
					float particleLife = particles[i].remainingLifetime / ps.main.startLifetimeMultiplier;

					if (customData[i].x > 0)
					{
						float x = customData[i].x;
						x = Mathf.Max(x - Time.deltaTime, 0.0f);
						customData[i] = new Vector4(x, 0, 0, 0);
					}
				}
			}

			ps.SetCustomParticleData(customData,  );

		}*/


		void LateUpdate()
		{
			return;

			if (cachedRenderer is ParticleSystemRenderer)
			{
				Mesh[] meshes = new Mesh[ (cachedRenderer as ParticleSystemRenderer).meshCount];
				List<Vector2> result = new List<Vector2>();

				Vector4 uv = DataUtility.GetOuterUV(sprite);

				(cachedRenderer as ParticleSystemRenderer).GetMeshes(meshes);
//				Debug.LogFormat("hogehogeX {0} : {1}, {2}, {3}, {4},", meshes.Length, uv.x ,uv.y ,uv.z ,uv.w);




				foreach(var m in meshes)
				{
					var uvs = m.uv;


					Debug.LogFormat("hogehogeX {0} : {1}, {2}, {3}, {4},", meshes.Length, uvs[0] ,uvs[1] ,uvs[2] ,uvs[3]);


					uvs[0].Set(uv.x, uv.y);
					uvs[1].Set(uv.z, uv.w);//
					uvs[2].Set(uv.z, uv.y);
					uvs[3].Set(uv.x, uv.w);

					m.uv = uvs;
				}


				(cachedRenderer as ParticleSystemRenderer).SetMeshes(meshes);
			}
		}

		#if UNITY_EDITOR
		void OnValidate()
		{
			hasChanged = true;

			if (!Application.isPlaying)
				UnityEditor.EditorUtility.SetDirty(cachedRenderer);
		}
		#endif
	}
}
