using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class DynamicScrollviewController<DataType, UIBehaviour> : MonoBehaviour 
	where DataType : class 
	where UIBehaviour : MonoBehaviour{
	// 面板对象
	public ScrollRect _scrollRect;
	// 控件原型
	public GameObject _itemPrototype;
	// 可视区间里存在的控件实体数
	public int AREA_VIEW_NUM_MAX = 15;
	// 控件数阈值（例如，面板往上拖拽，超过5个控件的距离后，每次拖拽一个控件，则会把之前最上面的那个控件位置挪到队列最下面，然后用新数据更新它）
	public int AREA_UPDATE_NUM_THRESHOLD = 5;
	// 控件偏移
	public float ITEM_OFFSET = 10.0f;

	// 数据列表
	protected List<DataType> _dataList = new List<DataType>();
	// 控件列表
	protected List<UIBehaviour> _viewAreaItemBehaviourList = new List<UIBehaviour> ();
	// 数据总数
	protected int _dataListCount = 0;
	// 控件高度
	protected float _itemHeightDelta;
	// 默认的content大小
	protected Vector2 _contentDefaultDeltaSize;
	protected Vector2 _defaultNormalizePosition;
	// 面板中能容纳的item数
	protected int _itemInAreaNum = 0;
	// 单一控件占据面板content的比值
	protected float _normalizePositionDeltaY = 0;
	// 初始更新索引时的阈值， 和AREA_UPDATA_NUM_THRESHOLD相关
	protected float _upperThreshold = 0;
	// 上一次的更新索引
	protected int _lastIndex = -1;
	// 初始化标记
	protected bool _init = false;

	public int LastIndex
	{
		get
		{
			return _lastIndex;
		}
	}

	public List<DataType> DataList {
		get { return _dataList; }
	}

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	protected virtual void Update () {
		if (!_init)
			return;
		
		int currentIndex = (int)Mathf.Ceil((_upperThreshold - _scrollRect.normalizedPosition.y) / _normalizePositionDeltaY);
		if (_dataListCount - AREA_VIEW_NUM_MAX > 0)
			currentIndex = Mathf.Clamp (currentIndex, 0, _dataListCount - AREA_VIEW_NUM_MAX);
		else
			currentIndex = 0;

//		Debug.Log ("current Index = "+currentIndex);
//		Debug.Log ("normalizePos" + _scrollRect.normalizedPosition);
	
		UpdateItems (currentIndex);

		_lastIndex = currentIndex;
	}

	public void InitScrollView(List<DataType> dataList){
		InitDataList (dataList);

		Init ();

		RectTransform itemRect = _itemPrototype.GetComponent<RectTransform> ();
		_itemHeightDelta = itemRect.sizeDelta.y + ITEM_OFFSET;
//		Debug.Log ("item height is "+_itemHeightDelta);

		_itemInAreaNum = (int)(_scrollRect.GetComponent<RectTransform> ().sizeDelta.y / _itemHeightDelta);

		_contentDefaultDeltaSize = _scrollRect.content.sizeDelta;
		_defaultNormalizePosition = new Vector2(_scrollRect.normalizedPosition.x, 1.0f);

		RefreshContentHeight ();
		RefreshThreshold ();

		InitAllItems ();

		_lastIndex = 0;
		_init = true;

		DefaultProcess ();
	}

	public void InitDataList(List<DataType> dataList){
		// 数据列表初始化
		_dataList.Clear ();
		ListUtility.ForEach (dataList, (DataType data) => {
			_dataList.Add(data);
		});
		_dataListCount = _dataList.Count;
	}

	private void InitAllItems(){
		// 控件列表初始化
		ListUtility.ForEach (_viewAreaItemBehaviourList, (UIBehaviour behaviour) => {
			if(behaviour != null) DestroyImmediate(behaviour.gameObject);
		});
		_viewAreaItemBehaviourList.Clear ();

		for (int i = 0; i < AREA_VIEW_NUM_MAX; ++i) {
			GameObject obj = UGUIUtility.CreateObj(_itemPrototype, _itemPrototype.transform.parent.gameObject);
			obj.name += i.ToString ();
			UIBehaviour item = obj.GetComponent<UIBehaviour> ();
			_viewAreaItemBehaviourList.Add (item);
			SetItem (item, i);
			ItemHandlerInit (item);
		}

		_itemPrototype.SetActive (false);
	}

	public void SetItem(UIBehaviour item, int index){
		item.gameObject.transform.localPosition = _itemPrototype.transform.localPosition - index * new Vector3 (0, _itemHeightDelta, 0);

		if (index < _dataListCount) {
			ItemUpdateFunc (item, _dataList [index]);
			item.gameObject.SetActive (true);
		} else {
			item.gameObject.SetActive (false);
		}
	}

	private void UpdateItems(int index){
		if (_lastIndex == index)
			return;

		int offset = Mathf.Abs (index - _lastIndex);
		
		for (int i = 0; i < offset; ++i) {
			// 把首控件娜到队列尾部，并更新数据
			if (index > _lastIndex) {
				UIBehaviour item = _viewAreaItemBehaviourList [0];
				item.gameObject.transform.localPosition = 
					_viewAreaItemBehaviourList [AREA_VIEW_NUM_MAX - 1].gameObject.transform.localPosition - new Vector3 (0, _itemHeightDelta, 0);

				_viewAreaItemBehaviourList.Remove (item);
				_viewAreaItemBehaviourList.Add (item);
				ItemUpdateFunc(item, _dataList [_lastIndex + i + AREA_VIEW_NUM_MAX]);
			} else {
				// 把尾部控件挪到队列首
				UIBehaviour item = _viewAreaItemBehaviourList [AREA_VIEW_NUM_MAX - 1];
				item.gameObject.transform.localPosition = 
					_viewAreaItemBehaviourList [0].gameObject.transform.localPosition + new Vector3 (0, _itemHeightDelta, 0);

				_viewAreaItemBehaviourList.Remove (item);
				_viewAreaItemBehaviourList.Insert (0, item);
				ItemUpdateFunc (item, _dataList [_lastIndex - i - 1]);
			}
		}
	}

	public virtual void DataUpdateList(List<DataType> dataList, bool changeScrollView){
		InitDataList (dataList);

		_lastIndex = 0;
		RefreshContentHeight ();
		if (changeScrollView) {
			_scrollRect.normalizedPosition = _defaultNormalizePosition;
		}

		ItemUpdateList ();
	}

	public virtual void ItemUpdateList(int startIndex = 0){
		for (int i = 0; i < AREA_VIEW_NUM_MAX; ++i) {
			if (i < _viewAreaItemBehaviourList.Count) {
				SetItem (_viewAreaItemBehaviourList [i], startIndex + i);
			}
		}
	}

	private void RefreshLastIndex(int startIndex){	// 正常区间
		if (startIndex + AREA_VIEW_NUM_MAX > _dataListCount) {
			// 需要调整 lastindex
			int offset = _dataListCount - (startIndex + AREA_VIEW_NUM_MAX);
			_lastIndex = startIndex + offset;
			_lastIndex = Mathf.Clamp (_lastIndex, 0, _dataListCount - AREA_VIEW_NUM_MAX);
		} 
	}

	private float RefreshContentHeight(){
		float oldY = _scrollRect.content.sizeDelta.y;

		Vector2 deltaSize = new Vector2(_contentDefaultDeltaSize.x, _dataListCount * _itemHeightDelta);
		_scrollRect.content.sizeDelta = deltaSize;

		if (deltaSize.y == 0.0f)
			return 0.0f;
		else
			return oldY / deltaSize.y;
	}

	private void RefreshThreshold(){
		if (_dataListCount == 0)
			_normalizePositionDeltaY = 1.0f;
		else {
			_normalizePositionDeltaY = 1.0f / _dataListCount;
		}
		_upperThreshold = 1.0f - AREA_UPDATE_NUM_THRESHOLD * _normalizePositionDeltaY;
	}

	private void RefreshNormalizePosition(int startIndex, float ratio){
		float normalizY;
		if (_dataListCount <= _itemInAreaNum) {
			normalizY = 1.0f;
		}
		else{
			normalizY = _scrollRect.normalizedPosition.y;
		}
		
		normalizY = Mathf.Clamp (normalizY, 0.0f, 1.0f);

		Vector2 deltaPos = new Vector2 (_scrollRect.normalizedPosition.x, normalizY);
		_scrollRect.normalizedPosition = deltaPos;
	}

	public virtual void DeleteItemList(List<DataType> dataList){
		_dataList = ListUtility.SubtractList (_dataList, dataList);
		_dataListCount = _dataList.Count;

		// 更新lastindex
		RefreshLastIndex (_lastIndex);
		// content的高度调整
		float ratio = RefreshContentHeight();

		RefreshThreshold ();
		// normalizePositionY的调整
		RefreshNormalizePosition(_lastIndex, ratio);

		ItemUpdateList (_lastIndex);
	}

	protected virtual void ItemUpdateFunc(UIBehaviour item, DataType data){
		// user code
	}

	protected virtual void DefaultProcess(){
		// user code
	}

	protected virtual void Init(){
		// user code
	}

	protected virtual void ItemHandlerInit(UIBehaviour item){
		// user code
	}
}
