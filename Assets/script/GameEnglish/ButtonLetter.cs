// ButtonLetter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonLetter : MonoBehaviour
{
    public char letter; // Буква остаётся в том регистре, в котором задана в инспекторе
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

        // Устанавливаем текст кнопки без изменения регистра
        Text buttonText = GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = letter.ToString(); // Убираем ToUpper()
        }
    }

    private void OnButtonClick()
    {
        // Меняем цвет кнопки
        StartCoroutine(ChangeButtonColor());

        // Отправляем нажатую букву игроку (без изменения регистра)
        PlayerController.Instance.OnKeyboardKeyPressed(letter);
    }

    private IEnumerator ChangeButtonColor()
    {
        buttonImage.color = pressedColor;
        yield return new WaitForSeconds(colorChangeDuration);
        buttonImage.color = originalColor;
    }
}