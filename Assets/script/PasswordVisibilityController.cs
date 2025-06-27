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
        // �������� ��������� Image ������
        buttonImage = showPasswordButton.GetComponent<Image>();

        // ������������� ��������� ��������� (���� ������)
        buttonImage.sprite = eyeClosedSprite;

        // ��������� ���������� ������� ������
        showPasswordButton.onClick.AddListener(TogglePasswordVisibility);
    }

    private void TogglePasswordVisibility()
    {
        // ������ ��������� ��������� ������
        isPasswordVisible = !isPasswordVisible;

        // ��������� ��� ����� ���� ������
        passwordInputField.contentType = isPasswordVisible ?
            TMP_InputField.ContentType.Standard :
            TMP_InputField.ContentType.Password;

        // ��������� ������ ������
        buttonImage.sprite = isPasswordVisible ?
            eyeOpenSprite :
            eyeClosedSprite;

        // ��������� ���� �����, ����� ��������� �������� � ����
        passwordInputField.ForceLabelUpdate();
    }
}
