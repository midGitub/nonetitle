using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillBoardManager : SimpleSingleton<BillBoardManager>{
	
	BillBoard _billBoard;

    private GameObject _billboardgameobject;
	private BillboardPatchBehaviour _billboardPatch;

    public void Add(BillBoardBase billboardbase)
	{
		InitBillBoard();
		_billBoard.Add(billboardbase);
		CheckTwoImage();
	}

	public void Delete(BillBoardBase billboardbase)
	{
		if (_billBoard.Delete(billboardbase))
			Reset();
	}

    void InitBillBoard()
    {
        if (_billBoard == null)
        {
            _billboardgameobject = UGUIUtility.InstantiateUI("Game/UI/BillBoard/BillBoard");
            Transform parent = GameObject.Find("BillBoardParent").transform;
            Debug.Assert(parent != null, "BillBoardMng: billboard parent is null !");
            _billboardgameobject.transform.SetParent(parent, false);
            _billboardgameobject.SetActive(true);
            _billBoard = _billboardgameobject.GetComponent<BillBoard>();
			_billboardPatch = _billboardgameobject.GetComponentInChildren<BillboardPatchBehaviour>();
        }
    }

    void Reset()
	{
		if (_billBoard != null&&_billBoard.NoBillExist())
		{
			_billBoard.gameObject.SetActive(false);
			_billBoard = null;
		}
	}

	private void CheckTwoImage(){
		if (_billBoard == null || _billboardPatch == null){
			return;
		}

		if (_billBoard.Count == 2){
			_billboardPatch.Enable2Image();
		}else {
			_billboardPatch.Disable2Image();
		}
	}
}
