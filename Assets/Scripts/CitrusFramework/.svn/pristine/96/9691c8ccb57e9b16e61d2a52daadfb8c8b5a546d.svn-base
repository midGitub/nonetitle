using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GrowingNumber : MonoBehaviour {

	[SerializeField]
	float MaxTime = 1.0f;
	[SerializeField]
	float NumberAddInSecond = 100;
    [SerializeField]
    Text NumberText;

	void Awake()
	{
		mCurrent = 0;
		mTarget = mCurrent;

	}
	
	// Update is called once per frame
	void Update () {
		int delta = mTarget - mCurrent;

		if(delta > 0)
		{
			if(mCurrentSpeed == 0)
				mCurrentSpeed = MaxTime*NumberAddInSecond > delta ? NumberAddInSecond : delta / MaxTime;
			mCurrent += (int)(mCurrentSpeed * Time.deltaTime);
			if(mCurrent > mTarget)
				mCurrent = mTarget;
            if(NumberText != null)
			    UIUtility.SetText(transform, mCurrent.ToString());
		}
		else if(delta < 0)
		{
            mCurrent = mTarget;
            if(NumberText != null)
			    UIUtility.SetText(transform, mTarget.ToString());
		}
	}

	public void Add(int toAdd)
	{
		if(toAdd != 0)
		{
			mTarget = mCurrent + toAdd;
			mCurrentSpeed = 0;
		}
	}

	public void AddTo(int to)
	{
		if(mTarget != to)
		{
			mTarget = to;
			mCurrentSpeed = 0;
		}
	}

    public int GetCurrent()
    {
        return mCurrent;
    }

    public bool IsGrowing()
    {
        return mCurrent != mTarget;
    }

	int mTarget = 0;
	int mCurrent = 0;
	float mCurrentSpeed = 0;
}
