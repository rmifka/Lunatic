using UnityEngine;
using UnityEngine.UI;

namespace Vanguard.TestModule;

public class UIExtensions : MonoBehaviour
{
    public static bool speedrunToggleOn;


    private Toggle debugToggle;


    private UIManagerMainMenu mainMenu;


    private Toggle speedrunToggle;


    public void Start()
    {
        if (!PlayerPrefs.HasKey("DEBUG"))
        {
            PlayerPrefs.SetInt("DEBUG", 0);
            PlayerPrefs.SetInt("SPEEDRUN", 0);
            PlayerPrefs.Save();
        }

        mainMenu = FindAnyObjectByType<UIManagerMainMenu>();
        debugToggle = mainMenu.eatingSoundsToggle;
        speedrunToggle = mainMenu.toiletSoundsToggle;
        var componentInChildren = debugToggle.GetComponentInChildren<Text>();
        var componentInChildren2 = speedrunToggle.GetComponentInChildren<Text>();
        componentInChildren.supportRichText = true;
        componentInChildren2.supportRichText = true;
        componentInChildren.text = "<color=#44FF00>Debug Mode</color>";
        componentInChildren2.text = "<color=#FF0044>Speedrun Mode</color>";
        debugToggle.isOn = PlayerPrefs.GetInt("DEBUG", 0) == 1;
        speedrunToggle.isOn = PlayerPrefs.GetInt("SPEEDRUN", 0) == 1;
        OnDebugActivated(debugToggle.isOn);
        OnSpeedrunActivated(speedrunToggle.isOn);
        debugToggle.onValueChanged.AddListener(OnDebugActivated);
        speedrunToggle.onValueChanged.AddListener(OnSpeedrunActivated);
        new GameObject("Speedrun").AddComponent<SpeedrunHandler>();
        new GameObject("Timer").AddComponent<SpeedrunTimer>();
    }


    public void OnDebugActivated(bool isOn)
    {
        // ReSharper disable once Unity.UnknownResource
        Resources.Load<GlobalTestingChecklist>("GlobalTestingChecklist").EnableTestCases = isOn;
        PlayerPrefs.SetInt("DEBUG", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }


    public void OnSpeedrunActivated(bool isOn)
    {
        speedrunToggleOn = isOn;
        PlayerPrefs.SetInt("SPEEDRUN", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}