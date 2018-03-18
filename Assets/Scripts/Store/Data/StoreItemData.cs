using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StoreItemData 
{
	public string TitleText;
	public string DescriptionText;
	public string PriceText;
	public string ProductID;

	public StoreItemData(string productID,string title,string description,string price)
	{
		ProductID = productID;
		TitleText = title;
		DescriptionText = description;
		PriceText = price;
	}

}
