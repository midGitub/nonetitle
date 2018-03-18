using UnityEngine;
using System.Collections;
using System.Text;
using System.Net;
using System;
using System.IO;
using System.Collections.Generic;
using JsonFx.Json;

namespace BrainBit.TinyPNG
{
    public class TinyPngThread : MonoBehaviour
    {
        public TinyPNGThreadCallback Finish;
        public TinyPNGThreadCallback Error;
        public TinyPNGThreadCallback ProcessData;
        private string _error;
        private bool _log = true;
        private WWW _send;
        private WWW _recieve;
        private bool _complete = false;
        private FileInfo _info;
        private byte[] _resultBytes = new byte[]{};
        private string _url = "https://api.tinypng.com/shrink";
        private string _outPath;
        private string _threadName;

        /*
         * Should debug messages be written.
         */
        public bool Logging
        {
            get
            {
                return this._log;
            }

            set
            {
                this._log = value;
            }
        }

        /*
         * Path compressed image will be written to. 
         */
        public string OutPath
        {
            get
            {
                return this._outPath;
            }

            set
            {
                this._outPath = value;
            }
        }

        /*
         * Is thread complete.
         */
        public bool Complete
        {
            get
            {
                return this._complete;
            }
        }

        /*
         * Thread name. 
         */
        public string Name
        {
            get
            {
                if(this._threadName != null)
                {
                    return this._threadName;
                }else if(this._info != null)
                {
                    return this._info.Name;
                }
                else
                {
                    return "";
                }
            }

            set
            {
                this._threadName = value;
            }
        }

        /*
         * Error returned from TinyPNG server or Client connection.
         */
        public string ErrorMessage
        {
            get
            {
                return this._error;
            }
        }

        /*
         * Bytes return from TinyPNG server. 
         */
        public byte[] ResultBytes
        {
            get
            {
                return this._resultBytes;
            }
        }

        /*
         * Thread Progress
         */
        public float Progress
        {
            get
            {

                float result = 0;

                if (this._send != null)
                {
                    result = this._send.progress * 100;
                }

                if (this._send != null && this._recieve != null)
                {
                    result = ((this._send.progress + this._recieve.progress) / 2) * 100;
                }

                return result;
            }
        }


        /*
         * Compress file and write returned bytes to disk. Bytes are also cached in ResultBytes.
         \param overwrite Whether origional file should be overwritten. 
         \param apiKey TinyPNG API Key. 
         \param inPath Path to target image.
         */
        public bool CompressWithWrite(bool overwrite, string apiKey, string inPath)
        {
            if (File.Exists(inPath) && !String.IsNullOrEmpty(apiKey))
            {
                this._info = new FileInfo(inPath);
                this._outPath = inPath;
                if (!overwrite)
                {
                    this._outPath = this._info.FullName.Replace(this._info.Extension, "-tiny" + this._info.Extension);
                }

                byte[] bytes = this._readBytes(inPath);
                StartCoroutine(this._compress(apiKey, bytes));
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
         * Compress file. Returned bytes are available via ResultBytes
          \param apiKey TinyPNG API key. 
          \param inPath Path to target image. 
         */
        public bool Compress(string apiKey, string inPath)
        {
            if (File.Exists(inPath) && !String.IsNullOrEmpty(apiKey))
            {
                byte[] bytes = this._readBytes(inPath);
                StartCoroutine(this._compress(apiKey, bytes));
                return true;
            }
            else
            {
                return false;
            }
        }

		/*
		 * Destroy thread. 
		 */
		public void Destroy()
		{
			Destroy (this.gameObject);	
		}

        /*
         * Handles image compression. 
         \param apiKey TinyPNG API key. 
         \param bytes Bytest passed to TinyPNG server. 
         */
        private IEnumerator _compress(string apiKey, byte[] bytes)
        {
            
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes("api:" + apiKey));
            headers.Add(HttpRequestHeader.Authorization.ToString(), "Basic " + auth);

            this._send = new WWW(this._url, bytes, headers);

            //Log thread start. 
            if (this._log) { Debug.Log("Starting " + this.Name); }
            yield return this._send;

            if (this._send.text != null)
            {
                HttpResponse response = JsonReader.Deserialize<HttpResponse>(this._send.text);
                if (response.error != null)
                {
                    //Log errors and trigger error handling callback.

                    this._error = response.error + "" + response.message;

                    if(this._log){Debug.LogError(this._error);};
                    if(this.Error != null)
                    {
                        this.Error(this);
                    }
                }
                else
                {
                    
                    if (this._log) { Debug.Log("Processing " + this.Name + "..."); }
                    
                    //Pull down completed image. 
                    this._recieve = new WWW(this._send.responseHeaders["LOCATION"]);
                    yield return this._recieve;

                    //Load bytes.
                    this._resultBytes = this._recieve.bytes;

                    if(this.ProcessData != null)
                    {
                        this.ProcessData(this);
                    }
                }
            }
            else
            {
                this._error = "No response.";

                if (this._log) { Debug.LogError("Error compressing " + this.Name); }
                if(this.Error != null)
                {
                    
                    this.Error(this);
                }
            }

            this._complete = true;
            if(this.Finish != null)
            {
                this.Finish(this);
            }
            yield return null;
        }

        
        /*
         Turns file into byte array
         \param filename Path to file. 
         */
        private byte[] _readBytes(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Create a byte array of file stream length
            byte[] ImageData = new byte[fs.Length];

            //Read block of bytes from stream into the byte array
            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            return ImageData; //return the byte data
        }

        
        public class HttpResponse
        {
            public HttpResponseInput input;
            public HttpResponseOutput output;
            public string error;
            public string message;
        }

        public class HttpResponseInput
        {
            public string size;
            public string type;
        }

        public class HttpResponseOutput
        {
            public string size;
            public string type;
            public string ratio;
        }
    }
}
