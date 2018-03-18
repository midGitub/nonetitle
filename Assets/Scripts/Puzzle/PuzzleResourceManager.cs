using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleResourceManager
{
	private static readonly string _symbolImageDir = "Images/Symbols/"; //todo
	private static float _symbolBlurSpriteHeightFactor = 2.0f;
	private static float _normalBlurFactor = 1.0f;
	private static float _hypeBlurFactor = 1.5f;

	private Dictionary<string, Sprite> _symbolBlurSpriteDict = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _symbolHypeBlurSpriteDict = new Dictionary<string, Sprite>();
	private PuzzleMachine _machine;
	private MachineConfig _machineConfig;

	public Dictionary<string, Sprite> SymbolBlurSpriteDict { get { return _symbolBlurSpriteDict; } }
	public Dictionary<string, Sprite> SymbolHypeBlurSpriteDict { get { return _symbolHypeBlurSpriteDict; } }

	public PuzzleResourceManager(PuzzleMachine machine, MachineConfig config)
	{
		_machine = machine;
		_machineConfig = config;

		InitSymbolBlurSpriteDicts();
	}

	private void UpdateBlurFactor(){
		float normalBlurFactor = _machineConfig.BasicConfig.NormalBlurFactor;
		float hypeBlurFactor = _machineConfig.BasicConfig.HypeBlurFactor;

		if (normalBlurFactor > 0.0f){
			_normalBlurFactor = normalBlurFactor;
		}

		if (hypeBlurFactor > 0.0f){
			_hypeBlurFactor = hypeBlurFactor;
		}
	}

	private void InitSymbolBlurSpriteDicts()
	{
		UpdateBlurFactor();

		SymbolData[] dataArray = _machineConfig.SymbolConfig.Sheet.dataArray;
		for(int i = 0; i < dataArray.Length; i++)
		{
			string filePath = _symbolImageDir + dataArray[i].ArtAsset;

			Sprite blurSprite = CreateSingleSymbolBlurSprite(filePath, _normalBlurFactor);
			_symbolBlurSpriteDict.Add(dataArray[i].Name, blurSprite);

			blurSprite = CreateSingleSymbolBlurSprite(filePath, _hypeBlurFactor);
			_symbolHypeBlurSpriteDict.Add(dataArray[i].Name, blurSprite);
		}
	}

	private Sprite CreateSingleSymbolBlurSprite(string path, float blurFactor)
	{
		Sprite sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(path, _machine.MachineName);
		if(sprite == null)
		{
			Debug.LogError("Create blur sprite fail:" + path);
			Debug.Assert(false);
		}
		int width = sprite.texture.width;
		int height = (int)(sprite.texture.height * _symbolBlurSpriteHeightFactor);

		//GenBigSprite is useless now. The current SpinBlur.shader has included the logic of GenBigSprite.
//		//1 render to RenderTexture with shader
//		RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
//		rt.filterMode = FilterMode.Bilinear;
//		Shader shader = Shader.Find("Custom/GenBigSprite");
//		Material mat = new Material(shader);
//		Graphics.Blit(sprite.texture, rt, mat);
//
//		//2 read RenderTexture back to texture
//		Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
//		RenderTexture.active = rt;
//		tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
//		tex.Apply();
//		RenderTexture.active = null;
//
//		RenderTexture.ReleaseTemporary(rt);

		Texture2D blurTex = CreateBlurTexture(sprite.texture, width, height, blurFactor);

		Sprite result = Sprite.Create(blurTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
		return result;
	}

	private Texture2D CreateBlurTexture(Texture2D tex, int width, int height, float blurFactor)
	{
		RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
		rt.filterMode = FilterMode.Bilinear;
		Material matAsset = AssetManager.Instance.LoadAsset<Material>("Shader/SpinBlur");
		Material mat = new Material(matAsset);
		float radius = mat.GetFloat("_Radius");
		radius *= blurFactor;
		mat.SetFloat("_Radius", radius);

		Graphics.Blit(tex, rt, mat);

		Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
		RenderTexture.active = rt;
		result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		result.Apply();
		RenderTexture.active = null;

		RenderTexture.ReleaseTemporary(rt);

		return result;
	}
}
