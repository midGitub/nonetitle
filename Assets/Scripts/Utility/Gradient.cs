using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gradient : BaseMeshEffect {

	public Color32 _topColor = Color.white;
	public Color32 _bottomColor = Color.yellow;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void ModifyMesh(VertexHelper vh){
		if (!IsActive ())
			return;

		List<UIVertex> vertexList = new List<UIVertex> ();
		vh.GetUIVertexStream (vertexList);
		int count = vertexList.Count;

		ApplyGradient (vertexList, 0, count);
		vh.Clear ();
		vh.AddUIVertexTriangleStream (vertexList);
	}

	private void ApplyGradient(List<UIVertex> vertexList, int start, int end){
		float bottomY = vertexList [0].position.y;
		float topY = vertexList [0].position.y;
		for (int i = start; i < end; ++i) {
			float y = vertexList [i].position.y;
			if (y > topY)
				topY = y;
			else if (y < bottomY)
				bottomY = y;
		}

		float uiElementHeight = topY - bottomY;
		for (int i = start; i < end; ++i) {
			UIVertex uiVertex = vertexList [i];
			uiVertex.color = Color32.Lerp (_bottomColor, _topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
			vertexList [i] = uiVertex;
		}
	}

}
