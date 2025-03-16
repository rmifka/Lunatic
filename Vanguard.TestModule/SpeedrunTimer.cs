using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vanguard.TestModule;

public class SpeedrunTimer : MonoBehaviour
{
    private static SpeedrunTimer instance;


    public float realTime;


    private readonly List<Text> splitNameTexts = new();


    private readonly Dictionary<string, float> splits = new();


    private readonly List<Text> splitTimeTexts = new();


    private Canvas canvas;


    private Config config;


    private ConfigManager configManager;


    //private FirstPersonController controller;


    private bool isPaused;


    private Text pausedText;


    private Text realTimeSinceStartupText;


    private Text realTimeText;


    private float sceneTime;


    private Text sceneTimeText;


    private Text sectionFinished;


    private bool showingSplits;


    private float startTimeSinceStartup;


    private new void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }


    private void Start()
    {
        InitializeConfig();
        CreateCanvas();
        InitializeSplits();
        CreateSplitDisplay();
        SceneManager.sceneLoaded += OnSceneLoaded;
        startTimeSinceStartup = Time.realtimeSinceStartup;
        PlayerPrefs.SetInt(PlayerPrefKeys.FISHING, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.BOARD_GAME, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.HIDE_SEEK, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.MIDNIGHT, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.SOMEONE_AT_DOOR, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.RICK, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.WELCOME_TO_WOODBURY, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.MOE_PIZZA, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.BASEMENT, 0);
    }


    private void Update()
    {
        if (!IsPlaying())
        {
            startTimeSinceStartup += Time.deltaTime;
        }

        if (!isPaused && IsPlaying())
        {
            realTime += Time.deltaTime;
            sceneTime += Time.deltaTime;
            UpdateTimerUI();
            RefreshSplits();
        }

        if (SceneManager.GetActiveScene().name == SceneNameKeys.CREDITS_SCENE && !isPaused)
        {
            isPaused = true;
            UpdateTimerUI();
            UpdateRTA();
        }

        if (SceneManager.GetActiveScene().name == SceneNameKeys.MAIN_MENU_SCENE)
        {
            UpdateTimerUI();
            UpdateRTA();
        }

        if (IsPlaying())
        {
            UpdateRTA();
        }

        if (Input.GetKey(KeyCode.V))
        {
            if (!showingSplits)
            {
                DisplaySplits();
                showingSplits = true;
            }
        }
        else if (showingSplits)
        {
            HideSplits();
            showingSplits = false;
        }
    }


    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(PlayerPrefKeys.FISHING, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.BOARD_GAME, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.HIDE_SEEK, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.MIDNIGHT, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.SOMEONE_AT_DOOR, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.RICK, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.WELCOME_TO_WOODBURY, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.MOE_PIZZA, 1);
        PlayerPrefs.SetInt(PlayerPrefKeys.BASEMENT, 1);
    }


    private void CreateCanvas()
    {
        var go = new GameObject("SpeedrunTimerCanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        realTimeText = CreateTextElement("IGT: 0:00.000", config.IGT_Position, config.IGT_Color);
        sceneTimeText = CreateTextElement("STC: 0:00.000", config.STC_Position, config.STC_Color);
        realTimeSinceStartupText = CreateTextElement("RTA: 0:00.000", config.RTA_Position, config.RTA_Color);
        pausedText = CreateTextElement("PAUSED", new Vector2(0f, 800f), Color.red);
        pausedText.alignment = TextAnchor.MiddleCenter;
        pausedText.gameObject.SetActive(false);
        sectionFinished = CreateTextElement("", new Vector2(0f, 800f), Color.green);
        sectionFinished.gameObject.SetActive(false);
    }


    private Text CreateTextElement(string initialText, Vector2 anchoredPosition, Color color)
    {
        var gameObject = new GameObject(initialText);
        gameObject.transform.SetParent(canvas.transform);
        var text = gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = initialText;
        text.fontSize = config.Font_Size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        var component = text.GetComponent<RectTransform>();
        component.anchoredPosition = anchoredPosition;
        component.sizeDelta = new Vector2(800f, 100f);
        component.anchorMin = new Vector2(0.5f, 0f);
        component.anchorMax = new Vector2(0.5f, 0f);
        component.pivot = new Vector2(0.5f, 0f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);
        return text;
    }


    private void UpdateTimerUI()
    {
        realTimeText.text = "IGT: " + FormatTime(realTime);
        sceneTimeText.text = "STC: " + FormatTime(sceneTime);
    }


    private string FormatTime(float time)
    {
        var num = Mathf.FloorToInt(time / 60f);
        var num2 = Mathf.FloorToInt(time % 60f);
        var num3 = time % 1f * 1000f;
        return $"{num}:{num2:00}.{num3:000}";
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateCanvas();
        CreateSplitDisplay();
        sceneTime = 0f;
        if (scene.name == SceneNameKeys.PIZZERIA_SCENE)
        {
            UpdateSplit(SplitNameKeys.OFFICE, realTime);
        }

        if (scene.name == SceneNameKeys.CABIN_SCENE)
        {
            UpdateSplit(SplitNameKeys.MOES_PIZZA, realTime);
        }

        if (scene.name == SceneNameKeys.CREDITS_SCENE)
        {
            UpdateSplit(SplitNameKeys.HIDING_IN_BASEMENT, realTime);
        }

        if (scene.name == SceneNameKeys.OFFICE_SCENE)
        {
            ResetTime();
        }
    }


    public void PauseTimer()
    {
        isPaused = true;
        if (pausedText != null)
        {
            pausedText.gameObject.SetActive(true);
        }
    }


    public void ResumeTimer()
    {
        isPaused = false;
        if (pausedText != null)
        {
            pausedText.gameObject.SetActive(false);
        }
    }


    private void UpdateRTA()
    {
        var num = Time.realtimeSinceStartup - startTimeSinceStartup;
        realTimeSinceStartupText.text = "RTA: " + FormatTime(num);
    }


    private bool IsPlaying()
    {
        var name = SceneManager.GetActiveScene().name;
        return name != SceneNameKeys.MAIN_MENU_SCENE && name != SceneNameKeys.DISCLAIMER_SCENE && name != SceneNameKeys.CREDITS_SCENE;
    }


    public void ResetTime()
    {
        realTime = 0f;
        sceneTime = 0f;
        startTimeSinceStartup = Time.realtimeSinceStartup;
    }


    private void InitializeSplits()
    {
        splits.Add(SplitNameKeys.OFFICE, 0f);
        splits.Add(SplitNameKeys.MOES_PIZZA, 0f);
        splits.Add(SplitNameKeys.WELCOME_TO_WOODBURY, 0f);
        splits.Add(SplitNameKeys.FISHING, 0f);
        splits.Add(SplitNameKeys.BOARD_GAME, 0f);
        splits.Add(SplitNameKeys.HIDE_AND_SEEK, 0f);
        splits.Add(SplitNameKeys.MIDNIGHT, 0f);
        splits.Add(SplitNameKeys.SOMEONE_AT_DOOR, 0f);
        splits.Add(SplitNameKeys.RICK, 0f);
        splits.Add(SplitNameKeys.HIDING_IN_BASEMENT, 0f);
    }


    private void UpdateSplit(string splitName, float time)
    {
        if (splits.ContainsKey(splitName))
        {
            splits[splitName] = time;
            Debug.Log(splitName + " updated: " + FormatTime(time));
            return;
        }

        Debug.LogWarning(splitName + " does not exist in the splits dictionary.");
    }


    private void CheckAndUpdateSplit(string playerPrefKey, string splitName)
    {
        if (PlayerPrefs.GetInt(playerPrefKey) == 1 && splits[splitName] == 0f)
        {
            UpdateSplit(splitName, realTime);
            StartCoroutine(SectionFinished(splitName, sceneTime));
            sceneTime = 0f;
        }
    }


    private void CreateSplitDisplay()
    {
        splitNameTexts.Clear();
        splitTimeTexts.Clear();
        var vector = new Vector2(-1100f, 800f);
        var vector2 = new Vector2(-900f, 800f);
        foreach (var keyValuePair in splits)
        {
            var text = CreateTextElement(keyValuePair.Key, vector, Color.white);
            text.gameObject.SetActive(false);
            splitNameTexts.Add(text);
            var text2 = CreateTextElement(FormatTime(keyValuePair.Value), vector2, config.Split_Color);
            text2.gameObject.SetActive(false);
            splitTimeTexts.Add(text2);
            vector.y -= 30f;
            vector2.y -= 30f;
        }
    }


    private void DisplaySplits()
    {
        for (var i = 0; i < splitNameTexts.Count; i++)
        {
            splitNameTexts[i].gameObject.SetActive(true);
            splitTimeTexts[i].text = FormatTime(splits[splitNameTexts[i].text]);
            splitTimeTexts[i].gameObject.SetActive(true);
        }
    }


    private void HideSplits()
    {
        foreach (var text in splitNameTexts)
        {
            text.gameObject.SetActive(false);
        }

        foreach (var text2 in splitTimeTexts)
        {
            text2.gameObject.SetActive(false);
        }
    }


    public void RefreshSplits()
    {
        CheckAndUpdateSplit(PlayerPrefKeys.FISHING, SplitNameKeys.WELCOME_TO_WOODBURY);
        CheckAndUpdateSplit(PlayerPrefKeys.BOARD_GAME, SplitNameKeys.FISHING);
        CheckAndUpdateSplit(PlayerPrefKeys.HIDE_SEEK, SplitNameKeys.BOARD_GAME);
        CheckAndUpdateSplit(PlayerPrefKeys.MIDNIGHT, SplitNameKeys.HIDE_AND_SEEK);
        CheckAndUpdateSplit(PlayerPrefKeys.SOMEONE_AT_DOOR, SplitNameKeys.MIDNIGHT);
        CheckAndUpdateSplit(PlayerPrefKeys.RICK, SplitNameKeys.SOMEONE_AT_DOOR);
        CheckAndUpdateSplit(PlayerPrefKeys.BASEMENT, SplitNameKeys.RICK);
    }


    private void InitializeConfig()
    {
        configManager = new ConfigManager("speedrun_settings.cfg");
        config = new Config();
        config.STC_Color = configManager.GetColor("STC_Color", config.STC_Color);
        config.RTA_Color = configManager.GetColor("RTA_Color", config.RTA_Color);
        config.Split_Color = configManager.GetColor("Split_Color", config.Split_Color);
        config.Font_Size = configManager.GetInt("Font_Size", config.Font_Size);
        config.IGT_Color = configManager.GetColor("IGT_Color", config.IGT_Color);
        config.STC_Position = configManager.GetVector2("STC_Position", config.STC_Position);
        config.RTA_Position = configManager.GetVector2("RTA_Position", config.RTA_Position);
        config.IGT_Position = configManager.GetVector2("IGT_Position", config.IGT_Position);
    }


    private IEnumerator<WaitForSeconds> SectionFinished(string splitName, float time)
    {
        sectionFinished.text = "Section " + splitName + " finished in " + FormatTime(time);
        sectionFinished.gameObject.SetActive(true);
        yield return new WaitForSeconds(8f);
        sectionFinished.gameObject.SetActive(false);
        yield break;
    }


    private class Config
    {
        public int Font_Size = 24;


        public Color IGT_Color = Color.white;


        public Vector2 IGT_Position = new(0f, 1300f);


        public Color RTA_Color = Color.white;


        public Vector2 RTA_Position = new(-500f, 1300f);


        public Color Split_Color = Color.white;


        public Color STC_Color = Color.white;


        public Vector2 STC_Position = new(500f, 1300f);
    }
}