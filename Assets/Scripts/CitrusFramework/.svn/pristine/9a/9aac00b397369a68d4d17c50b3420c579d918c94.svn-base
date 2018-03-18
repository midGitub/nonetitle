using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJson;
using PomeloProtobuf;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Text;
using CielaSpike;
using System.Threading;
using System.IO.Compression;

namespace CitrusFramework
{
	public class NetworkManager
	{
		private Protobuf m_Protobuf;
		private bool   m_InitOk = false;
		private float  m_TimeOutTime = 30;

        private Dictionary<ServerType, string> m_ServerUrlDic = new Dictionary<ServerType, string>(new ServerTypeCompare());
		private List<HttpObject> m_InProgramHttp = new List<HttpObject>();
		private List<string> m_DisposeHttpObj = new List<string>(); 
		private Queue<JsonObject> m_ReceiveCmd = new Queue<JsonObject>();
        private Dictionary<string, Callback<byte[], string>> m_NormalHttpObj = new Dictionary<string, Callback<byte[], string>>();
        private Queue<UrlImageObject> m_UrlImageObjReceive = new Queue<UrlImageObject>();
		private MonoBehaviour m_HostMono;

		public	bool InitOk{ get{ return m_InitOk; }}
		
        public void Init(string GameServerUrl, HandlerFactoryBase Handler, MonoBehaviour HostMono)
		{
			if(!m_InitNetworkOk)
			{
				m_HostMono = HostMono;
                
				//register Handler for c#
				m_HandlerFactory = Handler;
                InitProtobuf();
                SetServerURL(ServerType.Game, GameServerUrl);

                m_InitNetworkOk = true;
                m_InitOk = true;
			}
		}

        public void SetServerURL(ServerType type, string url)
        {
            m_ServerUrlDic[type] = url;
        }

		private void InitProtobuf()
		{
			TextAsset text = Resources.Load("json/protos") as TextAsset;
			
			JsonObject protos = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(text.text);
			m_Protobuf = new Protobuf(protos, protos);
		}
		
		// Update is called once per frame
		public void Update () 
		{
			if(m_InitOk)
			{
				JsonObject cmdObj = null;
                UrlImageObject imgObj = null;
				lock(m_lock)
				{
					if(m_ReceiveCmd.Count > 0)
					{
						cmdObj = m_ReceiveCmd.Dequeue();
					}
                    if(m_UrlImageObjReceive.Count > 0)
                    {
                        imgObj = m_UrlImageObjReceive.Dequeue();
                    }
				}
				if(cmdObj != null)
				{
					CallExecuteByJson(cmdObj);
				}
                List<HttpObject> clearobj = null ;
                for(int i = 0; i < m_DisposeHttpObj.Count; i++)
                {
                    lock(m_lock)
                    {
                        for(int j = 0; j < m_InProgramHttp.Count; j++)
                        {
                            if(m_InProgramHttp[j].UUID == m_DisposeHttpObj[i])
                            {
                                if(clearobj == null)
                                {
                                    clearobj = new List<HttpObject>();
                                }
								clearobj.Add(m_InProgramHttp[j]);
                            }
                        }
//                        m_InProgramHttp.RemoveAll(s => s.UUID == m_DisposeHttpObj[i]);
                    }
                }
                for(int i = 0; i < m_InProgramHttp.Count; i++)
                {
                    HttpObject obj = m_InProgramHttp[i];
                    if(!obj.IsDispose)
                    {
                        obj.Update(Time.deltaTime);
                        if (obj.Timer < 0)
                        {
                            GameDebug.LogWarning("Cmd Time Out, id:" + obj.Id);
                            obj.Dispose();
                            if (clearobj == null)
                            {
                                clearobj = new List<HttpObject>();
                            }
                            clearobj.Add(m_InProgramHttp[i]);
                            ProtocolMessage cmd = GetCmdById(obj.Id);
                            if (cmd != null)
                            {
                                cmd.code = CmdCode.TimeOut;
                                INetworkHandlerBase Handler = m_HandlerFactory.GetHandler(cmd);
                                if (Handler != null)
                                {
                                    Handler.Execute(cmd);
                                }
                            }
                        }
                    }
                }
                if(clearobj != null)
                {
                    for(int j = 0; j < clearobj.Count; j++)
                    {
                        lock(m_lock)
                        {
                            m_InProgramHttp.Remove(clearobj[j]);
                        }
                    }
                }

                if(imgObj != null)
                {
                    imgObj.DoCallback();
                }
			}
		}

        public bool Send(ProtocolMessage cmd)
        {
            bool result = false;
            if(m_ServerUrlDic.ContainsKey(cmd.Server))
            {
                if(Application.internetReachability != NetworkReachability.NotReachable)
                {
                    if(cmd != null)
                    {
                        if(IsCanSendMulti(cmd))
                        {
                            string url = m_ServerUrlDic[cmd.Server] + cmd.GetId();

                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                            HttpObject obj = new HttpObject(cmd.GetId(), req, m_TimeOutTime);
                            string uuid = obj.GetHashCode().ToString();
                            obj.UUID = uuid;
                            cmd.uuid = uuid;

                            ArchiveO or = new ArchiveO(cmd);
                            or.DoSomething(cmd);

                            lock(m_lock)
                            {
                                m_InProgramHttp.Add(obj);
                            }

                            m_HostMono.StartCoroutineAsync(SendAsync(cmd, req, url));
//                            GameDebug.Log("Send Cmd Id :" + cmd.GetId());
                            result = true;
                        }
                        else
                        {
                            GameDebug.Log("The Same Cmd HadSend Id :" + cmd.GetId());
                        }
                    }
                }
                else
                {
                    GameDebug.LogWarning("Internet Is NotReachable!!!");
                }
            }
            else
            {
                GameDebug.LogError("GameServer is NULL, pls SetServerURL First, ServerType:" + cmd.Server);
            }
            return result;
        }

		private bool IsCanSendMulti(ProtocolMessage cmd)
		{
			bool result = true;
			if(!cmd.CanSendMulti)
			{
				lock(m_lock)
				{
					for(int i = 0; i < m_InProgramHttp.Count; i++)
					{
						if(m_InProgramHttp[i] != null && m_InProgramHttp[i].Id == cmd.GetId())
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

        private IEnumerator SendAsync(ProtocolMessage cmd, HttpWebRequest req, string url)
		{
 			if(url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
			{
				ServicePointManager.ServerCertificateValidationCallback = 
					new RemoteCertificateValidationCallback(CheckValidationResult);
			}

            if((cmd.SendType == CmdSendType.Protobuf && !string.IsNullOrEmpty(cmd.JSONKEY_REQUEST)) ||
                cmd.SendType == CmdSendType.Json)
			{
				byte[] bs = new byte[2];
				lock(m_lock)
				{
                    if(cmd.SendType == CmdSendType.Json)
                    {
                        object jsonData;

                        if(cmd.TryGetValue("JsonData", out jsonData))
                        {
                            bs = Encoding.UTF8.GetBytes(MiniJSON.Json.Serialize(jsonData));
                        }
                        else
                        {
                            bs = Encoding.UTF8.GetBytes(cmd.ToString());
                        }
                    }
                    else
                    {
                        bs = encode(cmd.JSONKEY_REQUEST, cmd);
                    }
				}

				if(bs != null)
				{
					req.Method = "POST";
//					req.ContentType = "application/octet-stream";
                    if(cmd.SendType == CmdSendType.Protobuf)
                    {
                        req.Headers.Add("Citrusjoy-Proto", cmd.JSONKEY_REQUEST);
                    }
                    else if(cmd.SendType == CmdSendType.Json)
                    {
                        req.ContentType = "application/json";
                    }
					req.ContentLength = bs.Length;

					GameDebug.Log("", "Network", "Send Cmd Id :" + cmd.GetId() + " lenght: " + req.ContentLength + " b.");
//					GameDebug.Log("", "Network", System.Text.Encoding.Default.GetString(bs));
					using(Stream reqStream = req.GetRequestStream())
					{
						reqStream.Write(bs, 0, bs.Length);
					}
				}
				else
				{
					yield return Ninja.JumpToUnity;
					GameDebug.LogWarning("Send Cmd Error bs is null :" + cmd.GetId());
					yield break;
				}
			}

			yield return req.BeginGetResponse(new AsyncCallback(ReadGetCallback), req);
			yield return Ninja.JumpToUnity;
		}

		private void ReadGetCallback(IAsyncResult asynchronousResult)
		{
			Receive(asynchronousResult);
		}
		
        private void Receive(IAsyncResult asynchronousResult)
		{
			HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
			string CmdId = request.Address.AbsolutePath.Substring(1);
			try
			{
                if(request.ContentType == "application/json")//json
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream st = response.GetResponseStream();

                        if (response.ContentEncoding.ToLower().Contains("gzip"))
                            st = new GZipStream(st, CompressionMode.Decompress);

                        StreamReader sr = new StreamReader(st, Encoding.Default);
                        string resultjson = sr.ReadToEnd();
                        JsonObject json = new JsonObject ();

                        lock(m_lock)
                        {
                            json = (SimpleJson.JsonObject)SimpleJson.SimpleJson.DeserializeObject (resultjson);
                        }

                        object uuid;
                        if(json.TryGetValue("HTTPUUID", out uuid))
                        {
                            DisposeHttpObj(uuid.ToString());
                        }

                        lock(m_lock)
                        {
                            m_ReceiveCmd.Enqueue(json);
                        }
                    }
                    response.Close ();
                }
                else //protobuf
                {
                    ProtocolMessage message = GetCmdById(CmdId);
                    
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);

                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        using(Stream streamResponse = response.GetResponseStream())
                        {
                            using(StreamReader streamRead = new StreamReader(streamResponse, Encoding.UTF8))
                            {
                                string resultjson = streamRead.ReadToEnd();

                                byte[] bpath = Convert.FromBase64String(resultjson);
                                JsonObject json = new JsonObject();
                                lock(m_lock)
                                {
                                    json = decode(message.JSONKEY_RESPOND, bpath);
                                }
                                object uuid;
                                if(json.TryGetValue("HTTPUUID", out uuid))
                                {
                                    DisposeHttpObj(uuid.ToString());
                                }
                                lock(m_lock)
                                {
                                    m_ReceiveCmd.Enqueue(json);
                                }
                            }
                        }
                    }
                    response.Close();
                }
			}
			catch(Exception ex)
			{
				Debug.LogError("respond:" + ex.Message);		
				JsonObject Errorcmd = new JsonObject();
				Errorcmd.Add("CmdId", CmdId);
				Errorcmd.Add("Code", (int)CmdCode.RespondError);
				lock(m_lock)
				{
					m_ReceiveCmd.Enqueue(Errorcmd);
				}
			}
		}

        public void GetUrlImage(string url, Callback<byte[], string> callback)
        {
            if(m_HostMono != null)
            {
                if(Application.internetReachability != NetworkReachability.NotReachable)
                {
                    if(!string.IsNullOrEmpty(url) && !m_NormalHttpObj.ContainsKey(url))
                    {
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                        m_NormalHttpObj[url] = callback;
                        m_HostMono.StartCoroutineAsync(GetUrlImageAsync(req));
//                        GetUrlImageAsync(req);
                    }
                }
                else
                {
                    GameDebug.LogWarning("Internet Is NotReachable!!!");
                }
            }
        }

        private IEnumerator GetUrlImageAsync(HttpWebRequest req)
        {
            if(req.Address.AbsoluteUri.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = 
                    new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            yield return req.BeginGetResponse(new AsyncCallback(GetUrlImageCallback), req);
        }

        private void GetUrlImageCallback(IAsyncResult asynchronousResult)
        {
            GetUrlImageReceive(asynchronousResult);
        }

        private void GetUrlImageReceive(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            string url = request.Address.AbsoluteUri;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    using(BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                    {
                        Byte[] bpath = reader.ReadBytes(1 * 1024 * 1024 * 10);
                        if(m_NormalHttpObj.ContainsKey(url))
                        {
                            lock(m_lock)
                            {
                                UrlImageObject receive = new UrlImageObject();
                                receive.Url = url;
                                receive.Result = bpath;
                                receive.Callback = m_NormalHttpObj[url];
                                m_UrlImageObjReceive.Enqueue(receive);
                                m_NormalHttpObj.Remove(url);
                            }
                        }
                    }
                }

                response.Close();
            }
            catch(Exception ex)
            {
                Debug.LogError("GetImageError:" + ex.Message);
                lock(m_lock)
                {
                    m_NormalHttpObj.Remove(url);
                }
            }
        }

	
		private void DisposeHttpObj(string uuid)
		{
			lock(m_lock)
			{
				for(int i = 0; i < m_InProgramHttp.Count; i++)
				{
					if(uuid == m_InProgramHttp[i].UUID)
					{
						m_InProgramHttp[i].Dispose();
						m_DisposeHttpObj.Add(uuid);
						return;
					}
				}
			}
		}

		public void CallExecuteByJson(JsonObject obj)
		{
			ProtocolMessage cmd = GetCmdByReceiveJson(obj);
			if(cmd != null)
			{
				GameDebug.Log("Receive cmd id:" + cmd.GetId());
				m_HandlerFactory.GetHandler(cmd).Execute(obj);
			}
		}
		
		public ProtocolMessage GetCmdByReceiveJson(JsonObject obj)
		{
			System.Object code = null;
			string id = "";
			if(obj.TryGetValue("CmdId", out code))
			{
				id = code.ToString();
			}
			return GetCmdById(id);
		}
		
		public ProtocolMessage GetCmdById(string Id)
		{
			return (ProtocolMessage)m_HandlerFactory.ProtocolFactory.CreateProtocolMessage(Id);
		}

		public byte[] encode(string key, JsonObject json)
		{
			byte[] bytes = m_Protobuf.encode(key, json);
			return bytes;
		}
		
		public JsonObject decode(string key, byte[] msg)
		{
			return m_Protobuf.decode(key, msg);		
		}

		private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}
		
		public void DebugCode(uint code)
		{
			GameDebug.LogError(((ErrorCode)code).ToString());
		}
		
		private bool m_InitNetworkOk = false;
		private HandlerFactoryBase m_HandlerFactory;
		
		private NetworkManager(){}
		public static NetworkManager Instance { get{ return m_Instance;} }
		private static NetworkManager m_Instance = new NetworkManager();
		private static object m_lock = new object();
	}

    public enum ServerType : int
    {
        Game    = 0,
        Rank    = 1,
        Pay     = 2,
        BI      = 3
    }

    public class ServerTypeCompare : IEqualityComparer<ServerType>
    {
        public bool Equals(ServerType x, ServerType y)
        {
            return x == y;
        }

        public int GetHashCode(ServerType obj)
        {
            return (int)obj;
        }
    }
}