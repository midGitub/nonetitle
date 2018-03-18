using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System;
using System.Linq;

public class IAPCatalogConfig : SimpleSingleton<IAPCatalogConfig>
{
	public static readonly string Name = "IAPCatalog";
	static readonly string _imageDirPath = "Images/UI/Store/";

	private IAPCatalogSheet _sheet { get; set; }
	public List<IAPCatalogData> ListSheet { private set; get; }

	public IAPCatalogConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<IAPCatalogSheet>(Name);
		ListSheet = _sheet.dataArray.ToList();
	}

	public static void Reload()
	{
		Debug.Log("Reload IAPCatalogConfig");
		IAPCatalogConfig.Instance.LoadData();
	}

	public IAPCatalogData FindIAPItemByID(string ID)
	{
		var item = ListSheet.FirstOrDefault((IAPCatalogData obj) => {
			return obj.ID == ID;
		});

		#if UNITY_EDITOR
		if(item == null)
			Debug.LogError("选择的商品没有id" + ID);
		#endif

		return item;
	}

	public Sprite GetItemImageByID(string id)
	{
		Sprite s = AssetManager.Instance.LoadAsset<Sprite>(_imageDirPath + id);
		//todo by nichos: give a default value
		if(s == null)
		{
			s = AssetManager.Instance.LoadAsset<Sprite>(_imageDirPath + "4");
			Debug.LogError("GetItemImageByID: fail to get image, id:" + id);
		}
		return s;
	}

	public float GetCreditsWithoutPromotion(IAPCatalogData data)
	{
		float result = data.CREDITS;
		if(Convert.ToBoolean(data.AffectByVipLevel))
			result += data.CREDITS * VIPSystem.Instance.GetCurrVIPInforData.StoreAddition;

		return result;
	}

	public float GetCreditsWithPromotion(IAPCatalogData data)
	{
		float result = GetCreditsWithoutPromotion(data);
		if(ShouldPromoteItem(data))
			result = result * PromotionHelper.Instance.PromotedCreditFactor;
		
		return result;
	}

	public bool ShouldPromoteItem(IAPCatalogData data)
	{
		bool result = false;
		if(PromotionHelper.Instance.IsInPromotion() && IsPromotionItem(data))
			result = true;
		
		return result;
	}

	bool IsPromotionItem(IAPCatalogData data)
	{
		bool result = data.Title.Contains("MORE");
		return result;
	}

	public int GetVIPPoint(IAPCatalogData data)
	{
		int result = data.OneDLAddVIPPoint;
		if(Convert.ToBoolean(data.AffectByVipLevel))
			result *= Mathf.RoundToInt(Convert.ToSingle(data.Price));
		return result;
	}

    public Sprite GetItemImageByPrice(float price)
    {
        string pathTale;
        if (price < 10)
        {
            pathTale = "6";
        }
        else if (price < 60)
        {
            pathTale = "3";
        }
        else 
        {
            pathTale = "1";
        }
        return AssetManager.Instance.LoadAsset<Sprite>(_imageDirPath + pathTale);
    }

}
