using System.Collections.Generic;
using UnityEngine;


namespace CitrusFramework
{
	/// <summary>
	/// IFSM transition.
	/// </summary>
	public interface IFSMTransition
	{
		/// <summary>
		/// Gets the unique identifier in FSM
		/// </summary>
		/// <returns>The unique identifier.</returns>
		int GetUniqueId();

		/// <summary>
		/// Returns a string represents the transition.
		/// </summary>
		/// <returns>string represents the transition</returns>
		string ToString();
	}

	/// <summary>
	/// Represents a invalid transition
	/// </summary>
	public sealed class InvalidTransition : IFSMTransition
	{
		public int GetUniqueId()
		{
			return 0;
		}

		public override string ToString()
		{
			return "Invalid transition";
		}

		private InvalidTransition(){}
		private static readonly InvalidTransition m_Instance = new InvalidTransition();
		public static InvalidTransition GetInvalidTransitionInstance()
		{
			return m_Instance;
		}

		public static bool IsValidTransition(IFSMTransition transition)
		{
			return (transition != null) && (transition != m_Instance);
		}
	}

	/// <summary>
	/// IFSM state.
	/// </summary>
	public interface IFSMState
	{
		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <returns>The unique identifier.</returns>
		int GetUniqueId();

		/// <summary>
		/// Returns a string represents the state.
		/// </summary>
		/// <returns>string represents the state</returns>
		string ToString();

		/// <summary>
		/// Add transition for this state
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="targetState">Target state.</param>
		void AddTransition(IFSMTransition transition, IFSMState targetState);

		/// <summary>
		/// Delete transition for this state
		/// </summary>
		/// <param name="transition">Transition.</param>
		void DeleteTransition(IFSMTransition transition);

		/// <summary>
		/// Check which state to go for this transition
		/// </summary>
		/// <returns>The next state.</returns>
		/// <param name="transition">Transition.</param>
		IFSMState GetNextState(IFSMTransition transition);

		/// <summary>
		/// Hook before entering this state
		/// </summary>
		void DoBeforeEntering(IFSMHost host);

		/// <summary>
		/// Hook before leaving this state
		/// </summary>
		void DoBeforeLeaving(IFSMHost host);

		/// <summary>
		/// Check if need change state
		/// </summary>
		/// <param name="host">Host.</param>
		void Reason(IFSMHost host);

		/// <summary>
		/// Performe action for host
		/// </summary>
		/// <param name="host">Host.</param>
		void Act(IFSMHost host);
	}

	/// <summary>
	/// Represents an invalid state
	/// </summary>
	public sealed class InvalidState : IFSMState
	{
		public int GetUniqueId()
		{
			return 0;
		}

		public override string ToString()
		{
			return "Invalid state";
		}

		public void AddTransition(IFSMTransition transition, IFSMState targetState)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}
		
		public void DeleteTransition(IFSMTransition transition)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}
		
		public IFSMState GetNextState(IFSMTransition transition)
		{
			GameDebug.LogWarning("Calling on invalid state.");
			return InvalidState.GetInvalidState();
		}
		
		public void DoBeforeEntering(IFSMHost host)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}
		
		public void DoBeforeLeaving(IFSMHost host)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}
		
		public void Reason(IFSMHost host)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}
		
		public void Act(IFSMHost host)
		{
			GameDebug.LogWarning("Calling on invalid state.");
		}

		private InvalidState() {}
		private static readonly InvalidState m_Instance = new InvalidState();
		public static InvalidState GetInvalidState()
		{
			return m_Instance;
		}

		public static bool IsValidState(IFSMState state)
		{
			return (state != null) && (state != m_Instance);
		}
	}

	/// <summary>
	/// The finite state machine system
	/// </summary>
	public interface IFSMSystem
	{
		/// <summary>
		/// Get current state
		/// </summary>
		/// <value>The state of the current.</value>
		IFSMState CurrentState { get; }

		/// <summary>
		/// Performs the transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		void PerformTransition(IFSMTransition transition, IFSMHost host);

		/// <summary>
		/// Returns a string represents the FSM system.
		/// </summary>
		/// <returns>string represents the FSM system</returns>
		string ToString();
	}

	/// <summary>
	/// IFSM host.
	/// </summary>
	public interface IFSMHost
	{
		/// <summary>
		/// Returns a string represents the IFSMHost.
		/// </summary>
		/// <returns>string represents the IFSMHost</returns>
		string ToString();
	}

	/// <summary>
	/// Base FSMState class, implements a simple dictionary to store all trasitions
	/// </summary>
	public abstract class FSMState : IFSMState
	{
		public abstract int GetUniqueId();
		public override string ToString()
		{
			return "FSBState";
		}
		
		/// <summary>
		/// map stores all transitions, <transition_id, state>
		/// </summary>
		private Dictionary<int, IFSMState> m_map = new Dictionary<int, IFSMState>();

		public void AddTransition(IFSMTransition transition, IFSMState targetState)
		{
			//check if invalid
			if (!InvalidTransition.IsValidTransition(transition)) 
			{
				GameDebug.LogError ("Transition is invalid");
			} 
			else if (!InvalidState.IsValidState(targetState)) 
			{
				GameDebug.LogError ("State is invalid");
			} 
			else if (m_map.ContainsKey(transition.GetUniqueId()))
			{
				GameDebug.LogWarning("The transition already exist!");
			}
			else
			{
				m_map.Add(transition.GetUniqueId(), targetState);
			}
		}

		public void DeleteTransition(IFSMTransition transition)
		{
			int transitionId = transition.GetUniqueId();

			if(!InvalidTransition.IsValidTransition(transition))
			{
				Debug.LogError ("Transition is invalid");
			}
			else if(!m_map.ContainsKey(transitionId))
			{
				Debug.LogWarning("The transition id does not exist!");
			}
			else 
			{
				m_map.Remove(transitionId);
			}
		}

		public IFSMState GetNextState(IFSMTransition transition)
		{
			IFSMState result = InvalidState.GetInvalidState();

			if(InvalidTransition.IsValidTransition(transition))
			{
				if(m_map.ContainsKey(transition.GetUniqueId()))
				{
					result = m_map[transition.GetUniqueId()];
				}
			}
			else
			{
				Debug.LogWarning("Input transition is invalid!");
			}

			return result;
		}

		public virtual void DoBeforeEntering(IFSMHost host){}
		public virtual void DoBeforeLeaving(IFSMHost host){}
		public virtual void Reason(IFSMHost host){}
		public virtual void Act(IFSMHost host){}
	}
}