using System;
using System.Collections.Generic;
using System.Text;
using CitrusFramework;
using MiniJSON;
using UnityEngine;

public class PropertyTrackManager : Singleton<PropertyTrackManager>
{
    private PropertyTrackFreeQueue<PropertyTrackFreeNode> _freeQueue = new PropertyTrackFreeQueue<PropertyTrackFreeNode>();
    private PropertyTrackFreeWinQueue<PropertyTrackFreeWinNode> _freeWinQueue = new PropertyTrackFreeWinQueue<PropertyTrackFreeWinNode>();
    private PropertyTrackPayQueue<PropertyTrackPayNode> _payQueue = new PropertyTrackPayQueue<PropertyTrackPayNode>();
    private PropertyTrackPayWinQueue<PropertyTrackPayWinNode> _payWinQueue = new PropertyTrackPayWinQueue<PropertyTrackPayWinNode>();
    private List<PropertyTrackBaseNode> _subtractList;
    private PropertyTrackPayNode _purchaseCreateNode;
    private PropertyTrackBaseNode _cursubtractingNode;
    private Dictionary<PropertyNodeType, string> _mapDict = new Dictionary<PropertyNodeType, string>
    {
        {PropertyNodeType.Pay, "PN:"},
        {PropertyNodeType.PayWin, "PNW:"},
        {PropertyNodeType.Free, "FN:"},
        {PropertyNodeType.FreeWin, "FNW:"},
    };

    private bool _isInited;
    private bool _mouduleSwitch;

#region self define strings
    private readonly string _idStr = "id";
    private readonly string _bornTimeStr = "bornTime";
    private readonly string _lastDecreaseTimeStr = "lastDecreaseTime";
    private readonly string _originalAmountStr = "originalAmount";
    private readonly string _remainAmountStr = "remainAmount";
    private readonly string _creditsSourceStr = "creditsSource";
    private readonly string _enQueueTotalSpinCountStr = "enQueueTotalSpinCount";
    private readonly string _deQueueTotalSpinCountStr = "deQueueTotalSpinCount";
    private readonly string _nodeIdsStr = "nodeIds";
    private readonly string _orderIdStr = "orderId";
    private readonly string _linkNodeBornTimeStr = "linkNodeBornTime";
    private readonly string _linkNodeBornTotalSpinCountStr = "linkNodeBornTotalSpinCount";
#endregion

    public void Init()
    {
        CitrusEventManager.instance.AddListener<LoadSceneFinishedEvent>(HandlerUserPropertyData);
        CitrusEventManager.instance.AddListener<AddCreditsEvent>(HandleAddCreditsEvent);
        _mouduleSwitch = MapSettingConfig.Instance.Read("EnableFreePropertyTrack", "0") == "1";
    }

    void HandlerUserPropertyData(LoadSceneFinishedEvent e)
    {
        if (!_isInited)
        {
            StoreManager.Instance.OnPurchaseCompletedEvent.AddListener(OnUserPayValid);
            _isInited = true;

            DeserializeFreeQueueData(new JSONObject(UserBasicData.Instance.FreeQueue));
            DeserializeFreeWinQueueData(new JSONObject(UserBasicData.Instance.FreeWinQueue));
            BindFreeQueueWithFreeWinQueue();

            DeserializePayQueueData(new JSONObject(UserBasicData.Instance.PayQueue));
            DeserializePayWinQueueData(new JSONObject(UserBasicData.Instance.PayWinQueue));
            BindPayQueueWithPayWinQueue();

            //user's property may not be tracked from the register date, so add user's all property to a new propertyTrackFreeNode, then start track
            HandlePropertyBeforeTrack();
        }
    }

    void HandleAddCreditsEvent(AddCreditsEvent e)
    {
        if (e.Source != FreeCreditsSource.NotFree)
        {
            uint newIndex = UserBasicData.Instance.FreeNodeIndex++;
            PropertyTrackFreeNode newNode = new PropertyTrackFreeNode(newIndex, e.Source, e.Amount);
            newNode.EnQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
            _freeQueue.Enqueue(newNode);
            SendFreeNodeEnQueueEvent(newNode);
            SaveFreeQueueToJson();
        }
    }

    void HandlePropertyBeforeTrack()
    {
        if (UserBasicData.Instance.Credits > 0 && FreeQueueLength() == 0 && FreeWinQueueLength() == 0)
        {
            ulong trackedAmount = TotalPayAmount() + TotalPayWinAmount();
            if (UserBasicData.Instance.Credits > trackedAmount)
            {
                ulong unTrackedAmount = UserBasicData.Instance.Credits - trackedAmount;
                AddNewFreeNode(FreeCreditsSource.RemainPropertyBeforeTrack, unTrackedAmount);
            }
        }
    }

    void AddNewFreeNode(FreeCreditsSource source, ulong amount)
    {
        uint newIndex = UserBasicData.Instance.FreeNodeIndex++;
        PropertyTrackFreeNode newNode = new PropertyTrackFreeNode(newIndex, source, amount);
        newNode.EnQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
        _freeQueue.Enqueue(newNode);
        SendFreeNodeEnQueueEvent(newNode);
        SaveFreeQueueToJson();
    }


    #region public functions
    public void OnUserPayValid(IAPData data)
    {
        uint newIndex = UserBasicData.Instance.PayNodeIndex ++;
        IAPCatalogData iapConfig = IAPCatalogConfig.Instance.FindIAPItemByID(data.LocalItemId);
        string orderId = data.TransactionId;

        ulong credits = (ulong)IAPCatalogConfig.Instance.GetCreditsWithPromotion(iapConfig);
        PropertyTrackPayNode newNode = new PropertyTrackPayNode(newIndex, orderId, credits);
        newNode.EnQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
        _payQueue.Enqueue(newNode);
        _purchaseCreateNode = newNode;
        SendPayNodeEnQueueEvent(newNode);
        SavePayQueueToJson();
    }

    public void OnPayGameRewardUser(string transactionId, ulong rewardCredits)
    {
        foreach (var node in _payQueue)
        {
            if (node.PaidOrderId == transactionId)
            {
                node.RemainAmount += rewardCredits;
                break;
            }
        }
    }

    public List<string> HandleSpinResul(CoreSpinResult result)
    {
        List<PropertyTrackBaseNode> subNodeList = HandlerSpinLoseEvent(result.BetAmount);
        if (result.WinAmount != 0)
        {
            HandleSpinWinEvent(result.WinAmount);
        }

        return GetsubtractInfosFromNodeList(subNodeList);
    }

    #endregion

    #region private functions
    private List<PropertyTrackBaseNode> HandlerSpinLoseEvent(ulong subtractAmount)
    {
        _subtractList = new List<PropertyTrackBaseNode>();
        subtractAmount = SubtractCreditsFromQueue(subtractAmount, _payQueue, SavePayQueueToJson, SendPayNodeDeQueueEvent);
        subtractAmount = SubtractCreditsFromQueue(subtractAmount, _payWinQueue, SavePayWinQueueToJson, SendPayWinNodeDeQueueEvent);
        subtractAmount = SubtractCreditsFromQueue(subtractAmount, _freeQueue, SaveFreeQueueToJson, SendFreeNodeDeQueueEvent);
        SubtractCreditsFromQueue(subtractAmount, _freeWinQueue, SaveFreeWinQueueToJson, SendFreeWinNodeDeQueueEvent);

        SendPropertyInfoAfterWholeSubtractOver(_subtractList);

        return _subtractList;
    }

    private void HandleSpinWinEvent(ulong addAmount)
    {
        if (_payQueue != null && _payQueue.Count > 0)
        {
            PropertyTrackPayNode head = _payQueue.Peek();
            if (head.LinkNode == null)
            {
                PropertyTrackPayWinNode newNode = new PropertyTrackPayWinNode(head.Id, addAmount, head.BornTime);
                head.LinkNode = newNode;
                newNode.LinkNodeBornTime = head.BornTime;
                newNode.LinkNodeBornTotalSpinCount = head.EnQueueTotalSpinCount;
                newNode.EnQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
                _payWinQueue.Enqueue(newNode);
                SendPayWinNodeEnQueueEvent(newNode);
            }
            else
                head.LinkNode.RemainAmount += addAmount;
            SavePayWinQueueToJson();
        }
        else
        {
            if (_payWinQueue != null && _payWinQueue.Count > 0)
            {
                PropertyTrackPayWinNode head = _payWinQueue.Peek();
                head.RemainAmount += addAmount;
                SavePayWinQueueToJson();
            }
            else
            {
                if (_freeQueue != null && _freeQueue.Count > 0)
                {
                    PropertyTrackFreeNode head = _freeQueue.Peek();
                    if (head.LinkNode == null)
                    {
                        PropertyTrackFreeWinNode newNode = new PropertyTrackFreeWinNode(head.Id, head.CreditsSource, addAmount, head.BornTime);
                        head.LinkNode = newNode;
                        newNode.LinkNodeBornTime = head.BornTime;
                        newNode.LinkNodeBornTotalSpinCount = head.EnQueueTotalSpinCount;
                        newNode.EnQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
                        _freeWinQueue.Enqueue(newNode);
                        SendFreeWinNodeEnQueueEvent(newNode);
                    }
                    else
                        head.LinkNode.RemainAmount += addAmount;
                    SaveFreeWinQueueToJson();
                }
                else
                {
                    if (_freeWinQueue != null && _freeWinQueue.Count > 0)
                    {
                        PropertyTrackFreeWinNode head = _freeWinQueue.Peek();
                        head.RemainAmount += addAmount;
                        SaveFreeWinQueueToJson();
                    }
                }
            }
        }
    }

    private ulong SubtractCreditsFromQueue<T>(ulong subtractAmount, Queue<T> queue, Action onsubtract, Action<T> dequeueCallBack) where T : PropertyTrackBaseNode
    {
        if (subtractAmount != 0 && queue != null && queue.Count != 0)
        {
            while (subtractAmount > 0 && queue.Count > 0)
            {
                T head = queue.Peek();
                _cursubtractingNode = _cursubtractingNode ?? head;
                SendNodeChangeEvent(_cursubtractingNode, head);
                _subtractList.Add(head);
                if (head.RemainAmount > subtractAmount)
                {
                    head.RemainAmount -= subtractAmount;
                    subtractAmount = 0;
                    break;
                }
                head.LastDecreaseTime = NetworkTimeHelper.Instance.GetNowTime();
                subtractAmount -= head.RemainAmount;
                head.RemainAmount = 0;
                T removeNode = queue.Dequeue();
                dequeueCallBack.Invoke(removeNode);
            }

            onsubtract.Invoke();
        }

        return subtractAmount;
    }
    #endregion

    #region server data

    private void SaveFreeQueueToJson()
    {
        Dictionary<string, object> freeQueueDic = new Dictionary<string, object>();
        StringBuilder sb = new StringBuilder();

        foreach (PropertyTrackFreeNode node in _freeQueue)
        {
            Dictionary<string, object> freeNodeDic = new Dictionary<string, object>();
            freeNodeDic.Add(_idStr, node.Id);
            freeNodeDic.Add(_bornTimeStr, node.BornTime);
            freeNodeDic.Add(_lastDecreaseTimeStr, node.LastDecreaseTime);
            freeNodeDic.Add(_originalAmountStr, node.OriginalAmount);
            freeNodeDic.Add(_remainAmountStr, node.RemainAmount);
            freeNodeDic.Add(_creditsSourceStr, (int)node.CreditsSource);
            freeNodeDic.Add(_enQueueTotalSpinCountStr, node.EnQueueTotalSpinCount);
            freeNodeDic.Add(_deQueueTotalSpinCountStr, node.DeQueueTotalSpinCount);
            string freeNodeDicStr = Json.Serialize(freeNodeDic);

            if (!freeQueueDic.ContainsKey(node.Id.ToString()))
            {
                freeQueueDic.Add(node.Id.ToString(), freeNodeDicStr);
                sb.Append(node.Id + ",");
            }
        }

        if (freeQueueDic.Count > 0)
        {
            freeQueueDic.Add(_nodeIdsStr, sb.ToString().TrimEnd(','));
        }
        string freeQueue = Json.Serialize(freeQueueDic);
        UserBasicData.Instance.FreeQueue = freeQueue;
        //Debug.LogError("FreeQueueData: " + freeQueue);
    }

    void SaveFreeWinQueueToJson()
    {
        Dictionary<string, object> freeWinQueueDic = new Dictionary<string, object>();
        StringBuilder sb = new StringBuilder();

        foreach (PropertyTrackFreeWinNode node in _freeWinQueue)
        {
            Dictionary<string, object> freeWinNodeDic = new Dictionary<string, object>();
            freeWinNodeDic.Add(_idStr, node.Id);
            freeWinNodeDic.Add(_bornTimeStr, node.BornTime);
            freeWinNodeDic.Add(_lastDecreaseTimeStr, node.LastDecreaseTime);
            freeWinNodeDic.Add(_originalAmountStr, node.OriginalAmount);
            freeWinNodeDic.Add(_remainAmountStr, node.RemainAmount);
            freeWinNodeDic.Add(_linkNodeBornTimeStr, node.LinkNodeBornTime);
            freeWinNodeDic.Add(_creditsSourceStr, (int)node.CreditsSource);
            freeWinNodeDic.Add(_enQueueTotalSpinCountStr, node.EnQueueTotalSpinCount);
            freeWinNodeDic.Add(_deQueueTotalSpinCountStr, node.DeQueueTotalSpinCount);
            freeWinNodeDic.Add(_linkNodeBornTotalSpinCountStr, node.LinkNodeBornTotalSpinCount);
            string freeWinNodeStr = Json.Serialize(freeWinNodeDic);
            freeWinQueueDic.Add(node.Id.ToString(), freeWinNodeStr);

            sb.Append(node.Id + ",");
        }

        if (_freeWinQueue.Count > 0)
        {
            freeWinQueueDic.Add(_nodeIdsStr, sb.ToString().TrimEnd(','));
        }
        string freeWinQueue = Json.Serialize(freeWinQueueDic);
        UserBasicData.Instance.FreeWinQueue = freeWinQueue;
        //Debug.LogError("FreeWinQueueData: " + freeWinQueue);
    }

    void SavePayQueueToJson()
    {
        Dictionary<string, object> payQueueDic = new Dictionary<string, object>();
        StringBuilder sb = new StringBuilder();

        foreach (PropertyTrackPayNode node in _payQueue)
        {
            Dictionary<string, object> payNodeDic = new Dictionary<string, object>();
            payNodeDic.Add(_idStr, node.Id);
            payNodeDic.Add(_bornTimeStr, node.BornTime);
            payNodeDic.Add(_lastDecreaseTimeStr, node.LastDecreaseTime);
            payNodeDic.Add(_originalAmountStr, node.OriginalAmount);
            payNodeDic.Add(_remainAmountStr, node.RemainAmount);
            payNodeDic.Add(_orderIdStr, node.PaidOrderId);
            payNodeDic.Add(_enQueueTotalSpinCountStr, node.EnQueueTotalSpinCount);
            payNodeDic.Add(_deQueueTotalSpinCountStr, node.DeQueueTotalSpinCount);
            string payNodeDicStr = Json.Serialize(payNodeDic);
            payQueueDic.Add(node.Id.ToString(), payNodeDicStr);

            sb.Append(node.Id + ",");
        }

        if (payQueueDic.Count > 0)
        {
            payQueueDic.Add(_nodeIdsStr, sb.ToString().TrimEnd(','));
        }
        string payQueue = Json.Serialize(payQueueDic);
        UserBasicData.Instance.PayQueue = payQueue;
        //Debug.LogError("PayQueueData: " + payQueue);
    }

    void SavePayWinQueueToJson()
    {
        Dictionary<string, object> payWinQueueDic = new Dictionary<string, object>();
        StringBuilder sb = new StringBuilder();

        foreach (PropertyTrackPayWinNode node in _payWinQueue)
        {
            Dictionary<string, object> payWinNodeDic = new Dictionary<string, object>();
            payWinNodeDic.Add(_idStr, node.Id);
            payWinNodeDic.Add(_bornTimeStr, node.BornTime);
            payWinNodeDic.Add(_lastDecreaseTimeStr, node.LastDecreaseTime);
            payWinNodeDic.Add(_originalAmountStr, node.OriginalAmount);
            payWinNodeDic.Add(_remainAmountStr, node.RemainAmount);
            payWinNodeDic.Add(_linkNodeBornTimeStr, node.LinkNodeBornTime);
            payWinNodeDic.Add(_enQueueTotalSpinCountStr, node.EnQueueTotalSpinCount);
            payWinNodeDic.Add(_deQueueTotalSpinCountStr, node.DeQueueTotalSpinCount);
            payWinNodeDic.Add(_linkNodeBornTotalSpinCountStr, node.LinkNodeBornTotalSpinCount);
            string paywinNodeStr = Json.Serialize(payWinNodeDic);
            payWinQueueDic.Add(node.Id.ToString(), paywinNodeStr);

            sb.Append(node.Id + ",");
        }

        if (_payWinQueue.Count > 0)
        {
            payWinQueueDic.Add(_nodeIdsStr, sb.ToString().TrimEnd(','));
        }
        string payWinQueue = Json.Serialize(payWinQueueDic);
        UserBasicData.Instance.PayWinQueue = payWinQueue;
        //Debug.LogError("PayWinQueueData: " + payWinQueue);
    }

    void DeserializeFreeQueueData(JSONObject jsonObj)
    {
        if (jsonObj.Count > 0)
        {
            string ids = jsonObj.GetField(_nodeIdsStr).str;
            string[] idArray = ids.Split(',');

            foreach (var id in idArray)
            {
                JSONObject nodeJson = new JSONObject(jsonObj.GetField(id).str);
                uint nodeId = (uint)nodeJson.GetField(_idStr).n;
                DateTime bornTime = Convert.ToDateTime(nodeJson.GetField(_bornTimeStr).str);
                DateTime lastDecreaseTime = Convert.ToDateTime(nodeJson.GetField(_lastDecreaseTimeStr).str);
                ulong originalAmount = (ulong)nodeJson.GetField(_originalAmountStr).n;
                ulong remainAmount = (ulong)nodeJson.GetField(_remainAmountStr).n;
                FreeCreditsSource source = (FreeCreditsSource)nodeJson.GetField(_creditsSourceStr).n;
                int enQueueTotalSpinCount = nodeJson.HasField(_enQueueTotalSpinCountStr) ? (int)nodeJson.GetField(_enQueueTotalSpinCountStr).n : 0;
                int deQueueTotalSpinCount = nodeJson.HasField(_deQueueTotalSpinCountStr)
                    ? (int)nodeJson.GetField(_deQueueTotalSpinCountStr).n
                    : 0;

                PropertyTrackFreeNode node = new PropertyTrackFreeNode(nodeId, source, originalAmount);
                node.BornTime = bornTime;
                node.LastDecreaseTime = lastDecreaseTime;
                node.RemainAmount = remainAmount;
                node.EnQueueTotalSpinCount = enQueueTotalSpinCount;
                node.DeQueueTotalSpinCount = deQueueTotalSpinCount;
                _freeQueue.Enqueue(node);
            }
        }
    }

    void DeserializeFreeWinQueueData(JSONObject jsonObj)
    {
        if (jsonObj.Count > 0)
        {
            string ids = jsonObj.GetField(_nodeIdsStr).str;
            string[] idArray = ids.Split(',');

            foreach (var id in idArray)
            {
                JSONObject nodeJson = new JSONObject(jsonObj.GetField(id).str);
                uint nodeId = (uint)nodeJson.GetField(_idStr).n;

                DateTime bornTime = Convert.ToDateTime(nodeJson.GetField(_bornTimeStr).str);
                DateTime lastDecreaseTime = Convert.ToDateTime(nodeJson.GetField(_lastDecreaseTimeStr).str);
                ulong originalAmount = (ulong)nodeJson.GetField(_originalAmountStr).n;
                ulong remainAmount = (ulong)nodeJson.GetField(_remainAmountStr).n;
                FreeCreditsSource source = (FreeCreditsSource)nodeJson.GetField(_creditsSourceStr).n;

                DateTime linkNodeBornTime = nodeJson.HasField(_linkNodeBornTimeStr) ? Convert.ToDateTime(nodeJson.GetField(_linkNodeBornTimeStr).str) : DateTime.Now;
                int enQueueTotalSpinCount = nodeJson.HasField(_enQueueTotalSpinCountStr) ? (int)nodeJson.GetField(_enQueueTotalSpinCountStr).n : 0;
                int deQueueTotalSpinCount = nodeJson.HasField(_deQueueTotalSpinCountStr)
                    ? (int)nodeJson.GetField(_deQueueTotalSpinCountStr).n
                    : 0;
                int linkNodeBornTotalSpinCount = nodeJson.HasField(_linkNodeBornTotalSpinCountStr) ? (int)nodeJson.GetField(_linkNodeBornTotalSpinCountStr).n : 0;

                PropertyTrackFreeWinNode node = new PropertyTrackFreeWinNode(nodeId, source, originalAmount, linkNodeBornTime);
                node.BornTime = bornTime;
                node.LastDecreaseTime = lastDecreaseTime;
                node.RemainAmount = remainAmount;
                node.LinkNodeBornTime = linkNodeBornTime;
                node.EnQueueTotalSpinCount = enQueueTotalSpinCount;
                node.DeQueueTotalSpinCount = deQueueTotalSpinCount;
                node.LinkNodeBornTotalSpinCount = linkNodeBornTotalSpinCount;
                _freeWinQueue.Enqueue(node);
            }
        }
    }

    void DeserializePayQueueData(JSONObject jsonObj)
    {
        if (jsonObj.Count > 0)
        {
            string ids = jsonObj.GetField(_nodeIdsStr).str;
            string[] idArray = ids.Split(',');

            foreach (var id in idArray)
            {
                JSONObject nodeJson = new JSONObject(jsonObj.GetField(id).str);
                uint nodeId = (uint)nodeJson.GetField(_idStr).n;
                DateTime bornTime = Convert.ToDateTime(nodeJson.GetField(_bornTimeStr).str);
                DateTime lastDecreaseTime = Convert.ToDateTime(nodeJson.GetField(_lastDecreaseTimeStr).str);
                ulong originalAmount = (ulong)nodeJson.GetField(_originalAmountStr).n;
                ulong remainAmount = (ulong)nodeJson.GetField(_remainAmountStr).n;
                string orderId = nodeJson.GetField(_orderIdStr).str;
                int enQueueTotalSpinCount = nodeJson.HasField(_enQueueTotalSpinCountStr) ? (int)nodeJson.GetField(_enQueueTotalSpinCountStr).n : 0;
                int deQueueTotalSpinCount = nodeJson.HasField(_deQueueTotalSpinCountStr)
                    ? (int)nodeJson.GetField(_deQueueTotalSpinCountStr).n
                    : 0;

                PropertyTrackPayNode node = new PropertyTrackPayNode(nodeId, orderId, originalAmount);
                node.BornTime = bornTime;
                node.LastDecreaseTime = lastDecreaseTime;
                node.RemainAmount = remainAmount;
                node.EnQueueTotalSpinCount = enQueueTotalSpinCount;
                node.DeQueueTotalSpinCount = deQueueTotalSpinCount;
                _payQueue.Enqueue(node);
            }
        }
    }

    void DeserializePayWinQueueData(JSONObject jsonObj)
    {
        if (jsonObj.Count > 0)
        {
            string ids = jsonObj.GetField(_nodeIdsStr).str;
            string[] idArray = ids.Split(',');

            foreach (var id in idArray)
            {
                JSONObject nodeJson = new JSONObject(jsonObj.GetField(id).str);
                uint nodeId = (uint)nodeJson.GetField(_idStr).n;
                
                DateTime bornTime = Convert.ToDateTime(nodeJson.GetField(_bornTimeStr).str);
                DateTime lastDecreaseTime = Convert.ToDateTime(nodeJson.GetField(_lastDecreaseTimeStr).str);
                ulong originalAmount = (ulong)nodeJson.GetField(_originalAmountStr).n;
                ulong remainAmount = (ulong)nodeJson.GetField(_remainAmountStr).n;

                DateTime linkNodeBornTime = nodeJson.HasField(_linkNodeBornTimeStr) ? Convert.ToDateTime(nodeJson.GetField(_linkNodeBornTimeStr).str) : DateTime.Now;
                int enQueueTotalSpinCount = nodeJson.HasField(_enQueueTotalSpinCountStr) ? (int)nodeJson.GetField(_enQueueTotalSpinCountStr).n : 0;
                int deQueueTotalSpinCount = nodeJson.HasField(_deQueueTotalSpinCountStr)
                    ? (int)nodeJson.GetField(_deQueueTotalSpinCountStr).n
                    : 0;
                int linkNodeBornTotalSpinCount = nodeJson.HasField(_linkNodeBornTotalSpinCountStr) ? (int)nodeJson.GetField(_linkNodeBornTotalSpinCountStr).n : 0;

                PropertyTrackPayWinNode node = new PropertyTrackPayWinNode(nodeId, originalAmount, linkNodeBornTime);
                node.BornTime = bornTime;
                node.LastDecreaseTime = lastDecreaseTime;
                node.RemainAmount = remainAmount;
                node.LinkNodeBornTime = linkNodeBornTime;
                node.EnQueueTotalSpinCount = enQueueTotalSpinCount;
                node.DeQueueTotalSpinCount = deQueueTotalSpinCount;
                node.LinkNodeBornTotalSpinCount = linkNodeBornTotalSpinCount;
                _payWinQueue.Enqueue(node);
            }
        }
    }

    void BindFreeQueueWithFreeWinQueue()
    {
        foreach (var winNode in _freeWinQueue)
        {
            foreach (var freeNode in _freeQueue)
            {
                if (winNode.Id == freeNode.Id)
                {
                    freeNode.LinkNode = winNode;
                }
            }
        }
    }

    void BindPayQueueWithPayWinQueue()
    {
        foreach (var winNode in _payWinQueue)
        {
            foreach (var payNode in _payQueue)
            {
                if (winNode.Id == payNode.Id)
                {
                    payNode.LinkNode = winNode;
                }
            }
        }
    }

    #endregion

    #region helper Functions
    public int FreeQueueLength()
    {
        int result = _freeQueue == null ? 0 : _freeQueue.Count;
        return result;
    }

    public int FreeWinQueueLength()
    {
        int result = _freeWinQueue == null ? 0 : _freeWinQueue.Count;
        return result;
    }

    public int PayQueueLength()
    {
        int result = _payQueue == null ? 0 : _payQueue.Count;
        return result;
    }

    public int PayWinQueueLength()
    {
        int result = _payWinQueue == null ? 0 : _payWinQueue.Count;
        return result;
    }
    public ulong TotalFreeAmount()
    {
        return GetQueueTotalAmount(_freeQueue);
    }

    public ulong TotalFreeWinAmount()
    {
        return GetQueueTotalAmount(_freeWinQueue);
    }

    public ulong TotalPayAmount()
    {
        return GetQueueTotalAmount(_payQueue);
    }

    public ulong TotalPayWinAmount()
    {
        return GetQueueTotalAmount(_payWinQueue);
    }

    ulong GetQueueTotalAmount<T>(Queue<T> queue) where T : PropertyTrackBaseNode 
    {
        ulong result = 0;
        if (queue != null)
        {
            foreach (var kv in queue)
            {
                result += kv.RemainAmount;
            }
        }

        return result;
    }

    public void AddPurcahseCreateNodeInfo(Dictionary<string, object> d)
    {
        if (_purchaseCreateNode != null)
        {
            d["str10"] = _purchaseCreateNode.Id.ToString();
        }
    }

    public void AddDefaultValueToEvent(Dictionary<string, object> d, bool isPayInfo)
    {
        d["integer4"] = isPayInfo ? PayQueueLength() : FreeQueueLength();
        d["integer5"] = isPayInfo ? (int)TotalPayAmount() : (int)TotalFreeAmount();
        d["integer6"] = isPayInfo ? PayWinQueueLength() : FreeWinQueueLength();
        d["integer7"] = isPayInfo ? (int)TotalPayWinAmount() : (int)TotalFreeWinAmount();
    }

    List<string> GetsubtractInfosFromNodeList(List<PropertyTrackBaseNode> list)
    {
        List<string> result = new List<string>();
        if (list.Count == 0)
        {
            result.Add(PropertyNodeType.UnKnow.ToString());
            result.Add("None");
        }
        else
        {
            List<PropertyNodeType> typeList = new List<PropertyNodeType>();
            Dictionary<PropertyNodeType, string> idsDict = new Dictionary<PropertyNodeType, string>(_mapDict);
            StringBuilder nodeNames = new StringBuilder("");
            ListUtility.ForEach(list, node =>
            {
                if (!typeList.Contains(node.NodeType))
                    typeList.Add(node.NodeType);
                idsDict[node.NodeType] += node.Id + ",";
            });

            if (typeList.Count > 1)
            {
                typeList.Clear();
                typeList.Add(PropertyNodeType.Misc);
            }

            foreach (var kv in idsDict)
            {
                if (kv.Value != _mapDict[kv.Key])
                {
                    string newVaule = kv.Value.TrimEnd(',');
                    nodeNames.Append(newVaule + " ");
                }
            }

            result.Add(typeList[0].ToString());
            result.Add(nodeNames.ToString());
        }
        
        Debug.Assert(result.Count >= 2, "PropertyTrackMananger Error : function return's error data");

        return result;
    }

    #endregion

    #region events
    void SendFreeNodeEnQueueEvent(PropertyTrackFreeNode node)
    {
        if (_mouduleSwitch)
            AnalysisManager.Instance.FreeNodeCreate(node);
    }

    void SendFreeNodeDeQueueEvent(PropertyTrackFreeNode node)
    {
        if (_mouduleSwitch)
        {
            node.DeQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
            AnalysisManager.Instance.FreeNodeDestroy(node);
        }
    }

    void SendFreeWinNodeEnQueueEvent(PropertyTrackFreeWinNode node)
    {
        if (_mouduleSwitch)
            AnalysisManager.Instance.FreeWinNodeCreate(node);
    }

    void SendFreeWinNodeDeQueueEvent(PropertyTrackFreeWinNode node)
    {
        if (_mouduleSwitch)
        {
            node.DeQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
            AnalysisManager.Instance.FreeWinNodeDestroy(node);
        }
    }

    void SendPayNodeEnQueueEvent(PropertyTrackPayNode node)
    {
        AnalysisManager.Instance.PayNodeCreate(node);
    }

    void SendPayNodeDeQueueEvent(PropertyTrackPayNode node)
    {
        node.DeQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
        AnalysisManager.Instance.PayNodeDestroy(node);
    }

    void SendPayWinNodeEnQueueEvent(PropertyTrackPayWinNode node)
    {
        AnalysisManager.Instance.PayWinNodeCreate(node);
    }

    void SendPayWinNodeDeQueueEvent(PropertyTrackPayWinNode node)
    {
        node.DeQueueTotalSpinCount = UserMachineData.Instance.TotalSpinCount;
        AnalysisManager.Instance.PayWinNodeDestroy(node);
    }

    void SendNodeChangeEvent(PropertyTrackBaseNode preNode, PropertyTrackBaseNode curNode)
    {
        if (preNode.Id != curNode.Id || (preNode.Id == curNode.Id && preNode.NodeType != curNode.NodeType))
        {
            AnalysisManager.Instance.PropertyNodeChange(preNode, curNode);
            _cursubtractingNode = curNode;
        }
    }

    void SendPropertyInfoAfterWholeSubtractOver(List<PropertyTrackBaseNode> subNodeList)
    {
        if (subNodeList.Count > 1)
        {
            AnalysisManager.Instance.PropertyInfoAfterWholeSubtract();
        }
    }

    #endregion
}
