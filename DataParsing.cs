using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class Row
{
    public OrderedDictionary data;
    public int Length { get { return data.Count; } }
    public int index = -1;
    public List<Row> table;
    public IList<string> Keys { get { return data.Keys.Cast<string>().ToList(); } }

    public string this[string i]
    {
        get { return data[i] as string; }
    }

    public string this[int i]
    {
        get { return data[i] as string; }
    }

    public Row(OrderedDictionary fill)
    {
        data = fill;
    }

    public bool has(string key)
    {
        return data.Contains(key) && data[key] as string != "";
    }

    public string AtOrDefault(string key, string def = "")
    {
        return has(key) ? this[key] : def;
    }

    public T EnumOrDefault<T>(string key, T def = default(T))
    {
        if (has(key))
            return (T)Enum.Parse(typeof(T), this[key]);
        return def;
    }

    public int Count { get { return data.Count; } }

    public string GetLast(string Key)
    {
        if (has(Key))
            return this[Key];
        else
        {
            var currentRow = this;
            while (currentRow.prev != null)
            {
                currentRow = currentRow.prev;

                if (currentRow.has(Key))
                    return currentRow[Key];
            }

            throw new FormatException(Key + " not found; at[" + index + "]");
        }
    }

    public Row next = null;
    public Row prev = null;
}

public class CsvCell
{
    private int x = 0, y = 0;
    List<Row> table;

    public CsvCell(int _x, int _y, List<Row> _table)
    {
        x = _x;
        y = _y;
        table = _table;
    }

    public bool IsAttribute()
    {
        return table[x].Keys[y].StartsWith("Attr", StringComparison.InvariantCultureIgnoreCase);
    }

    public CsvCell GetOffset(int _x, int _y)
    {
        return new CsvCell(x + _x, y + _y, table);
    }

    public bool hasValue()
    {
        return table != null && x < table.Count && y < table[x].Count && table[x][y] != "";
    }

    public string GetValue()
    {
        try
        {
            if (x < table.Count && y < table[x].Count)
            {
                return table[x][y];
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return "";
    }
}


public class CsvReader : IEnumerable<Row>
{
    List<string> columns;
    public List<Row> rows = new List<Row>();

    public IEnumerator<Row> GetEnumerator()
    {
        return rows.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public int rowCount
    {
        get { return rows.Count; }
    }

    public int colCount
    {
        get { return columns.Count; }
    }

    public Row atRow(int rowIndex)
    {
        return rows[rowIndex];
    }

    public Row atRow(string key, string value)
    {
        return rows.FirstOrDefault(val => { return val.has(key) && val[key] == value; });
    }

    public Row AtRealRow(int rowIndex)
    {
        return atRow(rowIndex - 2);
    }

    public List<string> GetColumns()
    {
        return columns;
    }


    public List<string> Column(string name)
    {
        return rows.Select(row => row[name]).ToList();
    }


    public CsvReader(string fileName)
    {
        using (StreamReader r = new StreamReader(fileName))
        {
            string line;
            line = r.ReadLine();

            rows.Clear();

            if (line != null)
            {
                columns = line.Split(',').Select(v => v.Trim()).ToList();

                Debug.Log(fileName + " Header: " + line);
                int rowIndex = 0;
                while ((line = r.ReadLine()) != null)
                {
                    var rawCells = new List<string>();

                    try
                    {
                        bool good = true;
                        string accumulatedCell = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (line[i] == ',' && good)
                            {
                                rawCells.Add(accumulatedCell.Trim());
                                accumulatedCell = "";
                            }
                            else if (line.Length > (i + 1) && line[i] == '"' && line[i + 1] == '"')
                            {
                                accumulatedCell += '"';
                                i++;
                            }
                            else if (line[i] == '"')
                            {
                                good = !good;
                            }
                            else
                                accumulatedCell += line[i];
                        }

                        rawCells.Add(accumulatedCell);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + e.StackTrace + ", at row " + rowIndex);
                    }

                    OrderedDictionary cells = new OrderedDictionary();

                    try
                    {
                        int indexCounter = 0;
                        foreach (string cell in rawCells)
                        {
                            if (indexCounter < columns.Count)
                                cells[columns[indexCounter]] = cell;
                            indexCounter++;
                        }

                        rows.Add(new Row(cells));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + e.StackTrace + ", at row " + rowIndex);
                    }

                    rowIndex++;
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    if (i > 0)
                    {
                        rows[i].prev = rows[i - 1];
                        rows[i - 1].next = rows[i];
                    }

                    rows[i].index = i;
                    rows[i].table = rows;
                }
            }
        }
    }
}


public class DownloadData
{
    public string fileName;
    public string urlID;

    public DownloadData(string fileName, string urlId)
    {
        this.fileName = fileName;
        urlID = urlId;
    }
}


public class MyWindow : EditorWindow
{
    static string baseDataUrl ="https://docs.google.com/spreadsheets/d/108L9fuesd_zodj2zPIuSU97W8dQYrjZYFFjRQ4yukG0/pub?gid=";
    static string urlEnding = "&single=true&output=csv";
    private static string finalPath = "Assets/Resources/Data/";
    private static string cvsPath = "Assets/Raw/";
    private static string cvsEnd = ".csv";

    public static List<DownloadData> Datas = new List<DownloadData>()
    {
        new DownloadData("personal_purchases", "55091556"),

    };

    [MenuItem("ImportExcel/DownLoadAndParse")]
    public static void ShowWindow2()
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        foreach (var data in Datas)
        {
            var path = cvsPath + data.fileName + cvsEnd;
            Download(baseDataUrl + data.urlID + urlEnding, path).Join();
            SaveToJson(new CsvReader(path), data.fileName);
        }
        Debug.Log("Downloaded");
    }
    

    private static Thread Download(string address, string path)
    {
        var thread = new Thread(() =>
        {
            var client = new WebClient();
            client.DownloadFile(address, path);
        });
        thread.Start();
        return thread;
    }

    
    public static void SaveToJson(CsvReader csv, string fileName)
    {
        var result = new StringBuilder();

        result.Append("[");
        
        for (var rowIndex = 0; rowIndex < csv.rows.Count; rowIndex++)
        {
            var row = csv.rows[rowIndex];
            
            if (rowIndex != 0 && rowIndex != csv.rows.Count)
                result.Append(",");  
                
            result.Append("{");
            for (var index = 0; index < row.Keys.Count; index++)
            {
                var rowKey = row.Keys[index];
                result.Append("\"");
                result.Append(rowKey);
                result.Append("\"");
                result.Append(":");
                result.Append("\"");
                result.Append(row[rowKey]);
                result.Append("\"");
                if (index < row.Keys.Count - 1)
                    result.Append(",");
            }

            result.Append("}");
        }
        result.Append("]");



        //File.WriteAllText(finalPath + fileName + ".json", result.ToString());

        string fullPath = Path.Combine(Application.dataPath, "Resources/Data");
        fullPath = Path.Combine(fullPath, fileName + ".json");

        Dictionary<string, Action<object>> fixActions = new Dictionary<string, Action<object>> {
            ["personal_purchases"] = FixPersonalPurchases
        };

        SerailizePretty(fullPath, result.ToString(), fixActions.ContainsKey(fileName) ? fixActions[fileName] : (o) => { });
        AssetDatabase.Refresh();
    }


    private static void SerailizePretty(string path, string value, Action<object> fixAction) {
        object jsonObj = JsonConvert.DeserializeObject(value);
        fixAction?.Invoke(jsonObj);

        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        using (StreamWriter streamWriter = new StreamWriter(path)) {
            using (JsonWriter writer = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(writer, jsonObj);
            }
        }

    }


    private static void FixPersonalPurchases(object jsonObj) {
        if (jsonObj is JArray) {
            foreach (JObject obj in (jsonObj as JArray)) {
                obj.Remove("");
                obj.Remove("Name");
                string price = obj["price"].ToString();
                if (!string.IsNullOrEmpty(price)) {
                    price = price.Replace(",", ".");
                    obj["price"] = price;
                }
            }
        }
    }
}




