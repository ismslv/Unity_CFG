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
        Console.Register(new FMLHT.Command() {
            name = "cfg_new",
            arguments = 1,
            argumentsText = "config name",
            toHideAfter = true,
            action = a => {
                NewConfig(a[1]);
            }
        });

        Console.Register(new Command()
        {
            name = "cfg_reload",
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

    public void NewConfig(string name) {
        string nameFull = UnityEngine.Application.dataPath + "/" + configFile + "_" + name + "." + extension;
        if (!File.Exists(nameFull)) {
            File.WriteAllText(nameFull, "#name " + name);
        } else {
            Console.Say("Already exists!", true);
        }
    }
}

}