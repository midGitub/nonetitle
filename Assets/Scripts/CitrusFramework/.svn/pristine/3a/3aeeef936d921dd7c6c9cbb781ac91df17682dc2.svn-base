using UnityEngine;
using System.Collections.Generic;


namespace CitrusFramework
{
	public class FSMTransition: IFSMTransition
	{
		public FSMTransition(int targetId)
		{
			mTargetState = targetId;
		}
		public int GetUniqueId()
		{
			return mTargetState;
		}
		
		protected int mTargetState;
	}

	public class FSMSystem : IFSMSystem, IFSMHost {
		protected List<FSMState> mStates;
		
		public FSMSystem()
		{
			mStates = new List<FSMState>();
		}
		
		/// <summary>
		/// This method places new mStates inside the FSM,
		/// or prints an ERROR message if the state was already inside the List.
		/// First state added is also the initial state.
		/// </summary>
		public virtual void AddState(FSMState s)
		{
			// Check for Null reference before deleting
			if (s == null)
			{
				GameDebug.LogError("FSM ERROR: Null reference is not allowed");
			}
			
			// First State inserted is also the Initial state,
			//   the state the machine is in when the simulation begins
			if (mStates.Count == 0)
			{
				mStates.Add(s);
				mCurrentState = s;
				mCurrentStateID = s.GetUniqueId();
				return;
			}
			
			// Add the state to the List if it's not inside it
			foreach (FSMState state in mStates)
			{
				if (state.GetUniqueId() == s.GetUniqueId())
				{
					GameDebug.LogError("FSM ERROR: Impossible to add state " + s.GetUniqueId().ToString() + 
					               " because state has already been added");
					return;
				}
			}
			mStates.Add(s);
		}
		
		/// <summary>
		/// This method delete a state from the FSM List if it exists, 
		///   or prints an ERROR message if the state was not on the List.
		/// </summary>
		public virtual void DeleteState(int id)
		{
			// Check for NullState before deleting
			if (id == InvalidState.GetInvalidState().GetUniqueId())
			{
				GameDebug.LogError("FSM ERROR: NullStateID is not allowed for a real state");
				return;
			}
			
			// Search the List and delete the state if it's inside it
			foreach (FSMState state in mStates)
			{
				if (state.GetUniqueId() == id)
				{
					mStates.Remove(state);
					return;
				}
			}
			GameDebug.LogError("FSM ERROR: Impossible to delete state " + id.ToString() + 
			               ". It was not on the list of mStates");
		}
		
		/// <summary>
		/// This method tries to change the state the FSM is in based on
		/// the current state and the transition passed. If current state
		///  doesn't have a target state for the transition passed, 
		/// an ERROR message is printed.
		/// </summary>
		public void PerformTransition(IFSMTransition trans, IFSMHost host)
		{
			// Check for NullTransition before changing the current state
			if (trans.GetUniqueId() == InvalidTransition.GetInvalidTransitionInstance().GetUniqueId())
			{
				GameDebug.LogError("FSM ERROR: NullTransition is not allowed for a real transition");
				return;
			}
			
			// Check if the mCurrentState has the transition passed as argument
			int id = mCurrentState.GetNextState(trans).GetUniqueId();
			if (id == InvalidState.GetInvalidState().GetUniqueId())
			{
				GameDebug.LogError("FSM ERROR: State " + mCurrentStateID.ToString() +  " does not have a target state " + 
                    " for transition " + trans.ToString() + " " + trans.GetUniqueId());
				return;
			}
			
			// Update the mCurrentStateID and mCurrentState		
			mCurrentStateID = id;
			foreach (FSMState state in mStates)
			{
				if (state.GetUniqueId() == mCurrentStateID)
				{
					// Do the post processing of the state before setting the new one
					mCurrentState.DoBeforeLeaving(host);
					
					mCurrentState = state;
					
					// Reset the state to its desired condition before it can reason or act
					mCurrentState.DoBeforeEntering(host);
					break;
				}
			}
			
		} // PerformTransition()

		// The only way one can change the state of the FSM is by performing a transition
		// Don't change the CurrentState directly
		protected int mCurrentStateID;
		public int CurrentStateID { get { return mCurrentStateID; } }
		protected FSMState mCurrentState;	
		public IFSMState CurrentState
		{
			get{
				return mCurrentState;
			}
		}

	}
}
