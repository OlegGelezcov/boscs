using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StreamingSerialization
{
    public static string Load(string fpath)
    {
        if (!File.Exists(fpath))
        {
            if (File.Exists("data.bak"))
                return File.ReadAllText("data.bak");

            return null;
        }

        return File.ReadAllText(fpath);
    }

    public static void Save(string fpath, object obj)
    {
        var finalPath = Path.Combine(Application.persistentDataPath, fpath);
        var tempFile = Path.Combine(Application.persistentDataPath, "temp.json");
        var backFile = Path.Combine(Application.persistentDataPath, "data.bak");
        //Application.persistentDataPath;
        var str = JsonConvert.SerializeObject(obj);
        if (File.Exists(tempFile))
            File.Delete(tempFile);

        File.WriteAllText(tempFile, str);
        //File.Replace(tempFile, finalPath, backFile);
        File.Copy(tempFile, finalPath, true);
    }
}
