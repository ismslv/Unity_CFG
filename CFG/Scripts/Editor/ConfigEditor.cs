using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FMLHT.Config {

[CustomEditor(typeof(CFGLoader))]
public class ConfigEditor : Editor
{
    static CFGLoader manager;

    [MenuItem("FMLHT/Config/Add to scene")]
    public static void AddPrefab() {
        if (Editor.FindObjectOfType<CFGLoader>() == null) {
            UnityEngine.Object prefab = Resources.Load("Config");
            var newObj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject obj = (GameObject)newObj;
            manager = obj.GetComponent<CFGLoader>();
            obj.name = "Config";
            var core = GameObject.Find("Core");
            if (core == null) {
                core = new GameObject();
                core.name = "Core";
            }
            obj.transform.SetParent(core.transform);
        } else {
            Debug.Log("There is already one Config Loader in this scene!");
        }
    }

    [MenuItem("FMLHT/Config/Create base config file")]
    public static void CreateFileBase() {
        CreateFile("");
    }

    [MenuItem("FMLHT/Config/Create additional config file")]
    public static void CreateFileAdditional() {
        PromptForName window = new PromptForName();
        window.maxSize = window.minSize = new Vector2(300, 50);
        window.titleContent = new GUIContent("New Config");
        window.nameNew = "Custom";
        window.ShowUtility();
    }

    [MenuItem("FMLHT/Config/Pack base file")]
    public static void PackBaseFile() {
        if (manager == null) {
            Debug.Log("You have no Config Loader in scene!");
            return;
        }
        if (manager.BaseFile == "") {
            Debug.Log("You have no Base file!");
            return;
        }
        var dir = UnityEngine.Application.dataPath + "/Resources/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        dir += "Config/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var fileSrc = UnityEngine.Application.dataPath + "/" + manager.BaseFile;
        var fileDest = dir + manager.configFile + ".txt";
        if (!File.Exists(fileSrc)) {
            Debug.Log("Base config file is not found!");
            return;
        }
        var data = File.ReadAllText(fileSrc);
        File.WriteAllText(fileDest, data);
        Debug.Log("Base config successfully packed!");
    }

    public static void CreateFile(string filename) {
        string basefilename = "Config";
        string ext = ".fmcfg";
        if (manager != null) {
            basefilename = manager.configFile;
            ext = "." + manager.extension;
        }
        string path = UnityEngine.Application.dataPath + "/" + basefilename + filename + ext;
        if (!File.Exists(path)) {
            File.Create(path);
            if (filename == "") {
                manager.BaseFile = basefilename + filename + ext;
            }
        } else {
            Debug.Log("File " + path + " already exists!");
        }
    }

    public override void OnInspectorGUI() {
        manager = (CFGLoader)target;
        DrawDefaultInspector();
        if (manager.configFile != "" && manager.extension != "") {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create base")) {
                CreateFileBase();
            }
            if (GUILayout.Button("Create additional")) {
                CreateFileAdditional();
            }
            if (manager.BaseFile != "") {
                if (GUILayout.Button("Pack base config")) {
                    PackBaseFile();
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public class PromptForName : EditorWindow
    {
        public string editorWindowText = "Choose a config name: ";
        public string nameNew = "";
 
        void OnGUI()
        {
            string inputText = EditorGUILayout.TextField(editorWindowText, nameNew);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create new config")) {
                CreateFile("_" + nameNew);
                Close();
            }
 
            if (GUILayout.Button("Abort"))
                Close();
            GUILayout.EndHorizontal();
        }
    }
}

}