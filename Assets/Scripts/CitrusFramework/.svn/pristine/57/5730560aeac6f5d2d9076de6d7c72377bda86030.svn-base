using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridExUpToDown : GridEx {			//From upside to downside 

	// Use this for initialization
	void Start () {
		mGrid = GetComponent<GridLayoutGroup>();
	}

	protected override void UpdateSizeDelta ()
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
                //var delta = size - sizeDelta[0];
                sizeDelta[0] = size;
                (transform as RectTransform).sizeDelta = sizeDelta;
            }

			//PS: Grid's pivot must set as (0.5, 1), and size (width, 0) 
            if(mGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {

                float size = (mLastBorder[2] - mLastBorder[3]);
                if(!Mathf.Equals(size, 0))
                {
                    size = size + mGrid.padding.top + mGrid.padding.bottom;
                    size += mGrid.cellSize.y;
                }
                var sizeDelta = (transform as RectTransform).sizeDelta;
                //var delta = size - sizeDelta[1];
                sizeDelta[1] = size;
                (transform as RectTransform).sizeDelta = sizeDelta;
            }
        }
    }

	// Update is called once per frame
	void Update () 
	{
		CheckBorderChange();
	}
}