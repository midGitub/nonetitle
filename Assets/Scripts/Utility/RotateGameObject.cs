using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGameObject : MonoBehaviour
{
	public float Speed = 100;
	public Vector3 Dir = -Vector3.forward;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		this.transform.Rotate(Speed * Dir * Time.deltaTime);
	}
}
