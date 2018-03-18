using UnityEngine;
using System.Collections;
using System.Net;

namespace CitrusFramework
{
	public class HttpObject
	{
		private string  m_Id;
		private HttpWebRequest m_Request;
		private float   m_Timer;
		private string  m_UUID;
        private bool    m_IsDispose = false;

		public  string Id{ get{ return m_Id; }}
		public	HttpWebRequest Request{ get{ return m_Request; }}
		public	float Timer{ get{ return m_Timer; }}
		public	string UUID{ get{ return m_UUID;} set{ m_UUID = value; }}
        public  bool IsDispose{ get{ return m_IsDispose; }}

		public HttpObject(string id, HttpWebRequest request, float time)
		{
			m_Id = id;
			m_Request = request;
			m_Timer = time;
		}

		public void Update(float time)
		{
			m_Timer -= time;
		}

		public void Dispose()
		{
            m_IsDispose = true;
			if(m_Request != null)
			{
				m_Request.Abort();
			}
		}
	}
}

