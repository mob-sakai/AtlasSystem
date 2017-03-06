#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

[ExecuteInEditMode]
public class AtlasParticleRender : MonoBehaviour {



	public int tileX=1;
	public int tileY=1;
	public int frame=0;


	void OnEnable()
	{


		//Debug.Log(r.material.mainTexture);

	}

	void OnValidate()
	{
//		ParticleSystemRenderer r= GetComponent<ParticleSystemRenderer>();
//		ParticleSystem p = GetComponent<ParticleSystem>();


		SerializedObject so = new SerializedObject(GetComponent<ParticleSystem>());
		so.Update();

		//Debug.Log(so.FindProperty("UVModule"));
		so.FindProperty("UVModule").FindPropertyRelative("frameOverTime").FindPropertyRelative("scalar").floatValue=0;
		so.FindProperty("UVModule").FindPropertyRelative("frameOverTime").FindPropertyRelative("minMaxState").intValue=0;

//		int tileSize = tileX * tileY;
		//float fr = (float)frame / (tileSize - 1);
		//float v = (float)fr * (tileSize-1) / (tileSize);
		so.FindProperty("UVModule").FindPropertyRelative("startFrame").FindPropertyRelative("scalar").floatValue= (float)frame / (tileX * tileY);
		so.FindProperty("UVModule").FindPropertyRelative("startFrame").FindPropertyRelative("minMaxState").intValue=0;

		so.FindProperty("UVModule").FindPropertyRelative("tilesX").intValue=tileX;
		so.FindProperty("UVModule").FindPropertyRelative("tilesY").intValue=tileY;

		so.FindProperty("UVModule").FindPropertyRelative("animationType").intValue=(int)ParticleSystemAnimationType.WholeSheet;


		so.ApplyModifiedProperties();
	}
}
#endif