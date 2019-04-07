# Unity_CFG
A simple class to store different settings in .fmcfg (or any other extension) plain text format.

## Has a [VSCode extension](https://marketplace.visualstudio.com/items?itemName=FMLHT.fmcfg)

Version 1.5 supports the following data formats:

string, int, float, bool, KeyCode, string/int/float/bool/KeyCode arrays and lists, Vector2, Vector2Int, Vector3, Vector3Int and randoms between two (see the usage example).

New in this version: generic Enums.



## Usage

Store settings in .fmcfg file like this:

```ini
#Config.fmcfg

BoardSize = 10:10 #Size of the board: width, height
TurnsTotal = 128
HeroName = "Captain Jack"
HeroStartHealth = 0.1:0.9
ExitKey = Esc
```

To be able to fold regions in VSCode
```ini
#> World settings
Setting1 = 1
Setting2List = 2,1,3
#<
```

## Spells
```ini
#name Hardcore
```

Defines a config name to show in the debugger

```ini
#stop
```

Stops config reading at this line.

If placed on top, disables all file and does not register it's name.

```ini
key, mouse, joystick, axisbutton
```

Autocompletes the lists of input values.

Keys according to [Unity KeyCodes](https://docs.unity3d.com/ScriptReference/KeyCode.html)

Mouse: Left, Right, Middle

Joystick and axisbutton add support for FMLHT input module (link will be here upon a release)

## Adding to a project
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
You can also add any amount of additional "Config_test1.fmcfg" files, that will be loaded upon the base, overwriting what doubles.

## Generic enums

To parse generic enums:

```csharp
CharacterState state = CFG.E<CharacterState>("Hero_StateStart");
CharacterState[] states = CFG.AE<CharacterState>("Monster_StatesOrder");
```