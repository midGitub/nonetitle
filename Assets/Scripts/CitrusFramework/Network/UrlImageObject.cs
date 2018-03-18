using UnityEngine;
using System.Collections;

namespace CitrusFramework
{
    public class UrlImageObject
    {
        public string Url;
        public byte[] Result;
        public Callback<byte[], string> Callback;

        public void DoCallback()
        {
            if(Callback != null)
            {
                Callback(Result, Url);
            }
        }
    }
}