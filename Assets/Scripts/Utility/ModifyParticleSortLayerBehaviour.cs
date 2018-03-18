using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyParticleSortLayerBehaviour : MonoBehaviour {
	// 层级参数
	public int _layerOrder = -1;
	// 该节点下所有的粒子系统
	private ParticleSystemRenderer[] _particleSystemRenders;

	// Use this for initialization
	void Start () {
		_particleSystemRenders = gameObject.GetComponentsInChildren<ParticleSystemRenderer> (true);
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < _particleSystemRenders.Length; ++i) {
			_particleSystemRenders [i].sortingOrder = _layerOrder;
		}
	}
}
