using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System;
using System.Linq;

namespace BrainBit.TinyPNG
{
    public delegate void TinyPNGCallback();
    public delegate void TinyPNGThreadCallback(TinyPngThread thread);

    public class TinyPNG
    {
        /**
         *  TinyPNG Api Key
         */
        public string TinyPngKey = "";

        /**
         * Path to files being evaluated. 
         */
        public string Path = "";

        /**
         * Whether the compressed files should overwrite origionals. 
         */
        public bool Overwrite = false;

        /**
         * GameObject to which all threads are parented in scene. 
         */
        private GameObject _root;

        /**
         * Active threads. 
         */
        private List<TinyPngThread> _threads;

        /**
        * Whether and error occured. 
        */
        private bool _hasError = false;

        /**
        * Has the process completed. 
        */ 
        private bool _finished = false;

        /**
         * Callbacks triggered once valid files have been located and just before threads are started.  
         */
        public TinyPNGCallback Begin;

        /**
        * Callback triggered when a thread is created. 
        */
        public TinyPNGThreadCallback ThreadCreate;

        /**
         * Callback triggered once all threads have completed. 
        */
        public TinyPNGCallback End;

        /**
         * Callback triggered when a thread completes. 
         */
        public TinyPNGThreadCallback ThreadFinish;

        /**
         * Root scene object under which all threads are grouped. 
         */
        public GameObject ThreadListing
        {
            get
            {
                return this._root;
            }
        }

        /**
         * Have all threads finished. 
         */
        public bool ThreadsFinished
        {
            get
            {
                return this._finished;

            }
        }

		/*
		 * Parent object for all threads.
		 */
		public GameObject Root{
			get{
						return this._root;
				}
		}

        /**
         * Evaluate project at Path and start necessary threads. 
        */
        public void ScanProject()
        {
            if ((this.TinyPngKey != null && this.TinyPngKey != String.Empty) && (this.Path != null || this.Path != String.Empty))
            {

                string path = Application.dataPath;
                if (this.Path.IndexOf("/") == 0)
                {
                    path += this.Path;
                }
                else
                {
                    path += "/" + this.Path;
                }

                if (Directory.Exists(path))
                {
                    this._root = new GameObject("TinyPNG");
                    this._root.AddComponent<TinyPngThread>();

                    if (this.Begin != null)
                    {
                        this.Begin();
                    }


                    this._threads = new List<TinyPngThread>();
                    this._processDir(path);

                }
                else
                {
                    Debug.LogError("The specified directory doesn't exist.");
                }

            }
            else
            {

                if (this.TinyPngKey == null || this.TinyPngKey == String.Empty)
                {
                    Debug.LogError("The TinyPNG API key is missing. To obtain one please visit: https://tinypng.com/developers");
                }

                if (this.Path == null || this.Path == String.Empty)
                {
                    Debug.LogError("Please specify a directory path for the images to compress.");
                }
            }

        }

        /*
         Are there active threads. 
        */
        public bool HasThreads
        {
            get
            {
                if (this._root != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /*
          Updates dispaly of threads in editor. 
        */
        public void UpdateThreadDispaly()
        {

            for (int i = 0; i < this._threads.Count; i++)
            {
                TinyPngThread thread = this._threads[i];
                thread.gameObject.name = thread.Name + ":" + thread.Progress + "%";
            }
        }

        /*
          Called when a thread complets. Updates process state and triggers relevant callbacks. 
        \param thread Completed thread
         */
        public void ThreadEnd(TinyPngThread thread)
        {
            //Callback for thread complete. 
            if (this.ThreadFinish != null)
            {
                this.ThreadFinish(thread);
            }

            //Check if all threads are done. 
            bool k = true;
            foreach (TinyPngThread t in this._threads)
            {
                if (!t.Complete)
                {
                    k = false;
                }
            }

            this._finished = k;

            //Run End callback if defined. 
            if (this._finished)
            {
                if (this.End != null)
                {
                    this.End();
                }
            }
        }

        /*
         *  Checks directory for sub directories to open and passes files for evaluation. 
        \param path Directory path. 
         */
        private void _processDir(string path)
        {
            //Process contained files
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                this._processFile(file);
            }

            //Process contained dirs
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                this._processDir(dir);
            }
        }

        /*
         * Starts threads for appropriate files. 
        \param filename Path to target file
         */
        private void _processFile(string filename)
        {
            FileInfo info = new FileInfo(filename);
            if (info.Extension == ".png" || info.Extension == ".jpg")
            {
                //Create gameobject for threaing within editor. 
                GameObject g = new GameObject("Thread");
                g.transform.parent = this._root.transform;

                TinyPngThread thread = g.AddComponent<TinyPngThread>();
                this._threads.Add(thread);

                //Configuring thread callbacks. 
                thread.ProcessData += this._writeFile;
                thread.Finish += this.ThreadEnd;

                //start thread
                thread.CompressWithWrite(this.Overwrite, this.TinyPngKey, filename);


                if(this.ThreadCreate != null)
                {
                    this.ThreadCreate(thread);
                }
            }
        }

        /*
         * Write thread bytes to file. 
        
         \param thread Thread to read bytes from
         */
        private void _writeFile(TinyPngThread thread)
        {
			FileStream file = File.Open(thread.OutPath, FileMode.Create);
            BinaryWriter binary = new BinaryWriter(file);
            binary.Write(thread.ResultBytes);
            file.Close();

        }
    }
}
