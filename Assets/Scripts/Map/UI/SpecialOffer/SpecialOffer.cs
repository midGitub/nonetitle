using UnityEngine;
using UnityEngine.UI;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;

public class SpecialOffer : MonoBehaviour {
	// Use this for initialization
	public Button ExitButton;
	public Button BuyButton;
	public Canvas ThisCanvas;
	public Text OldPriceText;
	public Text NowPriceText;
	public Text CreditsText;
	public Text DisCount;
	public GameObject ThisGameObject;

	private readonly string _dollarSymbol = "$";
	private WindowInfo _windowInfoReceipt = null;
	private string _productID = "35";
	private float _oldPrice;
	private float _nowPrice;
	private ObscuredULong _credits;
	private string _disCount;
	void Start () {
		SetContent ();
		ExitButton.onClick.AddListener (ExitButtonClick);
		BuyButton.onClick.AddListener (BuyButtonClick);
		StoreManager.Instance.InitStore ();
		Show ();
	}

	void Show()
	{
		if (_windowInfoReceipt == null)
		{
			_windowInfoReceipt = new WindowInfo(Open, null, ThisCanvas, ForceToCloseImmediately);
			WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
		}
	}

	void Open()
	{
		ThisCanvas.worldCamera = Camera.main;
		ThisCanvas.gameObject.SetActive (true);
		UserBasicData.Instance.LastShowSpecialOffer = NetworkTimeHelper.Instance.GetNowTime ();
		SpecialOfferHelper.Instance.SetStoreAnalysisData();
	}

	void ForceToCloseImmediately()
	{
		ThisCanvas.gameObject.SetActive (false);
		ThisGameObject.SetActive (false);
		Destroy (ThisGameObject);
	}

	void ExitButtonClick()
	{
        PlayClickSound();
        ForceToCloseImmediately ();
		WindowManager.Instance.TellClosed (_windowInfoReceipt);
	}

	void BuyButtonClick()
	{
		Buy ();
        PlayClickSound();
		ExitButtonClick ();
    }

	void Buy()
	{
		StoreManager.Instance.InitiatePurchase (_productID);
	}

    void PlayClickSound()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);
    }

    void SetContent()
	{
		var item = IAPCatalogConfig.Instance.FindIAPItemByID (_productID);
		_oldPrice = item.OldPrice;
		_nowPrice = item.Price;
		_credits = (ulong)item.CREDITS;
		_disCount = item.Preferential;
		NowPriceText.text = StoreManager.Instance.GetPriceString (item);
		OldPriceText.text = string.Format ("<color=#d11425>{0}</color>", "was:") + string.Format ("<color=#d28aff>{0}</color>", UpdateOldPrice(NowPriceText.text,_nowPrice,_oldPrice));
		CreditsText.text = (_credits/1000).ToString () + ",000";
		float result = 0;
		float.TryParse (_disCount,out result);
		int dis = (int)(result * 100.0);
		DisCount.text = dis.ToString()+"%";
	}

	private string UpdateOldPrice(string currentPriceStr, float localPrice, float oldPriceAsDoller){
		int front = currentPriceStr.Length;
		int end = -1;
		string currencySymbolfront = _dollarSymbol , currencySymbolend = "",result;
		StringBuilder stringbuilder = new StringBuilder ();
		for (int i = 0, count = 0; i < currentPriceStr.Length; i++)
			if ((currentPriceStr [i] >= '0' && currentPriceStr [i] <= '9')) 
			{
				front = Mathf.Min (front, i);
				end = i;
				stringbuilder.Append (currentPriceStr [i]);
			} 
			else if (currentPriceStr [i] == '.') 
			{
				if (count++ == 0)
					stringbuilder.Append (currentPriceStr [i]);
				else
					Debug.Assert (false, "price has more than one . symbol");
			}
		
		if(front != currentPriceStr.Length)
			currencySymbolfront = currentPriceStr.Substring (0, front);
		if (end != -1)
			currencySymbolend = currentPriceStr.Substring (end + 1, currentPriceStr.Length - end - 1);
		if (end >= 0 && currencySymbolfront != _dollarSymbol && localPrice != 0) 
		{
			string priceStr = stringbuilder.ToString();
			float price = System.Convert.ToSingle (priceStr);
			price =  price / localPrice * oldPriceAsDoller;
			result = currencySymbolfront + ((int)price).ToString ("f2") + currencySymbolend;
		}
		else 
			result = currencySymbolfront + oldPriceAsDoller;
		return result;
	}   
}
