using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSelectionScene : MonoBehaviour
{
    [Header("Settings")]
    public Sprite[] avatars; // 6 �������� � ����������
    public Image oldAvatarDisplay;
    public Image newAvatarDisplay;

    [Header("UI")]
    public Button[] avatarButtons; // 6 ������
    public Button confirmButton;
    public Button backButton;

    private int currentSelection = 0;

    void Start()
    {
        // ���������� ������� ������
        oldAvatarDisplay.sprite = avatars[AvatarManager.Instance.selectedAvatarIndex];

        // ��������� ����������� ��� ������ ��������
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }

        confirmButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(GoBack);
    }

    void SelectAvatar(int index)
    {
        currentSelection = index;
        newAvatarDisplay.sprite = avatars[index];
    }

    void ConfirmSelection()
    {
        // ��������� � ���������� �� ������ (��������)
        AvatarManager.Instance.SaveAvatar(currentSelection);
        Debug.Log($"������ {currentSelection} �������!");

        // ��������� �������
        SceneManager.LoadScene("NewsScene"); // ��� ������ �����
    }

    void GoBack()
    {
        SceneManager.LoadScene("SettingsScene"); // ������ ������
    }
}
