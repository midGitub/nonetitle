using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using CitrusFramework;

//[ExecuteInEditMode]
public class TextureLoader : MonoBehaviour
{
	private class CManagedTexture
	{
		public Texture2D Texture { get; set; }
		public int UsageCount { get; set; }
	}

	private static Hashtable ms_oTextureCache = new Hashtable();

	public TextAsset m_RGBBytesFile;
	public TextAsset m_AlphaBytesFile;
	public Material m_EmptyMaterial;
	public TextureFormat m_TexFormat = TextureFormat.RGB24;

	Texture2D m_LoadedTexture;
	Material m_Material;

	///----------------------------------------------------------------------------
	#if UNITY_EDITOR
	int m_nRGBBytesHash;
	int m_nAlphaBytesHash;
	#endif
	///----------------------------------------------------------------------------

	void Awake()
	{
		CreateAndReplaceMaterial();
	}

	///----------------------------------------------------------------------------
	
	void CreateAndReplaceMaterial()
	{
		GameDebug.Warning(m_EmptyMaterial != null, "No empty material set!");
		if (m_EmptyMaterial != null && m_Material == null)
		{
			m_Material = Material.Instantiate(m_EmptyMaterial) as Material;
			GameDebug.Assert(m_Material != null, "Failed to create an instance of the empty material!");
		}

		GameDebug.Warning(m_Material != null, "No material created!");
		LoadTexture();

		ReplaceEmptyMaterial();
	}

	///----------------------------------------------------------------------------

	void OnDestroy()
	{
		if (m_Material != null)
		{
#if UNITY_EDITOR
			DestroyImmediate(m_Material);
#else
			Destroy(m_Material);
#endif
			m_Material = null;
		}
		FreeTexture();
	}

	///----------------------------------------------------------------------------

	public void ReplaceEmptyMaterial()
	{
		GameDebug.Assert(m_Material != null, "No material created for ZTextureLoader.");

		var oRenderer = GetComponent<MeshRenderer>();		//only 3d
		if (oRenderer != null)
		{
			oRenderer.sharedMaterial = m_Material;
		}

		var img = GetComponent<Image>();					//2d
		if (img != null)
		{
			img.material = m_Material;
		}
		GameDebug.Assert((oRenderer != null) || (img != null), "Failed to find a MeshRenderer or Image component.");
	}


	///----------------------------------------------------------------------------

	private void LoadTexture()
	{
		//GameDebug.Warning(m_RGBBytesFile != null, "No RGB texture set!");
		if (m_Material != null && m_RGBBytesFile != null)
		{
			m_LoadedTexture = LoadTexture(m_RGBBytesFile, m_AlphaBytesFile);
			m_LoadedTexture.wrapMode = TextureWrapMode.Clamp;
			m_Material.mainTexture = m_LoadedTexture;
			//m_Material.SetTexture("_MainTexture", m_LoadedTexture);

			#if UNITY_EDITOR
			m_nRGBBytesHash = m_RGBBytesFile.bytes.GetHashCode();

			if (m_AlphaBytesFile != null)
			{
				m_nAlphaBytesHash = m_AlphaBytesFile.bytes.GetHashCode();
			}
			#endif
		}
	}

	Texture2D LoadTexture(TextAsset oRGBBytes, TextAsset oAlphaBytes)
	{
		string sName;

		if (oAlphaBytes == null)
		{
			sName = oRGBBytes.name;
		}
		else
		{
			sName = oRGBBytes.name + "/" + oAlphaBytes.name;
		}

		if (ms_oTextureCache.ContainsKey(sName))
		{
			var oMT = (CManagedTexture)ms_oTextureCache[sName];
			oMT.UsageCount++;
			return oMT.Texture;
		}
		else
		{
			var oTex = new Texture2D(1, 1, m_TexFormat, false);

			if (oAlphaBytes == null)
			{
				oTex.LoadImage(oRGBBytes.bytes);
				var aTex = oTex.GetPixels32();
				oTex.Resize(oTex.width, oTex.height, oTex.format, false);
				oTex.SetPixels32(aTex);
				oTex.Apply(false, false);
			}
			else
			{
				oTex.LoadImage(oAlphaBytes.bytes);

				var rgb = new Texture2D(1, 1, m_TexFormat, false);
				rgb.LoadImage(oRGBBytes.bytes);

				GameDebug.Assert(oTex.width == rgb.width && oTex.height == rgb.height, "Alpha and RGB sizes do not match!");

//				var oStart = DateTime.Now;

				var aTex = oTex.GetPixels32();
				var aRGB = rgb.GetPixels32();

				for (int i = 0; i < aTex.Length; ++i)
				{
					var c = aTex[i];
					var a = c.r;
					c = aRGB[i];
					c.a = a;
					aTex[i] = c;
				}
				oTex.Resize(oTex.width, oTex.height, oTex.format, false);
				oTex.SetPixels32(aTex);

//				var oTime = DateTime.Now - oStart;
//				Debug.Log("Time: " + oTime.TotalMilliseconds + "ms");

#if UNITY_EDITOR
				UnityEngine.Object.DestroyImmediate(rgb);
#else
				UnityEngine.Object.Destroy(rgb);
#endif
				oTex.Apply(false, true);
			}

			var oMT = new CManagedTexture();
			oMT.Texture = oTex;
			oMT.UsageCount = 1;
			oTex.name = sName;
			ms_oTextureCache.Add(sName, oMT);

#if DEBUG
			var oSB = new System.Text.StringBuilder();
			oSB.Append("<color=magenta>loading texture ");
			oSB.Append(oRGBBytes.name);
			if (oAlphaBytes != null)
			{
				oSB.Append("+");
				oSB.Append(oAlphaBytes.name);
			}
			oSB.Append(": ");
			oSB.Append(" ms</color>");
			GameDebug.Log("TexLoader", "", oSB.ToString());
#endif
			return oTex;
		}
	}

	///----------------------------------------------------------------------------

	void FreeTexture(Texture2D oTex)
	{
		if (oTex != null)
		{
			var sName = oTex.name;
			var oMT = (CManagedTexture)ms_oTextureCache[sName];
			if (oMT != null)
			{
				oMT.UsageCount--;
				if (oMT.UsageCount == 0)
				{
					ms_oTextureCache.Remove(sName);

#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(oTex);
#else
					UnityEngine.Object.Destroy(oTex);
#endif
				}
			}
		}
	}

	private void FreeTexture()
	{
		if (m_LoadedTexture != null)
		{
			FreeTexture(m_LoadedTexture);
			m_LoadedTexture = null;
		}
	}

	///----------------------------------------------------------------------------

#if UNITY_EDITOR

	void OnEnable()
	{
		if (UnityEditor.EditorApplication.isPlaying)
			return;	// don't handle OnEnable when playing the game in the editor

		Renderer oRenderer = GetComponent<Renderer>();
		if (oRenderer != null && oRenderer.sharedMaterial == null)
		{
			// when the component is first added to the object, Awake is called before an Empty Material could be set => m_Material was not created
			// this code will make sure everything gets created and the Preview works when the changes are applied to the prefab
			CreateAndReplaceMaterial();
		}
		else if (IsChanged())
		{
			FreeTexture();
			LoadTexture();
		}
	}

	///----------------------------------------------------------------------------

	private bool IsChanged()  
	{
		if (m_RGBBytesFile != null)
		{
			if (m_RGBBytesFile.bytes.GetHashCode() != m_nRGBBytesHash)
			{
				return true;
			}
		}

		if (m_AlphaBytesFile != null)
		{
			if (m_AlphaBytesFile.bytes.GetHashCode() != m_nAlphaBytesHash)
			{
				return true;
			}
		}

		return false;
	}

#endif
}
