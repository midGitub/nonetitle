using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BillBoard :MonoBehaviour{
	public GameObject Content;
	public Image DotImage;
	public Button DotButtonLeft;
	public Button DotButtonRight;
	public GameObject ViewPort;
	public GameObject ScrollView;

	private ScrollRect scrollRect;
	private int _currentButtonIndex = 0;
	private bool isDrag;
	private int targetPos;
	private float eps = 0.001f;
	private DateTime _lastTime;
	private int _defaultSecond = 3;
	private bool _moveDown;
	private BillBoardScroll _billBoardScroll;
	List<BillBoardBase> _billBoardBaseList= new List<BillBoardBase>();
	List<Button> _buttonList= new List<Button>();

	public int Count{
		get {
			if (_billBoardBaseList != null){
				return _billBoardBaseList.Count;
			}
			return 0;
		}
	}

	void Start()
	{
		string str = MapSettingConfig.Instance.Read("DefaultScrollSecond","3");
		_defaultSecond = int.Parse(str);
		scrollRect = ScrollView.GetComponent<ScrollRect>();
		ShowUI();
		targetPos = 0;
		_billBoardScroll = ScrollView.GetComponent<BillBoardScroll>();
		_billBoardScroll.OnBeginDragEvent.AddListener(OnBeginDrag);
		_billBoardScroll.OnEndDragEvent.AddListener(OnEndDrag);
	}

	void OnDestroy()
	{
		if (_billBoardScroll != null)
		{
			_billBoardScroll.OnBeginDragEvent.RemoveListener(OnBeginDrag);
			_billBoardScroll.OnEndDragEvent.RemoveListener(OnEndDrag);
		}
	}

	void Update()
	{
		if (isDrag)
		{
			_lastTime = System.DateTime.Now;
		}
		else if (_moveDown)
		{
			if ((System.DateTime.Now - _lastTime).TotalSeconds > _defaultSecond)
			{
				targetPos += 1;
				_currentButtonIndex = (_currentButtonIndex + 1) % _billBoardBaseList.Count;
				_lastTime = System.DateTime.Now;
				_moveDown = false;
			}
		}
		else
		{
			int count = _billBoardBaseList.Count - 1;
			if (targetPos == 0 && _billBoardBaseList.Count >= 3)
			{
				ViewPort.transform.GetChild(ViewPort.transform.childCount - 1).SetAsFirstSibling();
				scrollRect.horizontalNormalizedPosition += 1.0f / count;
				targetPos = 1;
			}
			else if (targetPos >= count && _billBoardBaseList.Count >= 3)
			{
				ViewPort.transform.GetChild(0).SetAsLastSibling();
				scrollRect.horizontalNormalizedPosition -= 1.0f / count;
				targetPos = count - 1;
			}

			if(_billBoardBaseList.Count <= 2)
			{
				targetPos %= 2;
			}

			float x = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, (float)(targetPos * 1.0/(float)count), 0.3f);
			if(Mathf.Abs(x - scrollRect.horizontalNormalizedPosition) < eps)
			{
				scrollRect.horizontalNormalizedPosition = (float)(targetPos * 1.0/(float)count);
				_moveDown = true;
				_lastTime = System.DateTime.Now;
				SetDotImage();
			}
			else
				scrollRect.horizontalNormalizedPosition = x;
		}
			
	}

	public void OnEndDrag()
	{
		int count = _billBoardBaseList.Count - 1;
		float posX = scrollRect.horizontalNormalizedPosition;
		int lastpos = targetPos;
		if(scrollRect.velocity.x<0)
			targetPos = Mathf.FloorToInt(((posX + 1.0f * 3/count / 4)* count ));
		else
			targetPos = Mathf.FloorToInt(((posX + 1.0f /count / 4)* count ));
		targetPos = Mathf.Clamp(targetPos, 0, count);
		isDrag = false;
		_moveDown = false;
		_currentButtonIndex = (targetPos - lastpos + _currentButtonIndex + _billBoardBaseList.Count) %(_billBoardBaseList.Count);
	}



	public void OnBeginDrag()
	{
		isDrag = true;
	}
		
	public bool Add(BillBoardBase billboardbase)
	{
		bool result = !_billBoardBaseList.Contains(billboardbase);
		if (result)
		{
			AddButton();
			_billBoardBaseList.Add(billboardbase);
			billboardbase.SetParent();
			ShowUI();
		}
		return result;
	}

	public bool Delete(BillBoardBase billboardbase)
	{
		bool result = _billBoardBaseList.Contains(billboardbase);
		if (result)
		{
			DeleteButton(_billBoardBaseList.IndexOf(billboardbase));
			_billBoardBaseList.Remove(billboardbase);
			billboardbase.Remove();
			ShowUI();
		}
		return result;
	}

	public bool NoBillExist()
	{
		return _billBoardBaseList.Count == 0;
	}

	void ShowUI()
	{
		if (_billBoardBaseList.Count > 0) 
		{
			PlaceButton();
			UpdateBill();
		}
		RectTransform rect = ViewPort.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(Content.GetComponent<RectTransform>().sizeDelta.x*_billBoardBaseList.Count,rect.sizeDelta.y);
	}

	void UpdateBill()
	{
		for (int i = 0; i < _billBoardBaseList.Count; i++)
		{
			_billBoardBaseList [i].Show();
		}
	}

	void AddButton()
	{
		Button button = Instantiate(DotButtonLeft, DotButtonLeft.transform.parent);
		_buttonList.Add (button);
		AddButtonFucntion(button, _buttonList.Count - 1);
	}

	void DeleteButton(int index)
	{
		_buttonList [index].gameObject.SetActive(false);
		_buttonList.RemoveAt(index);
		if (_currentButtonIndex == index)
			_currentButtonIndex = 0;
		else if (_currentButtonIndex > index)
			_currentButtonIndex -= 1;
		if (_buttonList.Count > 1)
			SetDotImage();
		else
			DotImage.gameObject.SetActive(false);
	}
		
	void PlaceButton()
	{
		int count = _buttonList.Count;
		float width = DotButtonLeft.GetComponent<RectTransform>().sizeDelta.x;
		float space = DotButtonRight.transform.localPosition.x - DotButtonLeft.transform.localPosition.x - width;
		float left = 0;
		if ((count & 1) != 0)
			left -= count / 2 * (space + width) + width / 2;
		else
			left -= count / 2 * (space + width) - space / 2;
		if (count == 1)
			_buttonList [0].gameObject.SetActive(false);
		else
		{
			for (int i = 0; i < count && count > 1; i++) 
			{
				Vector3 position = _buttonList [i].transform.localPosition;
				_buttonList [i].transform.localPosition = new Vector3 (left + i * (space + width) + width/2, position.y, position.z);
				_buttonList [i].gameObject.SetActive(true); 
			}
		}
		if (count > 1)
			SetDotImage();
	}

	void AddButtonFucntion(Button button,int index)
	{
		button.onClick.AddListener(() =>
		{
			this.ButtonClick(index);
		});
	}
		
	void ButtonClick(int index)
	{
		targetPos += (index - _currentButtonIndex + _buttonList.Count) % _buttonList.Count;
		targetPos %= _buttonList.Count;
		_moveDown = false;
		_currentButtonIndex = index;
		SetDotImage();
	}

	void SetDotImage()
	{
		DotImage.transform.localPosition = _buttonList [_currentButtonIndex].transform.localPosition;
		Vector3 position = DotImage.transform.localPosition;
		position.z -= 1;
		DotImage.transform.localPosition = position;
		DotImage.gameObject.SetActive(true);
	}
}
