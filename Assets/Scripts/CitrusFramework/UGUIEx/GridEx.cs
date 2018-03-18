using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridEx : MonoBehaviour {			//From downside to upside

	[SerializeField]
	protected bool AutoSize = true;

	// Use this for initialization
	void Start () {
		mGrid = GetComponent<GridLayoutGroup>();
	}

	protected virtual void UpdateSizeDelta()
    {
        if(AutoSize && mGrid.constraint != GridLayoutGroup.Constraint.Flexible)
        {

            if(mGrid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                float size = (mLastBorder[1] - mLastBorder[0]);
                if(!Mathf.Equals(size, 0))
                {
                    size = size + mGrid.padding.left + mGrid.padding.right;
                    size += mGrid.cellSize.x;
                }


                var sizeDelta = (transform as RectTransform).sizeDelta;
                var delta = size - sizeDelta[0];
                sizeDelta[0] = size;
                (transform as RectTransform).sizeDelta = sizeDelta;

                if(mGrid.childAlignment == TextAnchor.LowerRight
                    || mGrid.childAlignment == TextAnchor.MiddleRight
                    || mGrid.childAlignment == TextAnchor.UpperRight)
                {
                    for(int i = 0; i < mGrid.transform.childCount; ++i)
                    {
                        var child = mGrid.transform.GetChild(i);
                        if(child.gameObject.activeSelf)
                        {
                            var p = (child as RectTransform).anchoredPosition;
                            p.x += delta;
                            (child as RectTransform).anchoredPosition = p;
                        }
                    }
                }
            }

            if(mGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {

                float size = (mLastBorder[2] - mLastBorder[3]);
                if(!Mathf.Equals(size, 0))
                {
                    size = size + mGrid.padding.top + mGrid.padding.bottom;
                    size += mGrid.cellSize.y;
                }
                var sizeDelta = (transform as RectTransform).sizeDelta;
                var delta = size - sizeDelta[1];
                sizeDelta[1] = size;
                (transform as RectTransform).sizeDelta = sizeDelta;



                if(mGrid.childAlignment == TextAnchor.LowerLeft
                    || mGrid.childAlignment == TextAnchor.LowerCenter
                    || mGrid.childAlignment == TextAnchor.LowerRight)
                {
                    for(int i = 0; i < mGrid.transform.childCount; ++i)
                    {
                        var child = mGrid.transform.GetChild(i);
                        if(child.gameObject.activeSelf)
                        {
                            var p = (child as RectTransform).anchoredPosition;
                            p.y -= delta;
                            (child as RectTransform).anchoredPosition = p;
                        }
                    }
                }
            }
        }
    }

	protected int GetLogicalGridChildCount()
    {
        var ret = mLogicChildCount > 0 && mLogicChildCount <= mGrid.transform.childCount?
            mLogicChildCount : mGrid.transform.childCount;

        return ret;
    }
	
	// Update is called once per frame
	void Update () 
	{
		CheckBorderChange();
	}

	protected void CheckBorderChange()
	{
        bool hasActiveChild = false;
        // left, right, up, down
        float[] border = {Mathf.Infinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.Infinity};
        for(int i = 0; i < GetLogicalGridChildCount(); ++i)
        {
            var child = mGrid.transform.GetChild(i);
            if(child.gameObject.activeSelf)
            {
                hasActiveChild = true;

                var pos = child.localPosition;
                if(pos.x < border[0])
                    border[0] = pos.x;
                if(pos.x > border[1])
                    border[1] = pos.x;
                if(pos.y > border[2])
                    border[2] = pos.y;
                if(pos.y < border[3])
                    border[3] = pos.y;
            }
        }

        if(!hasActiveChild)
            border[0]=border[1]=border[2]=border[3]=0;

        bool toUpdate = false;
        for(int i = 0; i < 4; ++i)
        {
            if(!Mathf.Equals(border[i], mLastBorder[i]))
                toUpdate = true;

            mLastBorder[i] = border[i];
        }

        if(toUpdate)
        {
            UpdateSizeDelta();
        }

	}

    public void SetLogicChildCount(int count)
    {
        mLogicChildCount = count;
    }

	protected GridLayoutGroup mGrid = null;
	protected Vector4 mLastBorder = Vector4.zero;

	protected int mLogicChildCount = 0;
}