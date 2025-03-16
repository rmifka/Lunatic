using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vanguard.TestModule;

public class TestComponent : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
        Module.VanguardLogger.Debug("Hello from the test component!");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Module.VanguardLogger.Debug($"Scene loaded: {scene.name}");

        var uiM = Resources.FindObjectsOfTypeAll<UIManagerMainMenu>();

        if (uiM.Length == 0)
        {
            return;
        }

        var manager = uiM.First();

        var text = uiM.First().creditsPanel.GetComponentsInChildren<Text>()[1];

        manager.eatingSoundsToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.EATING_SOUNDS, 1) == 1;
        manager.toiletSoundsToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.TOILET_SOUNDS, 1) == 1;
        var eatingSoundsToggleMethod = manager.GetType().GetMethod("OnEatingSoundsToggleValueChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        var toiletSoundsToggleMethod = manager.GetType().GetMethod("OnToiletSoundsToggleValueChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        if (eatingSoundsToggleMethod != null)
        {
            var eatingDelegate = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), manager, eatingSoundsToggleMethod);
            manager.eatingSoundsToggle.onValueChanged.AddListener(eatingDelegate);
        }

        if (toiletSoundsToggleMethod != null)
        {
            var toiletDelegate = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), manager, toiletSoundsToggleMethod);
            manager.toiletSoundsToggle.onValueChanged.AddListener(toiletDelegate);
        }

        gameObject.AddComponent<UIExtensions>();
        text.supportRichText = true;
        text.text = string.Concat(Environment.NewLine, Environment.NewLine, "Rayll ", Environment.NewLine,
            " <color=#FFFFFF><size=15><b>Speedrun Mod</b></size></color> ", Environment.NewLine, " <color=#ff3370>Renschi</color>", Environment.NewLine);
        text.rectTransform.sizeDelta = new Vector2(350f, 350f);
    }
}