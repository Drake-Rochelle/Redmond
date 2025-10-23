using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO

public class redmondManager : MonoBehaviour
{
    [Serializable] public class KeyPair
    {
        [SerializeField] public string input;
        [SerializeField] public string output;
    }
    [SerializeField] private List<string> graphicsObjects;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private List<KeyPair> keys;
    private Mesh mesh;
    private string stackIn;
    string syntax_string;
    private dict numbers;
    private dict strings;
    private int_dict functions;
    private int_dict addresses;
    private image_dict images;
    private Dictionary<string, object> comment;
    private Dictionary<string, object> tooltip_descriptions;
    private Dictionary<string, object> tooltip_arguments;
    private int pc;
    private List<List<object>> stack;
    private bool buffer;
    private List<List<object>> drawStack;
    private bool draw;
    private bool useBuffer;
    private float depth = 0.1f;
    private float start;
    private List<Material> mats;
    private MiniInterpreter interpreter;
    class image
    {
        public string texture;
        public float width;
        public float height;
        public float uv_x;
        public float uv_y;
        public float uv_w;
        public float uv_h;
    }
    private class image_dict
    {
        public List<string> keys;
        public List<image> values;
        public image_dict()
        {
            keys = new List<string>();
            values = new List<image>();
        }
        public void DeleteKey(string key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
            {
                RaiseError("Key \"" + key.ToString() + "\" does not exist.");
            }
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        public image this[string key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    RaiseError("Key \"" + key.ToString() + "\" does not exist.");
                }
                image raw = values[index];
                return raw;
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index >= 0)
                {
                    values[index] = value;
                }
                else
                {
                    keys.Add(key);
                    values.Add(value);
                }
            }
        }
    }
    private class int_dict
    {
        public List<string> keys;
        public List<int> values;
        public int_dict()
        {
            keys = new List<string>();
            values = new List<int>();
        }
        public void DeleteKey(string key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
            {
                RaiseError("Key \"" + key.ToString() + "\" does not exist.");
            }
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        public int this[string key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    RaiseError("Key \"" + key.ToString() + "\" does not exist.");
                }
                int raw = values[index];
                return raw;
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index >= 0)
                {
                    values[index] = value;
                }
                else
                {
                    keys.Add(key);
                    values.Add(value);
                }
            }
        }
    }
    private class dict
    {
        public List<string> keys;
        public List<object> values;
        public List<string> types;
        public dict()
        {
            keys = new List<string>();
            values = new List<object>();
            types = new List<string>();
        }
        public void DeleteKey(string key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
            {
                RaiseError("Key \""+key.ToString()+"\" does not exist.");
            }
            keys.RemoveAt(index);
            values.RemoveAt(index);
            types.RemoveAt(index);
        }

        public string TypeOf(string key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
            {
                RaiseError("Key \"" + key.ToString() + "\" does not exist.");
            }
            string data_type = types[index];
            return data_type;
        }
        public object this[string key]
        {
            get
            {
                key = key.Trim();
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    RaiseError("Key \"" + key.ToString() + "\" does not exist.");
                }
                object raw = values[index];
                return raw;
            }
            set
            {
                key = key.Trim();
                int index = keys.IndexOf(key);
                string type = "";
                type = value.GetType().ToString();
                if (value.GetType() == typeof(int))
                {
                    value = (float)value;
                }
                if (type == "")
                {
                    RaiseError("Key \"" + key.ToString() + "\" does not have a supported type.");
                }
                if (index >= 0)
                {
                    values[index] = value;
                    types[index] = type;
                }
                else
                {
                    keys.Add(key);
                    values.Add(value);
                    types.Add(type);
                }
            }
        }
    }
    static public void Print(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#else
        Debug.Log(message);
        Console.WriteLine(message);
#endif
    }
    static private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    static private void RaiseError(string message)
    {
        Debug.LogError(message);
        Quit();
    }
    private int Count(string input, string substring)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(substring))
        {
            return 0;
        }
        int count = 0;
        int index = 0;
        while ((index = input.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
    private object parse_number(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        if (input.Contains("."))
        {
            try
            {
                return float.Parse(input);
            }
            catch { }
        }
        try
        {
            return (float)int.Parse(input);
        }
        catch { }
        return null;
    }
    private void DrawStack()
    {
        depth = Camera.main.nearClipPlane+0.001f;
        mesh = new Mesh();
        mesh.subMeshCount = mats.Count+1;
        List<Vector3> verts = new List<Vector3>();
        List<List<int>> trigs = new List<List<int>>();
        for (int i = 0; i < mats.Count+1; i++)
        {
            trigs.Add(new List<int>());
        }
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < drawStack.Count; i++)
        {
            depth += 0.002f;
            int curr = verts.Count;
            switch (graphicsObjects[(int)drawStack[i][0]])
            {
                case "pixel":
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1] + 1, (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2] + 1));
                    verts.Add(GetPos((float)drawStack[i][1] + 1, (float)drawStack[i][2] + 1));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    trigs[0].Add(curr);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 3);
                    for (int j = 0; j < 4; j++)
                    {
                        colors.Add(new Color((float)drawStack[i][3] / 255, (float)drawStack[i][4] / 255, (float)drawStack[i][5] / 255, 1));
                    }
                    break;
                case "rect":
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1] + (float)drawStack[i][3], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2] + (float)drawStack[i][4]));
                    verts.Add(GetPos((float)drawStack[i][1] + (float)drawStack[i][3], (float)drawStack[i][2] + (float)drawStack[i][4]));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    trigs[0].Add(curr);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 3);
                    for (int j = 0; j < 4; j++)
                    {
                        colors.Add(new Color((float)drawStack[i][5] / 255, (float)drawStack[i][6] / 255, (float)drawStack[i][7] / 255, 1));
                    }
                    break;
                case "gradient":
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1] + (float)drawStack[i][3], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2] + (float)drawStack[i][4]));
                    verts.Add(GetPos((float)drawStack[i][1] + (float)drawStack[i][3], (float)drawStack[i][2] + (float)drawStack[i][4]));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    trigs[0].Add(curr);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 3);
                    Color color1 = new Color((float)drawStack[i][5] / 255, (float)drawStack[i][6] / 255, (float)drawStack[i][7] / 255, 1);
                    Color color2 = new Color((float)drawStack[i][8] / 255, (float)drawStack[i][9] / 255, (float)drawStack[i][10] / 255, 1);
                    if ((int)(float)drawStack[i][11] == 0)
                    {
                        colors.Add(color1);
                        colors.Add(color2);
                        colors.Add(color1);
                        colors.Add(color2);
                    }
                    else
                    {
                        colors.Add(color1);
                        colors.Add(color1);
                        colors.Add(color2);
                        colors.Add(color2);
                    }
                    break;
                case "trig":
                    verts.Add(GetPos((float)drawStack[i][1], (float)drawStack[i][2]));
                    verts.Add(GetPos((float)drawStack[i][3], (float)drawStack[i][4]));
                    verts.Add(GetPos((float)drawStack[i][5], (float)drawStack[i][6]));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 0));
                    trigs[0].Add(curr);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr + 2);
                    trigs[0].Add(curr + 1);
                    trigs[0].Add(curr);
                    trigs[0].Add(curr + 2);
                    color1 = new Color((float)drawStack[i][7] / 255, (float)drawStack[i][8] / 255, (float)drawStack[i][9] / 255, 1);
                    for (int j = 0; j < 3; j++)
                    {
                        colors.Add(color1);
                    }
                    break;
                case "image":
                    string tex = (string)drawStack[i][1];
                    float width = (float)drawStack[i][2];
                    float height = (float)drawStack[i][3];
                    float uvx = (float)drawStack[i][4];
                    float uvy = (float)drawStack[i][5];
                    float uvw = (float)drawStack[i][6];
                    float uvh = (float)drawStack[i][7];
                    float x = (float)drawStack[i][8];
                    float y = (float)drawStack[i][9];
                    float r = (float)drawStack[i][10];
                    Material mat = null;
                    int matIndex = 0;
                    for (int j = 0; j < mats.Count; j++)
                    {
                        if (mats[j].name == tex)
                        {
                            mat = mats[j];
                            matIndex = j+1;
                        }
                    }
                    if (mat != null)
                    {
                        List<Vector2> vertsIn = new List<Vector2>();
                        vertsIn.Add(new Vector2(-width / 2, -height / 2));
                        vertsIn.Add(new Vector2(width / 2, -height / 2));
                        vertsIn.Add(new Vector2(-width / 2, height / 2));
                        vertsIn.Add(new Vector2(width / 2, height / 2));
                        for (int j = 0; j < 4; j++)
                        {
                            vertsIn[j] = Rotate(vertsIn[j], r);
                            vertsIn[j] = new Vector2(vertsIn[j].x + x, vertsIn[j].y + y);
                            verts.Add(GetPos(vertsIn[j].x, vertsIn[j].y));
                        }
                        uvs.Add(new Vector2(uvx, uvy));
                        uvs.Add(new Vector2(uvx+uvw, uvy));
                        uvs.Add(new Vector2(uvx, uvy+uvh));
                        uvs.Add(new Vector2(uvx+uvw, uvy+uvh));
                        colors.Add(Color.white);
                        colors.Add(Color.white);
                        colors.Add(Color.white);
                        colors.Add(Color.white);
                        trigs[matIndex].Add(curr);
                        trigs[matIndex].Add(curr + 1);
                        trigs[matIndex].Add(curr + 2);
                        trigs[matIndex].Add(curr + 2);
                        trigs[matIndex].Add(curr + 1);
                        trigs[matIndex].Add(curr + 3);
                    }
                    else
                    {
                        RaiseError("Image not initialized.");
                    }
                    break;
            }
        }
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        for (int i = 0; i < trigs.Count; i++)
        {
            mesh.SetTriangles(trigs[i], i);
        }
        mesh.colors = colors.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    private Vector3 GetPos(float x, float y)
    {
        Camera cam = Camera.main;
        Vector2 screenPos = new Vector2(x, y);
        float ndcX = (screenPos.x / cam.pixelWidth) * 2f - 1f;
        float ndcY = 1f - (screenPos.y / cam.pixelHeight) * 2f;
        float halfHeight = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * depth;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 viewSpacePos = new Vector3(ndcX * halfWidth, ndcY * halfHeight, depth);
        return cam.transform.TransformPoint(viewSpacePos);
    }
    private string GetPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }
        return Path.Combine(Application.persistentDataPath, path.TrimStart('\\', '/'));
    }
    private string[] Split(string input, string delimiter)
    {
        var result = new List<string>();
        int start = 0;
        int index;
        while ((index = input.IndexOf(delimiter, start)) != -1)
        {
            result.Add(input.Substring(start, index - start));
            start = index + delimiter.Length;
        }
        result.Add(input.Substring(start));
        return result.ToArray();
    }
    private object ToFloat(object a)
    {
        if (a.GetType() == typeof(float))
        {
            return (float)a;
        }
        else if (a.GetType() == typeof(int))
        {
            return (float)(int)a;
        }
        else if (a.GetType() == typeof(long))
        {
            return (float)(long)a;
        }
        else if (typeof(IList).IsAssignableFrom(a.GetType()))
        {
            IList list = (IList)a;
            var result = new List<object>();
            foreach (var item in list)
            {
                result.Add(ToFloat(item));
            }

            if (AllAreType(result, typeof(float)))
            {
                return result.ConvertAll(x => (float)x);
            }
            else if (AllAreType(result, typeof(List<float>)))
            {
                return result.ConvertAll(x => (List<float>)x);
            }

            return result;
        }
        else
        {
            throw new InvalidCastException("Unsupported type: " + a.GetType());
        }
    }
    private bool AllAreType(List<object> list, Type type)
    {
        foreach (var item in list)
        {
            if (item == null || item.GetType() != type)
                return false;
        }
        return true;
    }
    private string Key(string input)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].input == input)
            {
                return keys[i].output;
            }
        }
        return input;
    }
    private void OverwriteTextureWithBmp(Texture2D target, byte[] bmpBytes)
    {
        using (BinaryReader reader = new BinaryReader(new MemoryStream(bmpBytes)))
        {
            reader.ReadBytes(10);
            int dataOffset = reader.ReadInt32();
            int headerSize = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            reader.ReadInt16();
            int bitsPerPixel = reader.ReadInt16();
            if (bitsPerPixel != 24 && bitsPerPixel != 32)
                throw new NotSupportedException("Only 24-bit and 32-bit BMPs are supported.");
            reader.ReadBytes(dataOffset - 30);
            Color32[] pixels = new Color32[width * height];
            bool is32Bit = bitsPerPixel == 32;
            int rowPadding = is32Bit ? 0 : (4 - (width * 3) % 4) % 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte b = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte r = reader.ReadByte();
                    byte a = is32Bit ? reader.ReadByte() : (byte)255;

                    pixels[(height - y - 1) * width + x] = new Color32(r, g, b, a);
                }
                if (!is32Bit)
                    reader.ReadBytes(rowPadding);
            }
            target.Resize(width, height, TextureFormat.RGBA32, false);
            target.SetPixels32(pixels);
            target.Apply();
        }
    }
    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
    public static string Replace(string input, string target, string replacement)
    {
        var result = new System.Text.StringBuilder();
        bool insideString = false;
        int i = 0;
        while (i < input.Length)
        {
            if (input[i] == '"')
            {
                result.Append(input[i]);
                insideString = !insideString;
                i++;
                continue;
            }
            if (!insideString && input.Substring(i).StartsWith(target))
            {
                result.Append(replacement);
                i += target.Length;
            }
            else
            {
                result.Append(input[i]);
                i++;
            }
        }
        return result.ToString();
    }
    public static string ReplaceInString(string input, string target, string replacement)
    {
        var result = new System.Text.StringBuilder();
        bool insideString = false;
        int i = 0;

        while (i < input.Length)
        {
            if (input[i] == '"')
            {
                result.Append(input[i]);
                insideString = !insideString;
                i++;
                continue;
            }

            if (insideString && input.Substring(i).StartsWith(target))
            {
                result.Append(replacement);
                i += target.Length;
            }
            else
            {
                result.Append(input[i]);
                i++;
            }
        }

        return result.ToString();
    }
    public static string Unescape(string input)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\' && i + 1 < input.Length)
            {
                char next = input[i + 1];
                switch (next)
                {
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case '\\': result.Append('\\'); break;
                    case '"': result.Append('\"'); break;
                    case '\'': result.Append('\''); break;
                    case '0': result.Append('\0'); break;
                    case 'b': result.Append('\b'); break;
                    case 'f': result.Append('\f'); break;
                    case 'v': result.Append('\v'); break;
                    default:
                        result.Append('\\').Append(next);
                        break;
                }
                i++;
            }
            else
            {
                result.Append(input[i]);
            }
        }
        return result.ToString();
    }
    void Awake()
    {
        interpreter = GetComponent<MiniInterpreter>();
        interpreter.InitializeInterpreter();
        mats = new List<Material>();
        drawStack = new List<List<object>>();
        mesh = meshFilter.mesh;
        start = Time.time;
        if (!PlayerPrefs.HasKey("LOAD"))
        {
            PlayerPrefs.SetString("LOAD", "");
            PlayerPrefs.Save();
            RaiseError("Don't have key LOAD.");
        }
        else
        {
            string path = PlayerPrefs.GetString("LOAD");
            if (path != string.Empty)
            {
                if (File.Exists(path))
                {

                    stackIn = File.ReadAllText(path);
                    EntryPoint(stackIn);
                }
                else
                {
                    RaiseError("The path \"" + path + "\" doesn't exist.");
                }
#if !UNITY_EDITOR
                PlayerPrefs.SetString("LOAD", "");
                PlayerPrefs.Save();
#endif
            }
            else
            {
#if UNITY_EDITOR
                path = "Assets/LOAD";
#else
                path = "Redmond_Scene_Data/LOAD";
#endif
                if (File.Exists(path))
                {
                    path = File.ReadAllText(path);
                    if (File.Exists(path))
                    {
                        stackIn = File.ReadAllText(path);
                        EntryPoint(stackIn);
                    }
                    else
                    {
                        RaiseError("Path in LOAD not found.");
                    }
                }
                else
                {
                    RaiseError("No path or LOAD provided.");
                }
            }
        }
    }
    private void Run(string str)
    {
        bool is_string = false;
        string buffer = string.Empty;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '"')
            {
                is_string = !is_string;
            }
            else if ((str[i]!=' ')||(is_string))
            {
                buffer = buffer+str[i];
            }
            else if (str[i]==' ')
            {
                buffer = buffer + ",";
            }
        }
        List<string> buffer_split = buffer.Split(';').ToList();
        buffer_split.RemoveAll(s => s == "");
        List<List<string>> buffer_split_split = new List<List<string>>();
        List<List<object>> stack1 = new List<List<object>>();
        for (int i = 0;i < buffer_split.Count; i++)
        {
            buffer_split_split.Add(buffer_split[i].Split(',').ToList());
            stack1.Add(new List<object>());
            for (int x = 0; x  < buffer_split_split[i].Count; x++)
            {
                if (parse_number(buffer_split_split[i][x]) != null)
                {
                    stack1[i].Add((float)parse_number(buffer_split_split[i][x]));
                }
                else
                {
                    stack1[i].Add(buffer_split_split[i][x]);
                }
            }
        }
        for (int i = 0; i < stack1.Count; i++)
        {
            if (stack1[i].Count > 1)
            {
                if (stack1[i][1].ToString() == "=")
                {
                    if (stack1[i][2].ToString() != "FUNC")
                    {
                        List<object> buff = new List<object>();
                        for (int c = 0; c < stack1[i].Count; c++)
                        {
                            buff.Add(stack1[i][c]);
                        }
                        for (int c = 2; c < stack1[i].Count; c++)
                        {
                            buff[c-2]=stack1[i][c];
                        }
                        buff.RemoveAt(buff.Count - 1);
                        buff.RemoveAt(buff.Count - 1);
                        buff.Add(stack1[i][0]);
                        stack1[i] = buff;
                    }
                    else
                    {
                        string buff = stack1[i][2].ToString();
                        stack1[i][2]=stack1[i][0];
                        stack1[i][0] = buff;
                        stack1[i].Remove("=");
                    }
                }
            }
        }
        List<string> functions = new List<string>();
        for (int i = 0; i < stack1.Count; i++)
        {
            if (stack1[i][0].ToString() != "FUNC") continue;
            if (functions.Contains(stack1[i][1].ToString())) continue;
            functions.Add(stack1[i][1].ToString());
        }
        dict signatures = new dict();
        int_dict definitions_indecies = new int_dict();
        for (int f = 0; f < functions.Count; f++)
        {
            for (int index = 0; index < stack1.Count;  index++)
            {
                int i = stack1.Count - 1 - index;
                if (!((stack1[i][0].ToString() == "FUNC") && (stack1[i][1].ToString() == functions[f]))) continue;
                definitions_indecies[functions[f]] = i;
                List<string> signature = new List<string>();
                for (int x =  2; x < stack1[i].Count; x++)
                {
                    signature.Add(stack1[i][x].ToString());
                }
                signatures[functions[f]] = signature;
                while (stack1[i].Count > 2)
                {
                    stack1[i].Remove(stack1[i][2]);
                }
                stack1[i][0] = "FUNC";
                stack1[i][1] = functions[f];
                for (int y = i+1; y < stack1.Count; y++)
                {
                    if (stack1[y][0].ToString() == "RETURN")
                    {
                        stack1[y][0] = "ADD";
                        stack1[y].Add(3);
                        stack1[y].Add(("@" + functions[f] + "_return"));
                        stack1[y].Add(("@" + functions[f] + "_return"));
                        List<object> temp = new List<object>();
                        temp.Add("JUMP");
                        temp.Add(("@" + functions[f] + "_return"));
                        stack1.Insert(y + 1, temp);
                    }
                }
                break;
            }
        }
        List<List<object>> buffer1 = new List<List<object>>();
        for (int i = 0; i < stack1.Count; i++)
        {
            buffer1.Add(stack1[i]);
        }
        int added = 0;
        for (int f  = 0; f < functions.Count; f++)
        {
            for (int i = 0; i < stack1.Count; i++)
            {
                if ((stack1[i][0].ToString() != "FUNC") || (stack1[i][1].ToString() != functions[f]) || i == definitions_indecies[functions[f]]) continue;
                List<object> arguments = new List<object>();
                for (int x = 2; x < stack1[i].Count; x++)
                {
                    arguments.Add(stack1[i][x]);
                }
                List<object> temp;
                for (int a = 0; a <arguments.Count; a++)
                {
                    if (((List<string>)signatures[functions[f]])[a][0]=='@')
                    {
                        temp = new List<object>();
                        temp.Add("NUM");
                        temp.Add((signatures[functions[f]]));
                        temp.Add(arguments[a]);
                        buffer1.Insert(i + added, temp);
                    }
                    else if (((List<string>)signatures[functions[f]])[a][0] == '$')
                    {
                        temp = new List<object>();
                        temp.Add("STRING");
                        temp.Add(signatures[functions[f]]);
                        temp.Add(arguments[a]);
                        buffer1.Insert(i + added, temp);
                    }
                    added++;
                }
                temp = new List<object>();
                temp.Add("NUM");
                temp.Add(("@" + functions[f] + "_return"));
                buffer1.Insert(i + added, temp);
                added++;
                temp = new List<object>();
                temp.Add("PC");
                temp.Add(("@" + functions[f] + "_return"));
                buffer1.Insert(i + added, temp);
                added++;
                for (int a = 0; a < arguments.Count; a++)
                {
                    temp = new List<object>();
                    temp.Add("DELETE");
                    temp.Add(signatures[functions[f]]);
                    buffer1.Insert(i + added + 1, temp);
                    added++;
                }
                temp = new List<object>();
                temp.Add("DELETE");
                temp.Add(("@" + functions[f] + "_return"));
                buffer1.Insert(i + added + 1, temp);
                added++;
                temp = new List<object>();
                temp.Add("FUNC");
                temp.Add(functions[f]);
                buffer1[i + added - (arguments.Count + 1)] = temp;
            }
        }
        stack = buffer1;
        //string IL = string.Empty;
        //for (int i = 0; i < stack.Count; i++)
        //{
        //    for (int j = 0; j < stack[i].Count; j++)
        //    {
        //        IL += stack[i][j]+", ";
        //    }
        //}
        //print(IL);
        if (stack[stack.Count - 1][0].ToString().Length == 0)
        {
            stack.Remove(stack[stack.Count - 1]);
        }
        pc = 0;
        draw = false;
        useBuffer = false;
        for (int i = 0;i < stack.Count;i++)
        {
            if (stack[i][0].ToString() == "ADDRESS")
            {
                pc = i;
                run_address(stack[i][1]);
            }
        }
        for (int i = 0; i < stack.Count; ++i)
        {
            if (stack[(stack.Count - 1) - i][0].ToString() == "FUNC")
            {
                pc = (stack.Count - 1) - i;
                run_func(stack[(stack.Count - 1) - i][1]);
            }
        }
        pc = 0;
    }
    private void EntryPoint(string stack)
    {
        stack = stack.Replace("C#_START", "EXEC \"");
        stack = stack.Replace("C#_END", "\";");
        stack = Unescape(stack);
        numbers = new dict();
        strings = new dict();
        addresses = new int_dict();
        functions = new int_dict();
        images = new image_dict();
        string exeFolder = Directory.GetParent(Application.dataPath).FullName;
#if !UNITY_EDITOR
        string syntax_path = Path.Combine(exeFolder, ".json\\syntax.json");
#else
        string syntax_path = "C:/Users/duckm/OneDrive/Desktop/Rand/Desktop/Parser/.json/syntax.json";
#endif
        if (File.Exists(syntax_path))
        {
            syntax_string = File.ReadAllText(syntax_path);
        }
        else
        {
            RaiseError("The path \"" + syntax_path + "\" doesn't exist.");
        }
        Dictionary<string, object> syntax = (Dictionary<string, object>)MiniJSON.Json.Deserialize(syntax_string);
        foreach (KeyValuePair<string, object> kvp in syntax)
        {
            string key = kvp.Key;
            if (key == "comment")
            {
                comment = (Dictionary<string, object>)kvp.Value;
            }
            else if (key == "tooltip_arguments")
            {
                tooltip_arguments = (Dictionary<string, object>)kvp.Value;
            }
            else if (key == "tooltip_descriptions")
            {
                tooltip_descriptions = (Dictionary<string, object>)kvp.Value;
            }
        }
        string str = stack;
        str = ReplaceInString(str, ";", "¶");
        while (str.Contains("\n\n"))
        {
            str = str.Replace("\n\n", "\n");
        }
        str = Replace(str,"(", " ");
        str = Replace(str,")", "");
        str = str.Replace("BREAKPOINT;", "ADDRESS BREAKPOINT;\nBUFFER;\nADDRESS BREAKPOINT;");
        string comment_start = Regex.Escape(comment["start"].ToString());
        string comment_end = Regex.Escape(comment["end"].ToString());
        string pattern = comment_start+".*?"+comment_end;
        str = Regex.Replace(str, pattern, ";");
        MatchCollection matches = Regex.Matches(str, @"{.*?}", RegexOptions.Singleline);
        List<string> stringJSON = new List<string>();
        for (int i = 0; i < matches.Count; i++)
        {
            stringJSON.Add(matches[i].Value);
        }
        if (stringJSON.Count != 0)
        {
            matches = Regex.Matches(str, @"{.*?}", RegexOptions.Singleline);
            stringJSON = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                stringJSON.Add(matches[i].Value);
            }
            for (int i = 0; i < stringJSON.Count; i++)
            {
                str = str.Replace(stringJSON[i], "");
            }
            str = str.Replace("}", "");
            str = str.Replace("{", "");
            str = Replace(str, "\n", "");
            string JSON = "";
            for (int i = 0; i < stringJSON.Count; i++)
            {
                if (Count(stringJSON[i], "{") != Count(stringJSON[i], "}"))
                {
                    stringJSON[i] = stringJSON[i] + "\n}";
                }
                if (i != stringJSON.Count - 1)
                {
                    JSON = JSON + stringJSON[i] + ",\n";
                }
                else
                {
                    JSON = JSON + stringJSON[i] + "\n";
                }
            }
            JSON = JSON.Replace("\n},\n{", ",");
            if (JSON != string.Empty)
            {
                Dictionary<string, object> alloc = (Dictionary<string, object>)MiniJSON.Json.Deserialize(JSON);
                foreach (KeyValuePair<string, object> kvp in alloc)
                {
                    string key = kvp.Key;
                    object value = kvp.Value;
                    if (key[0].ToString() == "@") numbers[key] = ToFloat(value);
                    else if (key[0].ToString() == "$") strings[key] = value;
                    else RaiseError("Variable has invalid data type: " + key);
                }
            }
        }
        Run(str);
    }
    private void Update()
    {
        drawStack.Clear();
        buffer = false;
        while (!buffer && Time.frameCount!=1)
        {
            if (pc >= stack.Count || pc == -1)
            {
                Quit();
                return;
            }
            switch (stack[pc][0].ToString())
            {
                case "EXIT":
                    run_exit();
                    break;
                case "NUM":
                    if (stack[pc].Count > 2)
                    {
                        float numArg = 0.0f;
                        for (int i = 2; i < stack[pc].Count; i++)
                        {
                            if ((stack[pc][i].ToString())[0] == '@')
                            {
                                numArg += (float)numbers[stack[pc][i].ToString()];
                            }
                            else if ((stack[pc][i].ToString())[0] == '$')
                            {
                                numArg += (float)parse_number(strings[stack[pc][i].ToString()].ToString());
                            }
                            else
                            {
                                numArg += (float)parse_number(stack[pc][i].ToString());
                            }
                        }
                        run_alloc(stack[pc][1], numArg);
                    }
                    else
                    {
                        run_alloc(stack[pc][1]);
                    }
                    break;
                case "PRINT":
                    string arg = String.Empty;
                    for (int i = 1; i < stack[pc].Count; i++)
                    {
                        if ((stack[pc][i].ToString())[0] == '@')
                        {
                            arg += numbers[stack[pc][i].ToString()].ToString();
                        }
                        else if ((stack[pc][i].ToString())[0] == '$')
                        {
                            arg += strings[stack[pc][i].ToString()].ToString();
                        }
                        else
                        {
                            arg += stack[pc][i].ToString();
                        }
                    }
                    run_print(arg);
                    break;
                case "KEY":
                    run_key(stack[pc][1], stack[pc][2]);
                    break;
                case "GREATER":
                    run_greater(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "JUMP":
                    run_jump(stack[pc][1]);
                    break;
                case "INC":
                    run_inc(stack[pc][1]);
                    break;
                case "DEC":
                    run_dec(stack[pc][1]);
                    break;
                case "WINDOW":
                    run_window(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "PIXEL":
                    run_pixel(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4], stack[pc][5]);
                    break;
                case "BUFFER":
                    run_buffer();
                    break;
                case "ADDRESS":
                    run_address(stack[pc][1]);
                    break;
                case "TIME":
                    run_time(stack[pc][1]);
                    break;
                case "SUB":
                    run_sub(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "ADD":
                    run_add(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "MUL":
                    run_mul(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "DIV":
                    run_div(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "MODULUS":
                    run_mod(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "POWER":
                    run_power(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "RADICAL":
                    run_radical(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "WRITE":
                    run_write(stack[pc][1], stack[pc][2]);
                    break;
                case "READ":
                    run_read(stack[pc][1], stack[pc][2]);
                    break;
                case "STRING":
                    if (stack[pc].Count > 2)
                    {
                        arg = String.Empty;
                        for (int i = 2; i < stack[pc].Count; i++)
                        {
                            if ((stack[pc][i].ToString())[0] == '@')
                            {
                                arg += numbers[stack[pc][i].ToString()].ToString();
                            }
                            else if ((stack[pc][i].ToString())[0] == '$')
                            {
                                arg += strings[stack[pc][i].ToString()].ToString();
                            }
                            else
                            {
                                arg += stack[pc][i].ToString();
                            }
                        }
                        run_string(stack[pc], arg);
                    }
                    else
                    {
                        run_string(stack[pc]);
                    }
                    break;
                case "REPLACE":
                    run_replace(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4]);
                    break;
                case "INDEX":
                    run_index(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "SPLIT":
                    run_split(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "LEN":
                    run_len(stack[pc][1], stack[pc][2]);
                    break;
                case "PC":
                    run_pc(stack[pc][1]);
                    break;
                case "FUNC":
                    run_func(stack[pc][1]);
                    break;
                case "DELETE":
                    run_delete(stack[pc][1]);
                    break;
                case "INT":
                    run_int(stack[pc][1], stack[pc][2]);
                    break;
                case "FLOAT":
                    run_float(stack[pc][1], stack[pc][2]);
                    break;
                case "MIN":
                    run_min(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "MAX":
                    run_max(stack[pc][1], stack[pc][2], stack[pc][3]);
                    break;
                case "RECT":
                    run_rect(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4], stack[pc][5], stack[pc][6], stack[pc][7]);
                    break;
                case "GRADIENT":
                    run_gradient(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4], stack[pc][5], stack[pc][6], stack[pc][7], stack[pc][8], stack[pc][9], stack[pc][10], stack[pc][11]);
                    break;
                case "TRIG":
                    run_trig(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4], stack[pc][5], stack[pc][6], stack[pc][7], stack[pc][8], stack[pc][9]);
                    break;
                case "SIN":
                    run_sin(stack[pc][1], stack[pc][2]);
                    break;
                case "COS":
                    run_cos(stack[pc][1], stack[pc][2]);
                    break;
                case "DEBUG":
                    run_debug();
                    break;
                case "HELP":
                    run_help();
                    break;
                case "JALLOC":
                    run_jalloc(stack[pc][1]);
                    break;
                case "IMAGE":
                    run_image(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4], stack[pc][5], stack[pc][6], stack[pc][7], stack[pc][8], stack[pc][9]);
                    break;
                case "BLIT":
                    run_blit(stack[pc][1], stack[pc][2], stack[pc][3], stack[pc][4]);
                    break;
                case "cls":
                    run_cls();
                    break;
                case "DIM":
                    run_dim(stack[pc][1], stack[pc][2]);
                    break;
                case "EXEC":
                    run_exec(stack[pc][1]);
                    break;
            }
            pc++;
        }
        if (drawStack.Count!=0)
        {
            DrawStack();
        }
    }
    private void run_alloc(object arg1_obj, object arg2_obj = null)
    {
        if (arg2_obj == null)
        {
            numbers[arg1_obj.ToString()] = 0.0f;
            return;
        }
        string arg1 = arg1_obj.ToString();
        if (arg2_obj.ToString()[0].ToString() == "@")
        {
            numbers[arg1] = numbers[arg2_obj.ToString()];
        }
        else
        {
            if (arg2_obj.GetType() == typeof(int))
            {
                numbers[arg1] = (int)arg2_obj;
            }
            else
            {
                numbers[arg1] = (float)arg2_obj;
            }
        }
    }
    private void run_exit()
    {
        pc = -2;
    }
    private void run_print(object arg)
    {
        Print(arg.ToString());
    }
    private void run_greater(object arg1, object arg2, object arg3)
    {
        if ((arg1.ToString())[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if ((arg2.ToString())[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        if ((float)arg1 > (float)arg2)
        {
            if (arg3.GetType() == typeof(int))
            {
                run_jump(arg3);
            }
            else
            {
                run_address(arg3);
            }
        }
    }
    private void run_key(object arg1, object arg2)
    {
        numbers[arg2.ToString()] = 0.0f;
        if (Input.GetKey(Key(arg1.ToString())))
        {
            numbers[arg2.ToString()] = 1.0f;
        }
    }
    private void run_address(object arg_obj)
    {
        string arg = arg_obj.ToString();
        if (addresses.keys.Contains(arg))
        {
            pc = addresses[arg];
        }
        else
        {
            addresses[arg] = pc;
        }
    }
    private void run_inc(object arg)
    {
        numbers[arg.ToString()] = (float)numbers[arg.ToString()] + 1.0f;
    }
    private void run_dec(object arg)
    {
        numbers[arg.ToString()] = (float)numbers[arg.ToString()] - 1.0f;
    }
    private void run_pixel(object x, object y, object r, object g, object b)
    {
        draw = true;
        if (draw)
        {
            if ((x.ToString())[0] == '@')
            {
                x = (float)numbers[x.ToString()];
            }
            if ((y.ToString())[0] == '@')
            {
                y = (float)numbers[y.ToString()];
            }
            if ((r.ToString())[0] == '@')
            {
                r = (float)numbers[r.ToString()];
            }
            if ((g.ToString())[0] == '@')
            {
                g = (float)numbers[g.ToString()];
            }
            if ((b.ToString())[0] == '@')
            {
                b = (float)numbers[b.ToString()];
            }
            List<object> obj = new List<object>();
            obj.Add(graphicsObjects.IndexOf("pixel"));
            obj.Add(x);
            obj.Add(y);
            obj.Add(r);
            obj.Add(g);
            obj.Add(b);
            drawStack.Insert(0, obj);
            if (!useBuffer)
            {
                buffer = true;
            }
        }
    }
    private void run_rect(object x, object y, object w, object h, object r, object g, object b)
    {
        draw = true;
        if (draw)
        {
            if ((x.ToString())[0] == '@')
            {
                x = (float)numbers[x.ToString()];
            }
            if ((y.ToString())[0] == '@')
            {
                y = (float)numbers[y.ToString()];
            }
            if ((w.ToString())[0] == '@')
            {
                w = (float)numbers[w.ToString()];
            }
            if ((h.ToString())[0] == '@')
            {
                h = (float)numbers[h.ToString()];
            }
            if ((r.ToString())[0] == '@')
            {
                r = (float)numbers[r.ToString()];
            }
            if ((g.ToString())[0] == '@')
            {
                g = (float)numbers[g.ToString()];
            }
            if ((b.ToString())[0] == '@')
            {
                b = (float)numbers[b.ToString()];
            }
            List<object> obj = new List<object>();
            obj.Add(graphicsObjects.IndexOf("rect"));
            obj.Add(x);
            obj.Add(y);
            obj.Add(w);
            obj.Add(h);
            obj.Add(r);
            obj.Add(g);
            obj.Add(b);
            drawStack.Insert(0, obj);
            if (!useBuffer)
            {
                buffer = true;
            }
        }
    }
    private void run_gradient(object x, object y, object w, object h, object r1, object g1, object b1, object r2, object g2, object b2, object vert)
    {
        draw = true;
        if (draw)
        {
            if ((x.ToString())[0] == '@')
            {
                x = (float)numbers[x.ToString()];
            }
            if ((y.ToString())[0] == '@')
            {
                y = (float)numbers[y.ToString()];
            }
            if ((w.ToString())[0] == '@')
            {
                w = (float)numbers[w.ToString()];
            }
            if ((h.ToString())[0] == '@')
            {
                h = (float)numbers[h.ToString()];
            }
            if ((r1.ToString())[0] == '@')
            {
                r1 = (float)numbers[r1.ToString()];
            }
            if ((g1.ToString())[0] == '@')
            {
                g1 = (float)numbers[g1.ToString()];
            }
            if ((b1.ToString())[0] == '@')
            {
                b1 = (float)numbers[b1.ToString()];
            }
            if ((r2.ToString())[0] == '@')
            {
                r2 = (float)numbers[r2.ToString()];
            }
            if ((g2.ToString())[0] == '@')
            {
                g2 = (float)numbers[g2.ToString()];
            }
            if ((b2.ToString())[0] == '@')
            {
                b2 = (float)numbers[b2.ToString()];
            }
            if ((vert.ToString())[0] == '@')
            {
                vert = (float)numbers[vert.ToString()];
            }
            List<object> obj = new List<object>();
            obj.Add(graphicsObjects.IndexOf("gradient"));
            obj.Add(x);
            obj.Add(y);
            obj.Add(w);
            obj.Add(h);
            obj.Add(r1);
            obj.Add(g1);
            obj.Add(b1);
            obj.Add(r2);
            obj.Add(g2);
            obj.Add(b2);
            obj.Add(vert);
            drawStack.Insert(0, obj);
            if (!useBuffer)
            {
                buffer = true;
            }
        }
    }
    private void run_trig(object x1, object y1, object x2, object y2, object x3, object y3, object r, object g, object b)
    {
        draw = true;
        if (draw)
        {
            if ((x1.ToString())[0] == '@')
            {
                x1 = (float)numbers[x1.ToString()];
            }
            if ((y1.ToString())[0] == '@')
            {
                y1 = (float)numbers[y1.ToString()];
            }
            if ((x2.ToString())[0] == '@')
            {
                x2 = (float)numbers[x2.ToString()];
            }
            if ((y2.ToString())[0] == '@')
            {
                y2 = (float)numbers[y2.ToString()];
            }
            if ((x3.ToString())[0] == '@')
            {
                x3 = (float)numbers[x3.ToString()];
            }
            if ((y3.ToString())[0] == '@')
            {
                y3 = (float)numbers[y3.ToString()];
            }
            if ((r.ToString())[0] == '@')
            {
                r = (float)numbers[r.ToString()];
            }
            if ((g.ToString())[0] == '@')
            {
                g = (float)numbers[g.ToString()];
            }
            if ((b.ToString())[0] == '@')
            {
                b = (float)numbers[b.ToString()];
            }
            List<object> obj = new List<object>();
            obj.Add(graphicsObjects.IndexOf("trig"));
            obj.Add(x1);
            obj.Add(y1);
            obj.Add(x2);
            obj.Add(y2);
            obj.Add(x3);
            obj.Add(y3);
            obj.Add(r);
            obj.Add(g);
            obj.Add(b);
            drawStack.Insert(0, obj);
            if (!useBuffer)
            {
                buffer = true;
            }
        }
    }
    private void run_buffer()
    {
        buffer = true;
    }
    private void run_jump(object arg)
    {
        if (arg.ToString()[0] == '@')
        {
            arg = (float)numbers[arg.ToString()];
        }
        pc = (int)(float)arg - 2;
    }
    private void run_window(object arg, object arg1, object arg2)
    {
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        draw = true;
        useBuffer = (float)arg == (float)1;
        Screen.SetResolution((int)(float)arg1, (int)(float)arg2, Screen.fullScreen);
    }
    private void run_time(object arg)
    {
        numbers[arg.ToString()] = Time.time-start;
    }
    private void run_sub(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = (float)arg1 - (float)arg2;
    }
    private void run_add(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = (float)arg1 + (float)arg2;
    }
    private void run_mul(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = (float)arg1 * (float)arg2;
    }
    private void run_div(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = (float)arg1 / (float)arg2;
    }
    private void run_mod(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = (float)arg1 % (float)arg2;
    }
    private void run_power(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = Mathf.Pow((float)arg1, (float)arg2);
    }
    private void run_radical(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = Mathf.Pow((float)arg1, 1/(float)arg2);
    }
    private void run_read(object arg1, object arg2)
    {
        strings[arg2.ToString()] = File.ReadAllText(GetPath(arg1.ToString()));
    }
    private void run_write(object arg1, object arg2)
    {
        if (arg2.ToString()[0] == '$')
        {
            arg2 = strings[arg2.ToString()];
        }
        else if (arg2.ToString()[0] == '@')
        {
            arg2 = numbers[arg2.ToString()].ToString();
        }
        else
        {
            arg2 = arg2.ToString().Replace("\"","");
        }
        File.WriteAllText(GetPath(arg1.ToString()), arg2.ToString());
    }
    private void run_string(object arg1, object arg2 = null)
    {
        if (arg2 == null)
        {
            arg2 = "";
        }
        if (arg2.ToString()[0] == '$')
        {
            arg2 = strings[arg2.ToString()];
        }
        else if (arg2.ToString()[0] == '@')
        {
            arg2 = numbers[arg2.ToString()].ToString();
        }
        else
        {
            arg2 = arg2.ToString().Replace("\"", "");
        }
        strings[arg1.ToString()] = arg2.ToString();
    }
    private void run_replace(object arg1, object arg2, object arg3, object arg4)
    {
        if (arg1.ToString()[0] == '$')
        {
            arg1 = strings[arg1.ToString()];
        }
        else
        {
            arg1 = arg1.ToString().Replace("\"", "");
        }
        if (arg2.ToString()[0] == '$')
        {
            arg2 = strings[arg2.ToString()];
        }
        else
        {
            arg2 = arg2.ToString().Replace("\"", "");
        }
        if (arg3.ToString()[0] == '$')
        {
            arg3 = strings[arg3.ToString()];
        }
        else
        {
            arg3 = arg3.ToString().Replace("\"", "");
        }
        strings[arg4.ToString()] = arg1.ToString().Replace(arg2.ToString(), arg3.ToString());
    }
    private void run_split(object arg1, object arg2, object arg3)
    {
        if (arg1.ToString()[0] == '$')
        {
            arg1 = strings[arg1.ToString()];
        }
        else
        {
            arg1 = arg1.ToString().Replace("\"", "");
        }
        if (arg2.ToString()[0] == '$')
        {
            arg2 = strings[arg2.ToString()];
        }
        else
        {
            arg2 = arg2.ToString().Replace("\"", "");
        }
        strings[arg3.ToString()] = Split(arg1.ToString(),arg2.ToString());
    }
    private void run_index(object arg1, object arg2, object arg3)
    {
        if (arg1.ToString()[0] == '@')
        {
            arg1 = numbers[arg1.ToString()];
        }
        else
        {
            arg1 = strings[arg1.ToString()];
        }
        if (arg2.GetType() != typeof(int) && arg2.GetType() != typeof(float))
        {
            if (arg2.ToString()[0] == '@')
            {
                arg2 = numbers[arg2.ToString()];
            }
            else if (arg2.ToString()[0] == '$')
            {
                arg2 = parse_number(strings[arg2.ToString()].ToString());
            }
        }
        if (arg3.ToString()[0] == '$')
        {
            strings[arg3.ToString()] = ((List<object>)arg1)[(int)(float)arg2];
        }
        else
        {
            numbers[arg3.ToString()] = ((List<float>)arg1)[(int)(float)arg2];
        }
    }
    private void run_len(object arg1, object arg2)
    {
        if (arg1.ToString()[0] == '$')
        {
            arg1 = strings[arg1.ToString()];
        }
        else if (arg1.ToString()[0] == '@')
        {
            arg1 = numbers[arg1.ToString()];
        }
        numbers[arg2.ToString()] = arg1.ToString().Length;
    }
    private void run_pc(object arg1)
    {
        numbers[arg1.ToString()] = (float)pc;
    }
    private void run_func(object arg_obj)
    {
        string arg = arg_obj.ToString();
        if (functions.keys.Contains(arg))
        {
            pc = functions[arg];
        }
        else
        {
            functions[arg] = pc;
        }
    }
    private void run_delete(object arg)
    {
        if (arg.ToString()[0] == '$')
        {
            strings.DeleteKey(arg.ToString());
        }
        else if (arg.ToString()[0] == '@')
        {
            numbers.DeleteKey(arg.ToString());
        }
        else if (arg.ToString()[0] == '#')
        {
            images.DeleteKey(arg.ToString());
        }
    }
    private void run_int(object arg1, object arg2)
    {
        run_float(arg1, arg2);
    }
    private void run_float(object arg1, object arg2)
    {
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        numbers[arg2.ToString()] = (float)arg1;
    }
    private void run_max(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = Mathf.Max((float)arg1, (float)arg2);
    }
    private void run_min(object arg1, object arg2, object arg3)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg2.GetType() == typeof(int))
        {
            arg2 = (float)(int)arg2;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        if (arg2.ToString()[0] == '@')
        {
            arg2 = (float)numbers[arg2.ToString()];
        }
        numbers[arg3.ToString()] = Mathf.Min((float)arg1, (float)arg2);
    }
    private void run_sin(object arg1, object arg2)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        numbers[arg2.ToString()] = Mathf.Sin((float)arg1);
    }
    private void run_cos(object arg1, object arg2)
    {
        if (arg1.GetType() == typeof(int))
        {
            arg1 = (float)(int)arg1;
        }
        if (arg1.ToString()[0] == '@')
        {
            arg1 = (float)numbers[arg1.ToString()];
        }
        numbers[arg2.ToString()] = Mathf.Cos((float)arg1);
    }
    private void run_debug()
    {
        Print("Debuging not available in Redmond Scene, sorry!");
    }
    private void run_help()
    {
        print("Redmond Scene\n\n");
        foreach (KeyValuePair<string, object> kvp in tooltip_descriptions)
        {
            Print(kvp.Key + ": " + tooltip_descriptions[kvp.Key].ToString() + " (Usage: " + tooltip_arguments[kvp.Key].ToString() + ").");
        }
    }
    private void run_jalloc(object arg)
    {
        Dictionary<string, object> alloc = (Dictionary<string, object>)MiniJSON.Json.Deserialize(arg.ToString());
        foreach (KeyValuePair<string, object> kvp in alloc)
        {
            string key = kvp.Key;
            object value = kvp.Value;
            if (key[0].ToString() == "@") numbers[key] = value;
            else if (key[0].ToString() == "$") strings[key] = value;
            else RaiseError("Variable has invalid data type: " + key);
        }

    }
    private void run_image(object image, object w, object h, object uv_x, object uv_y, object uv_w, object uv_h, object filter, object outVar)
    {
        if (image.ToString()[0] == '$')
        {
            image = strings[image.ToString()];
        }
        image = Path.Combine("C:/Users/duckm/OneDrive/Desktop/Rand/DESKTOP/Parser/", image.ToString());
        if (w.ToString()[0] == '@')
        {
            w = (float)numbers[w.ToString()];
        }
        if (h.ToString()[0] == '@')
        {
            h = (float)numbers[h.ToString()];
        }
        if (uv_x.ToString()[0] == '@')
        {
            uv_x = (float)numbers[uv_x.ToString()];
        }
        if (uv_y.ToString()[0] == '@')
        {
            uv_y = (float)numbers[uv_y.ToString()];
        }
        if (uv_w.ToString()[0] == '@')
        {
            uv_w = (float)numbers[uv_w.ToString()];
        }
        if (uv_h.ToString()[0] == '@')
        {
            uv_h = (float)numbers[uv_h.ToString()];
        }
        if (filter.ToString()[0] == '@')
        {
            filter = (float)numbers[filter.ToString()];
        }
        byte[] bytes = File.ReadAllBytes(image.ToString());
        Texture2D surface = new Texture2D(1,1);
        OverwriteTextureWithBmp(surface, bytes);
        if ((float)filter == (float)1)
        {
            surface.filterMode = FilterMode.Bilinear;
        }
        else
        {
            surface.filterMode = FilterMode.Point;
        }
        surface.name = Path.GetFileNameWithoutExtension(image.ToString());
        Material mat = null;
        for (int i = 0; i < mats.Count; i++)
        {
            if (mats[i].name == surface.name)
            {
                mat = mats[i];
            }
        }
        if (mat == null)
        {
            MeshRenderer mr = meshFilter.gameObject.GetComponent<MeshRenderer>();
            mat = new Material(mr.material);
            mat.name = surface.name;
            mat.SetTexture("_MainTex", surface);
            List<Material> mrl = mr.materials.ToList();
            mrl.Add(mat);
            mr.materials = mrl.ToArray();
            mats.Add(mat);
        }
        image imageSurface = new image();
        imageSurface.texture = surface.name;
        imageSurface.width = (float)w;
        imageSurface.height = (float)h;
        imageSurface.uv_x = (float)uv_x;
        imageSurface.uv_y = (float)uv_y;
        imageSurface.uv_w = (float)uv_w;
        imageSurface.uv_h = (float)uv_h;
        images[outVar.ToString()] = imageSurface;
    }
    private void run_blit(object image, object x,  object y, object r)
    {
        if (x.ToString()[0] == '@')
        {
            x = (float)numbers[x.ToString()];
        }
        if (y.ToString()[0] == '@')
        {
            y = (float)numbers[y.ToString()];
        }
        if (r.ToString()[0] == '@')
        {
            r = (float)numbers[r.ToString()];
        }
        List<object> drawObject = new List<object>();
        image image1 = images[image.ToString()];
        drawObject.Add(graphicsObjects.IndexOf("image"));
        drawObject.Add(image1.texture);
        drawObject.Add(image1.width);
        drawObject.Add(image1.height);
        drawObject.Add(image1.uv_x);
        drawObject.Add(image1.uv_y);
        drawObject.Add(image1.uv_w);
        drawObject.Add(image1.uv_h);
        drawObject.Add(x);
        drawObject.Add(y);
        drawObject.Add(r);
        drawStack.Insert(0, drawObject);
    }
    private void run_dim(object image,  object outVar)
    {
        if (image.ToString()[0] == '$')
        {
            image = strings[image.ToString()];
        }
        image = Path.Combine("C:/Users/duckm/OneDrive/Desktop/Rand/DESKTOP/Parser/",image.ToString());
        byte[] bytes = File.ReadAllBytes(image.ToString());
        Texture2D tex = new Texture2D(1, 1);
        OverwriteTextureWithBmp(tex, bytes);
        List<float> dim = new List<float>();
        dim.Add(tex.width);
        dim.Add(tex.height);
        numbers[outVar.ToString()] = dim;
    }
    private void run_cls(){}
    private void run_exec(object arg)
    {
        interpreter.Exec(arg.ToString().Replace("¶",";"));
    }
}