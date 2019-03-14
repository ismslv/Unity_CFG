//Uncomment if console is installed
//#define CONSOLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFGLoader : MonoBehaviour {

    public string configFile = "Settings";
    static string _configFile;

    private void Awake()
    {
        _configFile = configFile;
        LoadConfig();
    }

    private void Start()
    {
#if CONSOLE
        Console.Register(new Command()
        {
            name = "cfgreload",
            response = "Reloaded configuration!",
            action = (s) =>
            {
                CFGLoader.LoadConfig();
            }
        });
#endif
    }

    public static void LoadConfig()
    {
        CFG.Load(Application.dataPath + "/" + _configFile + ".ini");
    }
}
