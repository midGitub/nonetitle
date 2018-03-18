using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace BrainBit.TinyPNG
{
		public class TinyPngEditor : EditorWindow
		{
				public TinyPNG tiny = new TinyPNG ();
				
				[MenuItem ("Window/TinyPNG")]
				static void Init ()
				{
						// Get existing open window or if none, make a new one:
						TinyPngEditor window = (TinyPngEditor)EditorWindow.GetWindow (typeof(TinyPngEditor));
			window.tiny.End += () => {
				DestroyImmediate(window.tiny.Root);

			};
				}
		
				void OnGUI ()
				{
						this.tiny.TinyPngKey = EditorGUILayout.TextField ("API Key", this.tiny.TinyPngKey);
						this.tiny.Path = EditorGUILayout.TextField ("Path:", this.tiny.Path);
						this.tiny.Overwrite = EditorGUILayout.Toggle ("Overwrite:", this.tiny.Overwrite);

						if (GUILayout.Button ("Start")) {
								this.tiny.ScanProject ();
						}
				}

				void Update ()
				{
						if (this.tiny.HasThreads) {
								this.tiny.UpdateThreadDispaly ();	
								if (this.tiny.ThreadsFinished) {
										DestroyImmediate (this.tiny.ThreadListing);
										AssetDatabase.Refresh ();
								}
						}
				}
		}
}
