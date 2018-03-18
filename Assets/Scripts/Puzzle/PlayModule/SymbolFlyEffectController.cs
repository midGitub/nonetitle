using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CitrusFramework;

public class SymbolFlyEffectController : MonoBehaviour
{
	public GameObject _flyEndObject;
	public float _flyTime;

	PuzzleMachine _machine;
	MachineConfig _machineConfig;

	GameObject[] _flyEffects;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
		_machineConfig = _machine.MachineConfig;

		_flyEffects = new GameObject[_machineConfig.BasicConfig.BasicReelCount];

		if(!string.IsNullOrEmpty(_machineConfig.BasicConfig.FreeSpinSymbolFlyEffect))
		{
			for(int i = 0; i < _machineConfig.BasicConfig.BasicReelCount; i++)
			{
				GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(_machineConfig.BasicConfig.FreeSpinSymbolFlyEffect,
					_machine.MachineName);
				_flyEffects[i] = GameObject.Instantiate(obj);
				_flyEffects[i].SetActive(false);
				_flyEffects[i].transform.SetParent(_flyEndObject.transform);
				_flyEffects[i].transform.localPosition = Vector3.zero;
				_flyEffects[i].transform.localScale = Vector3.one;
			}
		}
	}

	public void PlayFlyEffect(List<PuzzleSymbol> symbols, Callback endCallback)
	{
		for(int i = 0; i < symbols.Count; i++)
		{
			GameObject flyEffect = _flyEffects[i];
			PuzzleSymbol symbol = symbols[i];
			Vector3 worldPos = symbol.transform.TransformPoint(Vector3.zero);
			Vector3 localPos = _flyEndObject.transform.InverseTransformPoint(worldPos);
			flyEffect.transform.localPosition = localPos;
			flyEffect.SetActive(true);

			Sequence seq = DOTween.Sequence();
			Tweener move = flyEffect.transform.DOLocalMove(Vector3.zero, _flyTime);
			seq.Append(move).AppendCallback(() => {
				if(endCallback != null)
					endCallback();
			}).AppendInterval(0.5f).OnComplete(() => {
				flyEffect.SetActive(false);
			});
		}

		AudioManager.Instance.PlaySound(AudioType.ParticleFly);
	}
}
