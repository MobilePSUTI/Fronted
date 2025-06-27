using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordVisibilityController : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField; 
    [SerializeField] private Button showPasswordButton;
    [SerializeField] private Sprite eyeOpenSprite;
    [SerializeField] private Sprite eyeClosedSprite;

    private bool isPasswordVisible = false;
    private Image buttonImage;

    private void Start()
    {
        // Получаем компонент Image кнопки
        buttonImage = showPasswordButton.GetComponent<Image>();

        // Устанавливаем начальное состояние (глаз закрыт)
        buttonImage.sprite = eyeClosedSprite;

        // Добавляем обработчик нажатия кнопки
        showPasswordButton.onClick.AddListener(TogglePasswordVisibility);
    }

    private void TogglePasswordVisibility()
    {
        // Меняем состояние видимости пароля
        isPasswordVisible = !isPasswordVisible;

        // Обновляем тип ввода поля пароля
        passwordInputField.contentType = isPasswordVisible ?
            TMP_InputField.ContentType.Standard :
            TMP_InputField.ContentType.Password;

        // Обновляем спрайт кнопки
        buttonImage.sprite = isPasswordVisible ?
            eyeOpenSprite :
            eyeClosedSprite;

        // Обновляем поле ввода, чтобы изменения вступили в силу
        passwordInputField.ForceLabelUpdate();
    }
}
