using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace CitrusFramework
{
	/// <summary>
	/// Runs code in the Unity thread with a delay.
	/// </summary>
	public class UnityTimer : Singleton<UnityTimer>
	{ 
		private List<UnityTimerBase> m_CoroutineList = new List<UnityTimerBase>();

		/// <summary>
		/// return a PID for This Timer
		/// </summary>
		/// <param name="seconds">Seconds.</param>
		/// <param name="action">Action.</param>
		public int StartTimerInPID(float seconds, Callback action)
		{
			int result = 0;
			UnityTimerBase MonoTimer = FindUnUseTimer();
			if(MonoTimer != null)
            {
                result = MonoTimer.PID;
                MonoTimer.IsUse = true;
				MonoTimer.StartCoroutine(Delay(seconds, action, result));
			}
			return result;
		}

		public int WaitForFrameInPID(int n, Callback action)
		{
			int result = 0;
			UnityTimerBase MonoTimer = FindUnUseTimer();
			if(MonoTimer != null)
            {
                result = MonoTimer.PID;
				MonoTimer.IsUse = true;
				MonoTimer.StartCoroutine(FrameDelay(n, action, result));
			}
			return result;
		}

		public static Coroutine Start(MonoBehaviour obj, float seconds, Callback action)
		{
			return obj.StartCoroutine(Instance.Delay(seconds, action));
		}

		public void StartTimer(float seconds, Callback action)
		{
			this.StartCoroutine(Delay(seconds, action));
		}

		public void WaitForFrame(int n, Callback action)
		{
			this.StartCoroutine(FrameDelay(n, action));
		}

		public void StartTimer(MonoBehaviour obj, float seconds, Callback action)
		{
			obj.StartCoroutine(Delay(seconds, action));
		}

		public void WaitForFrame(MonoBehaviour obj , int n , Callback action)
		{
			obj.StartCoroutine(FrameDelay(n , action));
		}

		private IEnumerator Delay(float seconds, Callback action, int PID)
		{
			yield return new WaitForSeconds(seconds);
			action();
			UnUse(PID);
		}
			
		private IEnumerator FrameDelay(int n , Callback action, int PID)
		{
			yield return n;
			action();
			UnUse(PID);
		}

		private IEnumerator Delay(float seconds, Callback action)
		{
			yield return new WaitForSeconds(seconds);
			action();
		}

		private IEnumerator FrameDelay(int n , Callback action)
		{
			yield return n;
			action();
		}

		//stop
		public void StopALL()
		{
			this.StopAllCoroutines();
			for(int i = 0; i < m_CoroutineList.Count; i++)
			{
				m_CoroutineList[i].StopAllCoroutines();
				m_CoroutineList[i].IsUse = false;
			}
		}

		public void Stop(int PID)
		{
			UnityTimerBase Timer = m_CoroutineList.Find(s => s.PID == PID);
			if(Timer != null)
			{
				Timer.StopAllCoroutines();
				Timer.IsUse = false;
			}
		}

		public void ClearALL()
		{
			for(int i = 0; i < m_CoroutineList.Count; i++)
			{
				GameObject.Destroy(m_CoroutineList[i].gameObject);
			}
			m_CoroutineList.Clear();
		}

		public void Clear()
		{
			List<UnityTimerBase> remove = new List<UnityTimerBase>();
			for(int i = 0; i < m_CoroutineList.Count; i++)
			{
				if(!m_CoroutineList[i].IsUse)
				{
					GameObject.Destroy(m_CoroutineList[i].gameObject);
					remove.Add(m_CoroutineList[i]);
				}
			}
			for(int i = 0; i < remove.Count; i++)
			{
				m_CoroutineList.Remove(remove[i]);
			}
		}

		private void UnUse(int PID)
		{
			UnityTimerBase Timer = m_CoroutineList.Find(s => s.PID == PID);
			if(Timer != null)
			{
				Timer.IsUse = false;
			}
		}

		private UnityTimerBase FindUnUseTimer()
		{
			UnityTimerBase result = null;
			for(int i = 0; i < m_CoroutineList.Count; i++)
			{
				if(!m_CoroutineList[i].IsUse)
				{
					result = m_CoroutineList[i];
					break;
				}
			}
			if(result == null)
			{
				result = CreatNewTimer();
			}

			return result;
		}

		private UnityTimerBase CreatNewTimer()
		{
			GameObject newTimer = new GameObject();
			newTimer.transform.SetParent(this.transform);
			newTimer.name = m_CoroutineList.Count.ToString();
			UnityTimerBase MonoTimer = newTimer.AddComponent<UnityTimerBase>();
			MonoTimer.PID = m_CoroutineList.Count;
			MonoTimer.IsUse = false;
			m_CoroutineList.Add(MonoTimer);
			return MonoTimer;
		}
	}
}

