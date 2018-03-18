using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class FreeSpinController : MonoBehaviour
{
	PuzzleMachine _machine;
	
	public Text _freeSpinCountText;
	public GameObject _effect;

	private int _currentCount;
	private int _totalCount;

	// Use this for initialization
	void Start () {
		if(_effect != null)
			_effect.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
	}

	public void SetCurrentAndTotalCount(int current, int total)
	{
		if (_freeSpinCountText != null) {
			_currentCount = current;
			_totalCount = total;
			UpdateText();
		}
	}

	public void SetTotalCount(int total)
	{
		if(_totalCount != total)
		{
			//not play effect for the first time
			if(_totalCount != 0 && _effect != null)
			{
				_effect.SetActive(true);

				#if DEBUG
				if(!gameObject.activeSelf)
					Debug.Assert(false);
				#endif

				UnityTimer.Start(this, 2.0f, () => {
					_effect.SetActive(false);
				});
			}
			
			_totalCount = total;
			UpdateText();
		}
	}

	void UpdateText()
	{
		_freeSpinCountText.text = (_currentCount + 1).ToString () + "  of  " + _totalCount.ToString ();
	}
}
