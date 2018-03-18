using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectWithScroll : MonoBehaviour
{
	// 最前面的活动层
	public ScrollRect MoveScrollRect;

	public List<ScrollRect> FollowScrollRect = new List<ScrollRect>();

	private void Start()
	{
		MoveScrollRect.onValueChanged.AddListener(TogetherMove);
	}

	private void TogetherMove(Vector2 postion)
	{
		foreach(var item in FollowScrollRect)
		{
			var nv = MoveScrollRect.horizontalNormalizedPosition;
			// 两边夹紧没有 反弹效果
			if(nv > 1)
			{
				nv = 1;
			}
			else if(nv < 0)
			{
				nv = 0;
			}
			item.horizontalNormalizedPosition = nv;
		}
	}
}
