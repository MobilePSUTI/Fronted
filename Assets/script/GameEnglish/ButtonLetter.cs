// ButtonLetter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonLetter : MonoBehaviour
{
    public char letter; // ����� ������� � ��� ��������, � ������� ������ � ����������
    private Button button;
    private Image buttonImage;
    private Color originalColor;
    public Color pressedColor = Color.green;
    public float colorChangeDuration = 0.3f;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;

        button.onClick.AddListener(OnButtonClick);

        // ������������� ����� ������ ��� ��������� ��������
        Text buttonText = GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = letter.ToString(); // ������� ToUpper()
        }
    }

    private void OnButtonClick()
    {
        // ������ ���� ������
        StartCoroutine(ChangeButtonColor());

        // ���������� ������� ����� ������ (��� ��������� ��������)
        PlayerController.Instance.OnKeyboardKeyPressed(letter);
    }

    private IEnumerator ChangeButtonColor()
    {
        buttonImage.color = pressedColor;
        yield return new WaitForSeconds(colorChangeDuration);
        buttonImage.color = originalColor;
    }
}