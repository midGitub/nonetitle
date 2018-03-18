using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class UIUtility {


    static void QuickSort(List<Transform> list, int left, int right, IComparer comparer)
    {
        int i = left, j = right;
        var pivot = list[(left + right) / 2];
        Transform temp = null;

        while (i <= j)
        {
            //while (m_TestObjectList[i].m_Z < pivot.m_Z)
            while(comparer.Compare(list[i], pivot) < 0)
            {
                i++;
            }

            //while (m_TestObjectList[j].m_Z > pivot.m_Z)
            while(comparer.Compare(list[j], pivot) > 0)
            {
                j--;
            }

            if (i <= j)
            {
                // Swap
                temp = list[i];
                list[i] = list[j];
                list[j] = temp;

                i++;
                j--;
            }
        }

        // Recursive calls
        if (left < j)
        {
            QuickSort(list, left, j, comparer);
        }

        if (i < right)
        {
            QuickSort(list, i, right, comparer);
        }
    }
    static public void QuickSortHierarchy(Transform parent, IComparer comparer)
    {
        if(parent.childCount == 0)
            return;
        
        var list = new List<Transform>(parent.childCount);
        for(int i = 0; i < parent.childCount; ++i)
        {
            list.Add(parent.GetChild(i));
        }

        // start sorting
        QuickSort(list, 0, list.Count-1, comparer);

        for (int i = 0; i < list.Count; ++i)
        {
            list[i].SetAsLastSibling();
        }

    }

	static public void SetTextInChild(Transform node, string childName, string text)
	{
		var o = node.FindChild(childName);
		if(o)
			SetText(node, text);
	}

	static public void SetTextInChild(Transform node, int childIndex, string text)
	{
		if(childIndex < node.childCount)
		{
			var o = node.GetChild(childIndex);
			if(o)
				SetText(node, text);
		}
	}
	
	static public void SetText(Transform node, int num, bool additive = false)
	{
		if(additive)
		{
			node.GetComponentInChildren<GrowingNumber>().AddTo(num);
		}
		else
			SetText(node, num.ToString());


	}

	static public void SetText(Transform node, string text)
	{
		var t = node.GetComponentInChildren<Text>();
		if(t)
			t.text = text;
	}

	static public string GetFractionText(int num1, int num2, bool canFirstRed = false, bool isFirstGreater = false)
	{
		if(canFirstRed)
		{
			if(isFirstGreater && num1>num2)
			{
				string s = "<color=#ff0000>" + num1;
				s += "</color>/" + num2;
				return s;
			}
			else if(!isFirstGreater && num1<num2)
			{
				string s = "<color=#ff0000>" + num1;
				s += "</color>/" + num2;
				return s;
			}
		}

		return num1 + "/" + num2;
	}
	
	static public string GetPercentText(float num)
	{
		return string.Format("{0:f}%", num);
	}

	static public string GetTimeText(int seconds)
	{
		int[] t = new int[3];
		t[0] = seconds/3600;
		t[1] = (seconds/60)%60;
		t[2] = seconds%60;
		return string.Format("{0:00}:{1:00}:{2:00}", t[0], t[1],t[2]);
	}

	static public Sprite GetSprite(string spritePath)
	{		
		return Resources.Load<Sprite>(spritePath);
	}
		
	static public void SetImage(Transform node, Sprite sprite, bool fitSize = false)
	{
		var i = node.GetComponentInChildren<Image>();
		if(i)
			i.sprite = sprite;
		if(fitSize)
			i.SetNativeSize();
	}	
	
	static public void SetChildrenImage(Transform node, List<Sprite> sprites)
	{
		int i =0;
		while(i<node.childCount)
		{
			var t = node.GetChild(i);
			var image = t.GetComponent<Image>();
			
			if(image)
			{
				if(i<sprites.Count)
				{
					t.gameObject.SetActive(true);
					image.sprite = sprites[i];
				}
				else
					t.gameObject.SetActive(false);
			}
			i++;
		}
	}
}
