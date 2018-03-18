using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class LongPressEventTrigger : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[ Tooltip( "How long must pointer be down on this object to trigger a long press" ) ]
	private static readonly float _defaultDurationThreshold = 1.5f;

	public event Action<LongPressEventTrigger> LongPressEventHandler = delegate {};
	public event Action<LongPressEventTrigger> PointerDownEventHandler = delegate {};

	private bool isPointerDown = false;
	private bool longPressTriggered = false;
	private float timePressStarted;
	private float _durationThreshold = _defaultDurationThreshold;

	public float DurationThreshold { get { return _durationThreshold; } set { _durationThreshold = value; } }

	private void Update( ) {
		if ( isPointerDown && !longPressTriggered ) {
			if ( Time.time - timePressStarted > _durationThreshold ) {
				longPressTriggered = true;
				LongPressEventHandler(this);
			}
		}
	}

	public void OnPointerDown( PointerEventData eventData ) {
		timePressStarted = Time.time;
		isPointerDown = true;
		longPressTriggered = false;

		PointerDownEventHandler(this);
	}

	public void OnPointerUp( PointerEventData eventData ) {
		isPointerDown = false;
	}

	public void OnPointerExit( PointerEventData eventData ) {
		isPointerDown = false;
	}
}
