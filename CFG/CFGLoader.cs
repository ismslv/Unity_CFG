#define CONSOLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace FMLHT.Config {

public class CFGLoader : MonoBehaviour {

    public string configFile = "Config";
    public string extension = "fmcfg";
    public bool loadAdditional = true;
    public bool loadOnAwake = true;

    private void Awake()
    {
        if (loadOnAwake)
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
                LoadConfig();
            }
        });
#endif
    }

    public void LoadConfig()
    {
        string cfgMain = UnityEngine.Application.dataPath + "/" + configFile + "." + extension;
        CFG.Load(cfgMain);
        if (loadAdditional) {
            var files = Directory.GetFiles(UnityEngine.Application.dataPath + "/", configFile + "*" + "." + extension);
            foreach (var file in files) {
                if (file != cfgMain) {
                    CFG.LoadAdditional(file);
                }
            }
        }
    }
}

}