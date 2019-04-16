/* CONFIG FILE READER
 * READS, PARSES AND STORES
 * DATA FROM *.FMCFG FILES
 * V1.53
 * FMLHT, 11.04.2019
 */

namespace FMLHT.Config
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class CFG
    {
        static Dictionary<string, int> cfgFiles;
        static Container container;
        static string[] boolsTrue = new string[] { "true", "yes", "ok", "da", "oui", "y" };
        static string[] boolsFalse = new string[] { "false", "no", "net", "non", "n" };
        public static string commentDivider = "#";
        public static string currentFile = "";

        public static bool isPacked;
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

        public class Container
        {
            public Dictionary<string, Item> items;
            public Container()
            {
                items = new Dictionary<string, Item>();
            }
            public void Add<T>(string name, T value)
            {
                items[name] = CreateItem<T>(value);
            }
            public void AddItem(string name, Item item)
            {
                items[name] = item;
            }
            public bool Has(string name)
            {
                return items.ContainsKey(name);
            }
            public bool Has<T>(string name)
            {
                return items[name].Type == typeof(T);
            }
            public T GetValue<T>(string name)
            {
                var item = items[name];
                return (items[name] as ItemPayload<T>).Payload;
            }
            public bool TryGet(string name, out Item item)
            {
                if (Has(name)) {
                    item = items[name];
                    return true;
                } else {
                    item = null;
                    return false;
                }
            }
            public static Item CreateItem<T>(T value)
            {
                return new ItemPayload<T>()
                {
                    Type = typeof(T),
                    Payload = value
                } as Item;
            }
            public static Item CreateArray(int length)
            {
                return new ItemArray(length) as Item;
            }
            public class Item
            {
                public System.Type Type;
                public bool IsArray = false;
                public bool Is<T>()
                {
                    return this.Type == typeof(T);
                }
                public T GetValue<T>()
                {
                    return (this as ItemPayload<T>).Payload;
                }
            }
            public class ItemPayload<T> : Item
            {
                public T Payload;
            }
            public class ItemArray : Item
            {
                public Item[] items;
                public ItemArray(int length)
                {
                    items = new Item[length];
                    Type = typeof(Container.ItemArray);
                    IsArray = true;
                }
                public Item this[int i]
                {
                    set
                    {
                        items[i] = value;
                    }
                    get
                    {
                        return items[i];
                    }
                }
            }
        }

        #region Public Getters
        public static T Get<T>(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetValueOf<T>(item);
            }
            return ZeroValueOf<T>();
        }

        public static bool B(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetBool(item);
            }
            return ZeroValueOf<bool>();
        }

        public static int I(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetInt(item);
            }
            return ZeroValueOf<int>();
        }

        public static float F(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetFloat(item);
            }
            return ZeroValueOf<float>();
        }

        public static string S(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetString(item);
            }
            return ZeroValueOf<string>();
        }

        public static Color C(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetColor(item);
            }
            return ZeroValueOf<Color>();
        }

        public static Color[] AC(string name) {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<Color>(item);
            }
            return ZeroValueOfArrayOf<Color>();
        }

        public static bool E<T>(string name, out T res) where T : struct {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetEnumOfType<T>(item, out res);
            }
            res = (T)(object)0;
            return false;
        }

        public static bool AE<T>(string name, out T[] res) where T : struct {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayEnumOfType<T>(item, out res);
            }
            res = new T[0];
            return false;
        }

        public static string[] AS(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<string>(item);
            }
            return ZeroValueOfArrayOf<string>();
        }

        public static List<string> LS(string name)
        {
            return AS(name).ToList();
        }

        public static string RS(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetRandomFromArrayOf<string>(item);
            }
            return ZeroValueOf<string>();
        }

        public static int[] AI(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<int>(item);
            }
            return ZeroValueOfArrayOf<int>();
        }

        public static List<int> LI(string name)
        {
            return AI(name).ToList();
        }

        public static int RI(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetRandomFromArrayOf<int>(item);
            }
            return ZeroValueOf<int>();
        }

        public static float[] AF(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<float>(item);
            }
            return ZeroValueOfArrayOf<float>();
        }

        public static List<float> LF(string name)
        {
            return LF(name).ToList();
        }

        public static float RF(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetRandomFromArrayOf<float>(item);
            }
            return ZeroValueOf<float>();
        }

        public static Vector2 V2(string name)
        {
            var v_ = AF(name);
            var vRes = new Vector2(0f, 0f);
            if (v_.Length > 0) vRes.x = v_[0];
            if (v_.Length > 1) vRes.y = v_[1];
            return vRes;
        }

        public static Vector2Int V2I(string name)
        {
            var v_ = AI(name);
            var vRes = new Vector2Int(0, 0);
            if (v_.Length > 0) vRes.x = v_[0];
            if (v_.Length > 1) vRes.y = v_[1];
            return vRes;
        }

        public static float V2Rand(string name)
        {
            var v_ = V2(name);
            return Random.Range(v_[0], v_[1]);
        }

        public static int V2RandI(string name)
        {
            var v_ = V2I(name);
            return Random.Range(v_[0], v_[1]);
        }

        public static Vector3 V3(string name)
        {
            var v_ = AF(name);
            var vRes = new Vector3(0f, 0f, 0f);
            if (v_.Length > 0) vRes.x = v_[0];
            if (v_.Length > 1) vRes.y = v_[1];
            if (v_.Length > 2) vRes.z = v_[2];
            return vRes;
        }

        public static Vector3 V3Int(string name)
        {
            var v_ = AI(name);
            var vRes = new Vector3Int(0, 0, 0);
            if (v_.Length > 0) vRes.x = v_[0];
            if (v_.Length > 1) vRes.y = v_[1];
            if (v_.Length > 2) vRes.z = v_[2];
            return vRes;
        }

        public static bool[] AB(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<bool>(item);
            }
            return ZeroValueOfArrayOf<bool>();
        }

        public static List<bool> LB(string name)
        {
            return AB(name).ToList();
        }

        public static KeyCode K(string name)
        {
            return GetKeyCode(container.items[name]);
        }

        public static KeyCode[] AK(string name)
        {
            Container.Item item;
            if (container.TryGet(name, out item))
            {
                return GetArrayOf<KeyCode>(item);
            }
            return ZeroValueOfArrayOf<KeyCode>();
        }

        public static List<KeyCode> LK(string name)
        {
            return AK(name).ToList();
        }
        #endregion

        #region Inner Getters
        static T[] GetArrayOf<T>(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new T[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetValueOf<T>(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return new T[1] {GetValueOf<T>(item)};
        }

        static T GetValueOf<T>(Container.Item item)
        {
            if (typeof(T) == typeof(int))
            {
                return (T)System.Convert.ChangeType(GetInt(item), typeof(T));
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)System.Convert.ChangeType(GetFloat(item), typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)System.Convert.ChangeType(GetString(item), typeof(T));
            }
            else if (typeof(T) == typeof(Color))
            {
                return (T)System.Convert.ChangeType(GetColor(item), typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)System.Convert.ChangeType(GetBool(item), typeof(T));
            }
            else if (typeof(T) == typeof(KeyCode))
            {
                return (T)System.Convert.ChangeType(GetKeyCode(item), typeof(T));
            }
            else if (typeof(T) == typeof(int[]))
            {
                return (T)System.Convert.ChangeType(GetArrayInt(item), typeof(T));
            }
            else if (typeof(T) == typeof(float[]))
            {
                return (T)System.Convert.ChangeType(GetArrayFloat(item), typeof(T));
            }
            else if (typeof(T) == typeof(string[]))
            {
                return (T)System.Convert.ChangeType(GetArrayString(item), typeof(T));
            }
            else if (typeof(T) == typeof(bool[]))
            {
                return (T)System.Convert.ChangeType(GetArrayBool(item), typeof(T));
            }
            else if (typeof(T) == typeof(KeyCode[]))
            {
                return (T)System.Convert.ChangeType(GetArrayKeyCode(item), typeof(T));
            }
            return (T)System.Convert.ChangeType(GetString(item), typeof(T));
        }

        static T ZeroValueOf<T>()
        {
            if (typeof(T) == typeof(int))
            {
                return (T)System.Convert.ChangeType(0, typeof(T));
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)System.Convert.ChangeType(0f, typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)System.Convert.ChangeType("", typeof(T));
            }
            else if (typeof(T) == typeof(Color))
            {
                return (T)System.Convert.ChangeType(Color.black, typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)System.Convert.ChangeType(false, typeof(T));
            }
            else if (typeof(T) == typeof(KeyCode))
            {
                return (T)System.Convert.ChangeType(KeyCode.None, typeof(T));
            }
            return (T)System.Convert.ChangeType(false, typeof(T));
        }

        static T[] ZeroValueOfArrayOf<T>() {
            return new T[0];
        }

        static T GetRandomFromArrayOf<T>(Container.Item item)
        {
            var array = GetArrayOf<T>(item);
            return array[Random.Range(0, array.Length)];
        }

        static bool GetBool(Container.Item item)
        {
            if (item.Is<bool>())
            {
                return item.GetValue<bool>();
            }
            if (item.Is<string>())
            {
                bool _bool;
                if (TryParseBool(item.GetValue<string>(), out _bool))
                {
                    return _bool;
                }
            }
            if (item.Is<int>())
            {
                return TryParseBool(item.GetValue<int>());
            }
            if (item.Is<float>())
            {
                return TryParseBool(item.GetValue<float>());
            }
            return ZeroValueOf<bool>();
        }

        static int GetInt(Container.Item item)
        {
            if (item.Is<int>())
            {
                return item.GetValue<int>();
            }
            if (item.Is<float>())
            {
                return Mathf.RoundToInt(item.GetValue<float>());
            }
            if (item.Is<string>())
            {
                int _int;
                if (int.TryParse(item.GetValue<string>(), out _int))
                {
                    return _int;
                }
            }
            return ZeroValueOf<int>();
        }

        static float GetFloat(Container.Item item)
        {
            if (item.Is<float>())
            {
                return item.GetValue<float>();
            }
            if (item.Is<int>())
            {
                return (float)item.GetValue<int>();
            }
            if (item.Is<string>())
            {
                float _float;
                if (float.TryParse(item.GetValue<string>(), out _float))
                {
                    return _float;
                }
            }
            return ZeroValueOf<float>();
        }

        static string GetString(Container.Item item)
        {
            if (item.Is<string>())
            {
                return item.GetValue<string>();
            }
            if (item.Is<int>())
            {
                return item.GetValue<int>().ToString();
            }
            if (item.Is<float>())
            {
                return item.GetValue<float>().ToString();
            }
            if (item.Is<bool>())
            {
                return item.GetValue<bool>().ToString();
            }
            if (item.Is<KeyCode>())
            {
                return item.GetValue<KeyCode>().ToString();
            }
            if (item.IsArray) {
                var s = new List<string>();
                foreach (var i in (item as Container.ItemArray).items) {
                    s.Add(GetString(i));
                }
                return System.String.Join(",", s);
            }
            return ZeroValueOf<string>();
        }

        static Color GetColor(Container.Item item)
        {
            Color color = ZeroValueOf<Color>();
            if (item.Is<string>())
            {
                string c_ = "#" + item.GetValue<string>();
                if (ColorUtility.TryParseHtmlString(c_, out color)) {
                    return color;
                }
            }
            return color;
        }

        static KeyCode GetKeyCode(Container.Item item)
        {
            if (item.Is<KeyCode>())
            {
                return item.GetValue<KeyCode>();
            }
            if (item.Is<string>())
            {
                KeyCode _keyCode;
                if (System.Enum.TryParse(item.GetValue<string>(), out _keyCode)) {
                    return _keyCode;
                }
            }
            return ZeroValueOf<KeyCode>();
        }

        static bool GetEnumOfType<T>(Container.Item item, out T res) where T : struct
        {
            if (item.Is<string>())
            {
                if (System.Enum.TryParse<T>(item.GetValue<string>(), out res)) {
                    return true;
                }
            }
            res = (T)(object)0;
            return false;
        }

        static bool GetArrayEnumOfType<T>(Container.Item item, out T[] array) where T : struct
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new List<T>();
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    T res;
                    if (GetEnumOfType<T>(arraySrc.items[i], out res)) {
                        arrayRes.Add(res);
                    }
                }
                array = arrayRes.ToArray();
                return true;
            } else {
                T res;
                if (GetEnumOfType<T>(item, out res)) {
                    array = new T[1]{res};
                    return true;
                }
            }
            array = new T[0];
            return false;
        }

        static string[] GetArrayString(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new string[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetString(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return ZeroValueOf<string[]>();
        }

        static int[] GetArrayInt(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new int[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetInt(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return ZeroValueOf<int[]>();
        }

        static float[] GetArrayFloat(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new float[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetFloat(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return ZeroValueOf<float[]>();
        }

        static bool[] GetArrayBool(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new bool[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetBool(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return ZeroValueOf<bool[]>();
        }

        static KeyCode[] GetArrayKeyCode(Container.Item item)
        {
            if (item.IsArray)
            {
                var arraySrc = (item as Container.ItemArray);
                var arrayRes = new KeyCode[arraySrc.items.Length];
                for (int i = 0; i < arraySrc.items.Length; i++)
                {
                    arrayRes[i] = GetKeyCode(arraySrc.items[i]);
                }
                return arrayRes;
            }
            return ZeroValueOf<KeyCode[]>();
        }
        #endregion

        #region Parsers
        public static bool Has(string name)
        {
            return container.Has(name);
        }

        public static void Clear()
        {
            cfgFiles = new Dictionary<string, int>();
            container = new Container();
        }

        public static bool ParseVariables(string val)
        {
            var val_ = val.Split(" "[0]);
            switch (val_[0])
            {
                case "name":
                    if (val_.Length > 1) {
                        cfgFiles[val_[1]] = 0;
                        currentFile = val_[1];
                    }
                    break;
                case "stop":
                    return false;
            }
            return true;
        }

        public static Container.Item ParseSimple(string val)
        {
            val = val.Trim();
            if (val[0] == '\"' || val[0] == '\'') {
                //is string
                val = val.Remove(0, 1);
                if (val[val.Length - 1] == '\"' || val[val.Length - 1] == '\'')
                    val = val.Remove(val.Length - 1, 1);
                return Container.CreateItem<string>(val);
            }
            int _int;
            if (int.TryParse(val, out _int))
            {
                //is int
                return Container.CreateItem<int>(_int);
            }
            float _float;
            if (float.TryParse(val, out _float))
            {
                //is float
                return Container.CreateItem<float>(_float);
            }
            bool _bool;
            if (TryParseBool(val, out _bool))
            {
                //is bool
                return Container.CreateItem<bool>(_bool);
            }
            KeyCode _keyCode;
            if (System.Enum.TryParse(val, out _keyCode))
            {
                //is KeyCode
                return Container.CreateItem<KeyCode>(_keyCode);
            }
            //is string
            return Container.CreateItem<string>(val);
        }

        public static Container.ItemArray ParseArray(string[] val)
        {
            var array = Container.CreateArray(val.Length) as Container.ItemArray;
            for (int i = 0; i < val.Length; i++)
            {
                var item = ParseSimple(val[i]);
                array[i] = item;
            }
            return array;
        }

        public static Container.Item FlattenArray(Container.ItemArray array)
        {
            var type = array.items[0].Type;
            if (type == typeof(int))
            {
                return FlattenArrayIterator<int>(array);
            }
            else if (type == typeof(float))
            {
                return FlattenArrayIterator<float>(array);
            }
            else if (type == typeof(bool))
            {
                return FlattenArrayIterator<bool>(array);
            }
            else if (type == typeof(KeyCode))
            {
                return FlattenArrayIterator<KeyCode>(array);
            }
            else
            {
                return FlattenArrayIterator<string>(array);
            }
        }

        public static Container.Item FlattenArrayIterator<T>(Container.ItemArray array)
        {
            var array_ = new List<T>();
            foreach (var item in array.items)
            {
                if (item.Type == typeof(T))
                    array_.Add((item as Container.ItemPayload<T>).Payload);
            }
            return Container.CreateItem<T[]>(array_.ToArray());
        }

        public static bool TryParseBool(string val, out bool boolRes)
        {
            if (System.Array.IndexOf(boolsTrue, val.ToLower()) > -1)
            {
                boolRes = true;
                return true;
            }
            else if (System.Array.IndexOf(boolsFalse, val.ToLower()) > -1)
            {
                boolRes = false;
                return true;
            }
            boolRes = false;
            return false;
        }

        public static bool TryParseBool(int val)
        {
            return val > 0;
        }

        public static bool TryParseBool(float val)
        {
            return val > 0f;
        }

        public static void ParseVal(string name, string val)
        {
            if (CheckIfString(val))
            {
                //it may only be a string, don't check for other symbols
                val = val.Replace("\"", "").Replace("'", "");
                container.Add<string>(name, val);
                return;
            }
            if (val.Contains(",") || val.Contains(":"))
            {
                //it is array or range
                string[] valA = new string[0];
                if (val.Contains(","))
                {
                    valA = val.Split(","[0]);
                }
                else if (val.Contains(":"))
                {
                    valA = val.Split(":"[0]);
                }
                if (valA.Length == 1)
                {
                    //a single value, not an array
                    container.AddItem(name, ParseSimple(valA[0]));
                }
                else
                {
                    //an array
                    var array = ParseArray(valA);
                    //container.Add(name, FlattenArray(array));
                    container.AddItem(name, array);
                }
                return;
            }
            //is single value
            container.AddItem(name, ParseSimple(val));
        }

        public static bool CheckIfString(string val) {
            bool isString = false;
            int q = 0;
            foreach (var c in val) {
                if (c == '\"' || c == '\'') {
                    q++;
                    isString = !isString;
                }
            }
            return q > 0 && ((!isString && q <= 2) || (isString && q > 2));
        }

        public static string AllFilesNames()
        {
            var res = new List<string>();
            foreach (var c in cfgFiles) {
                res.Add(c.Key + ":" + c.Value);
            }
            return string.Join(", ", res);
        }
        #endregion

        #region Loaders
        public static void LoadFromFile(string file, bool clear = true) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            if (clear)
                Clear();
            currentFile = "";
            if (File.Exists(file)) {
                FileReader.Read(file, (line) => {
                    return Load(line);
                });
            }
        }

        public static void LoadFromAsset(string name, bool clear = true) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            if (clear)
                Clear();
            currentFile = "";
            var asset = (TextAsset)Resources.Load<TextAsset>("Config/" + name);
            if (asset != null) {
                isPacked = true;
                var lines = Regex.Split(asset.text, LINE_SPLIT_RE);
                foreach (var l in lines) {
                    var success = Load(l);
                    if (!success) break;
                }
            }
        }

        public static bool Load(string line)
        {
            string key;
            string val;
            string[] lineSrc = line.Split('=');
            if (lineSrc.Length > 1)
            {
                //if variable line
                key = lineSrc[0].Trim();
                val = lineSrc[1].Trim();
                string[] val_ = val.Split(new string[] { commentDivider }, System.StringSplitOptions.None);
                val = val_[0];

                ParseVal(key, val);
                if (currentFile != "") cfgFiles[currentFile]++;
            }
            else if (lineSrc.Length == 1)
            {
                //if comment line
                key = lineSrc[0].Trim();
                string[] key_ = key.Split(new string[] { commentDivider }, System.StringSplitOptions.None);
                if (key_.Length == 2)
                {
                    return ParseVariables(key_[1]);
                }
            }
            return true;
        }

        public static void Write(string path, Dictionary<string, string> data)
        {
            List<string> list = new List<string>();
            foreach (var line in data)
            {
                list.Add(line.Key + " = " + line.Value);
            }
            FileReader.Write(path, list);
        }
        #endregion
    }
}