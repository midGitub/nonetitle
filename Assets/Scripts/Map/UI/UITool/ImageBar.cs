using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBar : MonoBehaviour
{
	public Image barImage;

	/// <summary>
	/// 差值所占的比例
	/// </summary>
	/// <param name="fromValue">From value.</param>
	/// <param name="toValue">To value.</param>
	/// <param name="currValue">Curr value.</param>
	public void ChangeBarState(float fromValue,float toValue, float currValue)
	{
		barImage.fillAmount = (currValue-fromValue) / (toValue-fromValue);
	}

	/// <summary>
	/// 总数所占的比例
	/// </summary>
	/// <param name="toValue">To value.</param>
	/// <param name="currValue">Curr value.</param>
	public void ChangeBarState(float toValue, float currValue)
	{
		barImage.fillAmount = currValue / toValue ;
	}
}
