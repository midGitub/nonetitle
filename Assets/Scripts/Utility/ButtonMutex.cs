using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMutex : MonoBehaviour
{
	private PuzzleMachine _machine;
	private Button _button;
	private GameScene _gameScene;

	// Use this for initialization
	void Start()
	{
		_button = gameObject.GetComponent<Button>();
		CheckGameScene ();
	}

	// Update is called once per frame
	void Update()
	{
		CheckGameScene ();
		if(_button != null && _gameScene != null)
		{
			_machine = _gameScene.PuzzleMachine;
			if (_machine.MachineConfig != null && _machine.MachineConfig.BasicConfig.HasFixWild){
				if (gameObject.name.Equals ("BackButton")) {
					// 冻住的时候也能够返回
					_button.interactable = (_machine._state == MachineState.Idle);
				} else {
					// 冻住的时候不能下注
					_button.interactable = (_machine._state == MachineState.Idle && _machine.CoreMachine.SmallGameState != SmallGameState.FixWild);
				}
			}
			else{
				_button.interactable = (_machine._state == MachineState.Idle);
			}
		}
	}

	private void CheckGameScene(){
		if (_gameScene == null) {
			_gameScene = GameScene.Instance;
		}
	}
}
