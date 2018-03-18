using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingRrotating : MonoBehaviour
{
	[SerializeField]
	private GameObject _RotateGameObject;

	public float RotateSpeed = 10;

	private Coroutine _rotateCoroutine = null;

	void OnEnable(){
		if (_rotateCoroutine != null) {
			StopCoroutine (_rotateCoroutine);
		}
		_rotateCoroutine = StartCoroutine (LoadingRotate());
	}

	void OnDestroy(){
		if (_rotateCoroutine != null) {
			StopCoroutine (_rotateCoroutine);
			_rotateCoroutine = null;
		}
	}

	// Update is called once per frame
	void Update()
	{
//		_RotateGameObject.transform.Rotate(RotateSpeed * Vector3.forward);
	}

	private IEnumerator LoadingRotate(){
		while (true) {
			if (_RotateGameObject != null) {
				_RotateGameObject.transform.Rotate(RotateSpeed * Vector3.forward);
			}
			yield return new WaitForSeconds (0.1f);
		}
	}
}
