using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UnityToolbarExtender.Examples
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
		}
	}

	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

		}



		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();
            Rect buttonRect;
            if (GUILayout.Button(new GUIContent("B", "Bake lights of current scene"), ToolbarStyles.commandButtonStyle))
			{

                //SceneHelper.StartScene("SampleScene");

                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new PopupExample());

                //Lightmapping.Bake();
                /*var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);*/

                //SetSuspendState(false, true, true);
            }
            if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();
        }




	}

    public class PopupExample : PopupWindowContent
    {
        bool toggleAll;
        bool toggleSP;
        bool toggleSD;
        bool foldOpen;
        String[] filePaths = Directory.GetFiles("Assets/Scenes", "*.unity");
        String[] scenePaths = new string[SceneManager.sceneCountInBuildSettings];
        bool[] scene = new bool[SceneManager.sceneCountInBuildSettings];
        int sceneCount;
        string branch;
        string cMessage;
        bool displayMessage;
        Vector2 scrollPosition;
        Vector2 pos;
        bool canBake;

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 350);
        }



        public override async void OnGUI(Rect rect)
        {
            for (int x = 0; x < SceneManager.sceneCountInBuildSettings; x++)
            {
                scenePaths[x] = SceneUtility.GetScenePathByBuildIndex(x);
            }

                editorWindow.position = new Rect(new Vector2(500,50), GetWindowSize());


            GUILayout.Label("Baking options", EditorStyles.boldLabel);
            
            GUILayout.Label("All scenes will be baked if closed");
            foldOpen = EditorGUILayout.Foldout(foldOpen, "Scenes");
            if (foldOpen)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
                for (int x = 0; x < SceneManager.sceneCountInBuildSettings; x++)
                {
                    string name = scenePaths[x].Remove(0, 14);
                    scene[x] = EditorGUILayout.Toggle(name, scene[x]);
                    
                }
                GUILayout.EndScrollView();
            }
            

            toggleSD = EditorGUILayout.Toggle("Shut down PC after bake", toggleSD);
            //GUILayout.Space(20);

            GUILayout.Label("Commit Message:");
            cMessage = EditorGUILayout.TextField(cMessage);

            

            if (GUILayout.Button("Bake Lights", GUILayout.Width(194)) && cMessage != null)
            {
                displayMessage = true;
                canBake = true;

            }

            if (displayMessage)
            {
                GUILayout.Label("Baking lightmaps...\nThis might take a while");
            }

            if (canBake)
            {
                canBake = false;
                await Task.Delay(TimeSpan.FromSeconds(1));
                bakeLights();
            }
        }

        

        public override void OnOpen()
        {
            //UnityEngine.Debug.Log("Popup opened: " + this);
        }

        public override void OnClose()
        {
            //UnityEngine.Debug.Log("Popup closed: " + this);
        }

        public void bakeLights()
        {
            File.WriteAllText("Assets/UILightBaker/param.txt", String.Empty);
            if (branch == null)
                branch = "empty";
            File.WriteAllText("Assets/UILightBaker/param.txt", cMessage + "," + toggleSD.ToString());

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (foldOpen)
                {
                    if (scene[i])
                    {
                        EditorSceneManager.OpenScene(scenePaths[i]);
                        Lightmapping.Bake();
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    }
                }

                else if (!foldOpen)
                {
                    EditorSceneManager.OpenScene(scenePaths[i]);
                    Lightmapping.Bake();
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }

            }


            Process.Start(Path.GetFullPath("Assets/UILightBaker/Github upload Script.bat"));
        }

    }


    static class SceneHelper
	{
		static string sceneToOpen;

		public static void StartScene(string sceneName)
		{
			if(EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null ||
			    EditorApplication.isPlaying || EditorApplication.isPaused ||
			    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				// need to get scene via search because the path to the scene
				// file contains the package version so it'll change over time
				string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
				if (guids.Length == 0)
				{
					UnityEngine.Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
					EditorSceneManager.OpenScene(scenePath);
					EditorApplication.isPlaying = true;
				}
			}
			sceneToOpen = null;
		}
	}

 
}