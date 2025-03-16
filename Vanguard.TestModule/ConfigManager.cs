using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Vanguard.TestModule;

public class ConfigManager
{
    private readonly Dictionary<string, string> configData;


    private readonly string filePath;


    public ConfigManager(string fileName)
    {
        configData = new Dictionary<string, string>();
        filePath = GetExecutablePath(fileName);
        LoadConfig();
    }


    private string GetExecutablePath(string fileName)
    {
        var text = Application.dataPath;
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            text = Directory.GetParent(text)?.FullName;
        }
        else if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            text = Path.Combine(text, "..", "..");
        }

        return Path.Combine(text ?? string.Empty, fileName);
    }


    private void LoadConfig()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Config file not found, creating new one.");
            return;
        }

        foreach (var text in File.ReadAllLines(filePath))
        {
            if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith(";") && !text.StartsWith("#"))
            {
                var num = text.IndexOf('=');
                if (num >= 0)
                {
                    var key = text.Substring(0, num).Trim();
                    var value = text.Substring(num + 1).Trim();
                    configData[key] = value;
                }
            }
        }
    }


    public void SaveConfig()
    {
        using var streamWriter = new StreamWriter(filePath);
        foreach (var keyValuePair in configData)
        {
            streamWriter.WriteLine(keyValuePair.Key + "=" + keyValuePair.Value);
        }
    }


    public void SetFloat(string key, float value)
    {
        configData[key] = value.ToString(CultureInfo.InvariantCulture);
    }


    public void SetInt(string key, int value)
    {
        configData[key] = value.ToString();
    }


    public void SetColor(string key, Color value)
    {
        configData[key] = ColorUtility.ToHtmlStringRGBA(value);
    }


    public float GetFloat(string key, float defaultValue = 0f)
    {
        string s;
        float result;
        if (configData.TryGetValue(key, out s) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return defaultValue;
    }


    public int GetInt(string key, int defaultValue = 0)
    {
        string s;
        int result;
        if (configData.TryGetValue(key, out s) && int.TryParse(s, out result))
        {
            return result;
        }

        return defaultValue;
    }


    public Color GetColor(string key, Color defaultValue)
    {
        string str;
        Color result;
        if (configData.TryGetValue(key, out str) && ColorUtility.TryParseHtmlString("#" + str, out result))
        {
            return result;
        }

        return defaultValue;
    }


    public bool ConfigExists()
    {
        return File.Exists(filePath);
    }


    public void SetVector2(string key, Vector2 value)
    {
        configData[key] = value.x.ToString(CultureInfo.InvariantCulture) + "," + value.y.ToString(CultureInfo.InvariantCulture);
    }


    public Vector2 GetVector2(string key, Vector2 defaultValue)
    {
        string text;
        if (configData.TryGetValue(key, out text))
        {
            var array = text.Split(',');
            float x;
            float y;
            if (array.Length == 2 && float.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
            {
                return new Vector2(x, y);
            }
        }

        return defaultValue;
    }
}