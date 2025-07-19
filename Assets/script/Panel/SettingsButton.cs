using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    public void OnSettingsClick()
    {
        SettingsManager.Instance?.ToggleSettingsPanel();
    }
}