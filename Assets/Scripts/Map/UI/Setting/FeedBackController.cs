using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using CitrusFramework;

[Serializable]
public class CanSerializableDictionaryList
{
	[Serializable]
	public class Item
	{
		public string ID;
		public List<string> Value = new List<string>();
	}

	public List<Item> ItemList = new List<Item>();

	public Item FindItemByID(string ID)
	{
		return ItemList.Find((obj) => { return obj.ID == ID; });
	}

	public bool IsExistID(string ID)
	{
		return ItemList.FirstOrDefault((arg) => { return arg.ID == ID; }) != default(Item);
	}
}

public class FeedBackController : MonoBehaviour
{
	public Dropdown DropDownFirst;
	public Dropdown DropDownChild;
	public GameObject LastGameObject;
	public InputField EmailInputF;
	public InputField InputF;
	public GameObject ASKEmail;
	public Image AskImage;
	public Text AskText;
	public float ShowTime = 1;
	public float StayTime = 2;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Canvas _canvas;

	private static string currEmail = "";

	public CanSerializableDictionaryList FeedBackSelectItem = new CanSerializableDictionaryList();

    private WindowInfo _windowInfoReceipt;

	public void Awake()
	{
		//Init();
		EmailInputF.onValueChanged.AddListener((arg0) => { currEmail = arg0; });
	}

	public void OnEnable()
	{
		Init();
		InputF.text = string.Empty;
		ShowDefultItem(DropDownFirst);
		if(currEmail != null)
		{
			EmailInputF.text = currEmail;
		}
	}

	public void Init()
	{
		InitFistDrop();
	}

	private void InitFistDrop()
	{
        DropDownFirst.options.Clear();
        foreach (var item in FeedBackSelectItem.ItemList)
		{
			Dropdown.OptionData dod = new Dropdown.OptionData(item.ID);
			DropDownFirst.options.Add(dod);
		}
		DropDownFirst.onValueChanged.AddListener(UpdateDropDownChild);
	}

	private void ShowDefultItem(Dropdown dd)
	{
		Dropdown.OptionData dod = new Dropdown.OptionData("Select issue type");
		dd.options.Add(dod);
		dd.value = dd.options.Count - 1;
		dd.RefreshShownValue();
		dd.options.RemoveAt(dd.options.Count - 1);
	}

	private void UpdateDropDownChild(int indix)
	{
		DropDownChild.onValueChanged.RemoveListener(ShowInputF);
		DropDownChild.ClearOptions();
		var idItem = DropDownFirst.options[indix];
		if(idItem == null || idItem.text == "Select issue type")
		{
			DropDownChild.gameObject.SetActive(false);
			ShowInputF(-1);
			return;
		}

		var item = FeedBackSelectItem.FindItemByID(idItem.text);
		if(item != null && item.Value != null && item.Value.Count > 0)
		{
			DropDownChild.gameObject.SetActive(true);
			foreach(var i in item.Value)
			{
				Dropdown.OptionData dod = new Dropdown.OptionData(i);
				DropDownChild.options.Add(dod);
			}
			ShowDefultItem(DropDownChild);
			DropDownChild.onValueChanged.AddListener(ShowInputF);
		}
		else
		{
			DropDownChild.gameObject.SetActive(false);
			ShowInputF(indix);
		}
	}

	private void ShowInputF(int i)
	{
		if(i >= 0)
		{
			LastGameObject.SetActive(true);
		}
		else
		{
			LastGameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		DropDownFirst.onValueChanged.RemoveListener(UpdateDropDownChild);
	}

	public void SendMessage()
	{
		string mail = EmailInputF.text;
		if(mail == "")
		{
			Debug.Log("显示警告");
			ShowAsk();
			return;
		}
		string titile = "FeedBack";
		string l1 = DropDownFirst.options[DropDownFirst.value].text;
		string l2 = "";
		string l3 = "";
		if(DropDownChild.IsActive())
		{
			l2 = DropDownChild.options[DropDownChild.value].text;
		}

		l3 = InputF.text;
		Debug.Log("邮箱" + mail);
		Debug.Log("反馈" + l1 + l2 + l3);
		NetworkTimeHelper.Instance.StartCoroutine(SendMessageNetWork.crFeedback(titile, l1, l2, l3, mail));
		Hide ();
		gameObject.SetActive(false);
	}

	public void ShowAsk()
	{
		ASKEmail.SetActive(true);
		StartCoroutine(ScriptEffect.FadeInAndOut(this, (co) =>
		{
			Color ca = (AskImage.color); ca.a = co; AskText.color = ca;
		}, ShowTime, StayTime, 0f, 1,
												 () => { ASKEmail.SetActive(false); }));
	}

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    public void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);   
            _windowInfoReceipt = null;
        });
    }

    void OnDestroy()
    {
        if (_windowInfoReceipt != null)
        {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void SelfClose(Action callBack)
    {
        gameObject.SetActive(false);
        callBack();
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_canvas, _exitButton.gameObject))
        {
            gameObject.SetActive(false);
            _windowInfoReceipt = null;
            callBack(true);
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }
}

