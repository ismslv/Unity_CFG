# Unity_CFG
A simple class to store different settings in .ini (or any other extension) plain text format.

Version 1.2 supports the following data formats: string, int, float, bool, string/int/float arrays and lists, Vector2, Vector2Int, Vector3 and random between two (see the usage example).

During testing, you can change settings and update them in game with `LoadConfig()` command (for example, from the Console).

Store settings in .fmcfg file like this:
```
#Config.fmcfg

BoardSize = 10:10 #Size of the board: width, height
TurnsTotal = 128
HeroName = "Captain Jack"
HeroStartHealth = 0.1:0.9
```

To be able to fold regions in VSCode
(using https://marketplace.visualstudio.com/items?itemName=FMLHT.fmcfg&ssr=false#overview)
```
#> World settings
Setting1 = 1
Setting2 = 2.1
#<
```

To get these settings,
add CFGLoader component and call it:

```csharp
CFG.Load(cfgfilepath);
//or, to reload when testing
CFGLoader.LoadConfig();

string name = CFG.S("HeroName");
int turns = CFG.I("TurnsTotal");
Vector2Int size = CFG.V2I("BoardSize");
float healthHero1 = V2Rand("HeroStartHealth");
float healthHero2 = V2Rand("HeroStartHealth");
```

Also, it automatically loads if flag "loadOnAwake" is set on true.
String option "configFile" sets the base name (by default, "Config").
Then, "Config.fmcfg" will carry base configurations.
You can also add any amount of additional "Config_test1.fmcfg" files,
that will be loaded upon the base, overwriting what doubles.