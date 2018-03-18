using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class DiceControler : MonoBehaviour {

	public Animator PlayDiceAnimator;
	public List<GameObject> Dices;
    public GameObject DiceBParticle;
    private GameScene _gameScene;
    private PuzzleMachine _machine;

    private const int _singleDice = 1;
    private const int _twoDices = 2;

    private readonly string _singleDiceIdle = "playSingleIdle";
    private readonly string _multipleDiceIdle = "playMultipleIdle";
    private readonly string _singleDiceAnimName = "playSingleDice";
    private readonly string _multipleDiceAnimName = "playMultipleDice";
    private readonly string _showSecondDiceAnimName = "showSecondDice";
    private readonly string _hideSecondDiceAnimName = "hideSecondDice";

    private Dictionary<int, Vector3> pointMapToRot = new Dictionary<int, Vector3> {{1,new Vector3(0,0,0)},   {2,new Vector3(270,0,0)}, 
																				   {3,new Vector3(0,0,270)}, {4,new Vector3(0,0,90)},
																				   {5,new Vector3(90f,0,0)}, {6,new Vector3(180f,0,0)},};

	void Start()
	{
        CitrusEventManager.instance.AddListener<PayForMoreDicesEvent>(OnPayForMoreDices);
        CitrusEventManager.instance.AddListener<ResetDiceCountEvent>(OnResetDicesCount);
        CitrusEventManager.instance.AddListener<DiceGameStartEvent>(OnGameStart);
    }

	void OnDestroy()
	{
        CitrusEventManager.instance.RemoveListener<PayForMoreDicesEvent>(OnPayForMoreDices);
        CitrusEventManager.instance.RemoveListener<ResetDiceCountEvent>(OnResetDicesCount);
        CitrusEventManager.instance.RemoveListener<DiceGameStartEvent>(OnGameStart);
    }

    void OnEnable()
    {
        //dice show six point as default state
        SetDefaultRotation(6);
        SetIdleAnim(DiceManager.Instance.PlayerDiceData.DiceData.DiceNum);

        CheckGameScene();
        // 停止游戏内bgm
        StopPuzzleMachineBGM();
    }

    void OnDisable(){
        // 恢复游戏内bgm
        ResumePuzzleMachineBGM();
    }

    private void CheckGameScene(){
        if (_gameScene == null){
            _gameScene = GameScene.Instance;
            _machine = _gameScene.PuzzleMachine;
        }
    }

    private void StopPuzzleMachineBGM(){
        if (_machine != null){
            _machine.StopBGM();
        }
    }

    private void ResumePuzzleMachineBGM(){
        if (_machine != null){
            _machine.ResumeBGM();
        }
    }

    void SetDefaultRotation(int point)
    {
        ListUtility.ForEach(Dices, x => 
            { x.transform.localEulerAngles = pointMapToRot[point]; });
    }

    void SetIdleAnim(int showDiceCount)
    {
        string animName = "";
        switch (showDiceCount)
        {
            case _singleDice:
                animName = _singleDiceIdle;
                break;
            case _twoDices:
                animName = _multipleDiceIdle;
                break;
        }

        PlayDiceAnimator.SetTrigger(animName);
    }

    void OnPayForMoreDices(PayForMoreDicesEvent e)
    {
        PlayDiceAnimator.SetTrigger(_showSecondDiceAnimName);
    }

    void OnResetDicesCount(ResetDiceCountEvent e)
    {
        PlayDiceAnimator.SetTrigger(_hideSecondDiceAnimName);
    }

    public void OnGameStart(DiceGameStartEvent e)
	{
	    string animName = GetDiceAnimNameByDiceCount(e.DiceCount);
        Debug.Assert(PlayDiceAnimator != null, "DiceModuleError, DiceAnimator Missed!， extra info : diceCount =  " + e.DiceCount);
	    if (PlayDiceAnimator != null)
	    {
            PlayDiceAnimator.SetTrigger(animName);
        }

		AudioManager.Instance.PlaySound(AudioType.RollCrazyDice);

		StartCoroutine(RaiseGameEndEvent(animName));

		List<int> resultList = DiceHelper.Instance.CalculateResultByDiceCount(e.DiceCount, e.Result);

		StartCoroutine(SetPlayDiceResultByChangeRot(resultList, e.DelayTime));
	}

    private string GetDiceAnimNameByDiceCount(int diceCount)
    {
        string result = "";
        switch (diceCount)
        {
            case _singleDice:
                result = _singleDiceAnimName;
                break;
            case _twoDices:
                result = _multipleDiceAnimName;
                break;
            default:
                result = _multipleDiceAnimName;
                break;
        }

        return result;
    }

    private readonly int _maxDiceNum = 6;
	private IEnumerator SetPlayDiceResultByChangeRot(List<int> result, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);

		for(int i = 0; i < result.Count; i++)
		{
			Debug.Assert(result[i] <= _maxDiceNum && result[i] > 0, "diceControler : error point result, need to check : error num " + result[i]);
			Dices[i].transform.localEulerAngles = pointMapToRot[result[i]];
		}
	}

	private IEnumerator RaiseGameEndEvent(string animName)
	{
        AnimatorStateInfo stateInfo = PlayDiceAnimator.GetCurrentAnimatorStateInfo(0);

        while(!stateInfo.IsName(animName))
		{
		    stateInfo = PlayDiceAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
		}

		yield return new WaitForSeconds(stateInfo.length);
		CitrusEventManager.instance.Raise(new DiceGameEndEvent());
		AudioManager.Instance.PlaySound(AudioType.CrazyDiceTimesResult);
	}
}
