/* FILE READER
 * V1.0
 * FMLHT, 15.04.2018
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace FMLHT.Config {

public static class FileReader
{
    public static void Read(string file, System.Func<string, bool> action)
    {
        StreamReader reader = new StreamReader(file);
        string line;
        bool further = true;
        while ((line = reader.ReadLine()) != null && further)
        {
            further = action(line);
        }
        reader.Close();
    }

    public static void Write(string file, List<string> data)
    {
        StreamWriter writer = new StreamWriter(file, true);
        foreach (string line in data)
            writer.WriteLine(line);
        writer.Close();
    }
}

}