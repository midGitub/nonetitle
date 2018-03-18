using UnityEngine;
using System.Collections;

namespace CitrusFramework
{
	public class ShaderOffsetSet : MonoBehaviour
    {

		public float offset_X = 0;
		public float offset_Y = 0;
        public int Width = 0;
        public int Height = 0;

		private Material m_Mat;
		private float m_LastoffsetX;
		private float m_LastoffsetY;

		// Use this for initialization
		void Start () 
		{
			m_Mat = this.GetComponent<Renderer>().materials[0];
		}
		
		// Update is called once per frame
		void FixedUpdate () 
		{
            var floorX = Mathf.Floor(offset_X);
            var floorY = Mathf.Floor(offset_Y);

            if(floorX != offset_X || floorY != offset_Y)
                return;
            
            if(floorX != m_LastoffsetX
                || floorY != m_LastoffsetY)
			{
                m_Mat.mainTextureOffset = new Vector2(
                    floorX / (float)Width,
                    floorY / (float)Height);
			}
            m_LastoffsetX = floorX;
            m_LastoffsetY = floorY;
		}
	}
}

