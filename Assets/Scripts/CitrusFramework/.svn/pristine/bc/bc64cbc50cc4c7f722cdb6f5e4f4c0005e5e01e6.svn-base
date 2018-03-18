using UnityEngine;
using System.Collections;
using BrainBit.TinyPNG;
using System;

public class ScriptingExample : MonoBehaviour {

    public string ApiKey;
    private string _path;
    private TinyPngThread _thread;
    private Texture2D _texture;
    private MeshRenderer _renderer;
    
    public void Awake()
    {
        this._path = Application.dataPath + "/unitytinypng/Example/Img/BrainBitStudios.png";

        //Get a refrence to the mesh renderer.
        if(this.gameObject.GetComponent<MeshRenderer>())
        {
            this._renderer = this.gameObject.GetComponent<MeshRenderer>();
        }

        //Create the new tinyPNG thread. 
        GameObject n = new GameObject("New Thread");
        this._thread = n.AddComponent<TinyPngThread>();
        n.transform.parent = this.transform;
        
    }

    public void Update()
    {
        //Once we have the compressed image, dress the cube up. 
       if(this._texture != null && this._renderer != null)
       {
           this._renderer.material.SetTexture(0, this._texture);
       }
    }

    
    public void OnGUI()
    {
        //When the button is clicked configure the thread and run.
        if(GUILayout.Button("Start") && !String.IsNullOrEmpty(this.ApiKey))
        {
            this._thread.ProcessData += this._processTexture;
            this._thread.Finish += this._cleanUp;
            this._thread.Name = "Example Thread";
            this._thread.Logging = true;
            this._thread.Compress(this.ApiKey, this._path);
        }
    }


    private void _processTexture(TinyPngThread thread)
    {
        //Check if we have bytes. 
        if(thread.ResultBytes.Length > 0)
        {
            this._texture = new Texture2D(40, 40);
            this._texture.LoadImage(thread.ResultBytes);
        }
    }

    private void _cleanUp(TinyPngThread thread)
    {
        //Destory the old thread and setup a new one. 
        Destroy(this._thread);
    }
}
