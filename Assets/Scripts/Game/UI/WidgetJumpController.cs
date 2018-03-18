using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

//[RequireComponent(typeof(Animator))]
public class WidgetJumpController : MonoBehaviour
{
	private static readonly string _animatorAssetPath = "Map/StoreAnim/BigBGPanel_Anim";
	private int _closeHash = 0;

	[SerializeField]
	private Animator _animator;

	void Awake()
	{
		//#if false
		if(_animator == null)
		{
			_animator = this.gameObject.AddComponent<Animator>();
		}
		//// todo 有了自己的动画就删除掉
		//UnityEngine.Object animControl = AssetManager.Instance.LoadAsset<UnityEngine.Object>(_animatorAssetPath);
		//if(animControl != null)
		//{
		//	_animator.runtimeAnimatorController = animControl as RuntimeAnimatorController;
		//}
		////#endif
		_closeHash = Animator.StringToHash("Close");
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Open(bool open, Callback func = null)
	{
		_animator.SetBool(_closeHash, !open);

		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
		UnityTimer.Start(this, info.length + 0.1f, () =>
		{
			if(func != null)
			{
				func();
			}
		});
	}
}
