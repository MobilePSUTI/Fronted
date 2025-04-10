using UnityEngine;
using UnityEngine.UI;

public class MobileKeyboardHandler : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;
    public PlayerController playerController;

    private void Update()
    {
        // ������������� ��������� ���������� ��� ������� (������ �� ��������� �����������)
        if (Application.isMobilePlatform && keyboard == null)
        {
            OpenSystemKeyboard();
        }
    }

    public void OpenSystemKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void OnKeyPressed(string key)
    {
        if (key.Length == 1)
        {
            playerController.OnKeyboardKeyPressed(key[0]);
        }
    }
}