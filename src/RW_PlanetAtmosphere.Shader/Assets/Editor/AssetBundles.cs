#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class AssetBundles
    {
        /// <summary>
        /// 打包生成所有的AssetBundles（包）
        /// </summary>
        [MenuItem("Assets/Build All Asset Bundles")]
        public static void BuildAllAB() {
            // 打包AB输出路径
            string strABOutPAthDir = "../../AssetBundles";

            // 获取“StreamingAssets”文件夹路径（不一定这个文件夹，可自定义）
            // strABOutPAthDir = Application.streamingAssetsPath;

            // 判断文件夹是否存在，不存在则新建
            if (Directory.Exists(strABOutPAthDir) == false)
            {
                Directory.CreateDirectory(strABOutPAthDir);
            }
            strABOutPAthDir = "../../AssetBundles/Windows";
            if (Directory.Exists(strABOutPAthDir) == false)
            {
                Directory.CreateDirectory(strABOutPAthDir);
            }
            BuildPipeline.BuildAssetBundles(strABOutPAthDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
            DirectoryInfo diWindows = new DirectoryInfo(strABOutPAthDir);
            Debug.Log(diWindows.FullName);
            foreach (FileInfo file in diWindows.GetFiles())
            {
                if (!file.Name.ToLower().Contains("_windows"))
                {
                    Debug.Log(file.Name);
                    file.Delete();
                }
            }

            strABOutPAthDir = "../../AssetBundles/Linux";
            if (Directory.Exists(strABOutPAthDir) == false)
            {
                Directory.CreateDirectory(strABOutPAthDir);
            }
            BuildPipeline.BuildAssetBundles(strABOutPAthDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);
            DirectoryInfo diLinux = new DirectoryInfo(strABOutPAthDir);
            Debug.Log(diWindows.FullName);
            foreach (FileInfo file in diLinux.GetFiles())
            {
                if (!file.Name.ToLower().Contains("_linux"))
                {
                    Debug.Log(file.Name);
                    file.Delete();
                }
            }

            strABOutPAthDir = "../../AssetBundles/MacOS";
            if (Directory.Exists(strABOutPAthDir) == false)
            {
                Directory.CreateDirectory(strABOutPAthDir);
            }
            BuildPipeline.BuildAssetBundles(strABOutPAthDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
            DirectoryInfo diMacOS = new DirectoryInfo(strABOutPAthDir);
            Debug.Log(diWindows.FullName);
            foreach (FileInfo file in diMacOS.GetFiles())
            {
                if (!file.Name.ToLower().Contains("_macos"))
                {
                    Debug.Log(file.Name);
                    file.Delete();
                }
            }

            // 打包生成AB包 (目标平台根据需要设置即可)

        }
    }

    class MyWindow : EditorWindow
    {
        static List<string> text;


        [MenuItem("Window/My Window")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(MyWindow));
            text = new List<string>();
            Texture2D[] t = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach(Texture2D texture in t)
            {
                Debug.unityLogger.logEnabled = false;
                GUIContent gc = EditorGUIUtility.IconContent(texture.name);
                Debug.unityLogger.logEnabled = true;
                if(gc != null)
                {
                    text.Add(texture.name);
                }
            }
        }
        public Vector2 scrollPosition;
        void OnGUI()
        {

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            //鼠标放在按钮上的样式
            foreach (MouseCursor item in Enum.GetValues(typeof(MouseCursor)))
            {
                GUILayout.Button(Enum.GetName(typeof(MouseCursor), item));
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), item);
                GUILayout.Space(10);
            }


            //内置图标
            for (int i = 0; i < text.Count; i += 8)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 8; j++)
                {
                    int index = i + j;
                    if (index < text.Count)
                    {
                        Texture2D texture = (Texture2D)EditorGUIUtility.IconContent(text[index]).image;
                        if(texture != null)
                        {
                            if(GUILayout.Button(texture, GUILayout.Width(128), GUILayout.Height(128)))
                            {
                                RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
                                RenderTexture.active = rt;
                                Graphics.Blit(texture,rt,Vector2.one,Vector2.zero);
                                texture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                                texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                                texture.Apply();
                                RenderTexture.active = null;
                                rt.Release();
                                byte[] bytes = texture.EncodeToPNG();
                                string path = Application.dataPath + "/textureOutput/";
                                if(!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                path += text[index] + ".png";
                                FileStream file = File.Open(path, FileMode.OpenOrCreate);
                                file.Write(bytes, 0, bytes.Length);
                                file.Flush();
                                file.Close();
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }




            GUILayout.EndScrollView();
        }
    }
}
#endif