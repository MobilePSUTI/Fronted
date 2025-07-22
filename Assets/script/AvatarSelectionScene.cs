using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSelectionScene : MonoBehaviour
{
    [Header("Settings")]
    public Sprite[] avatars; // 6 аватарок в инспекторе
    public Image oldAvatarDisplay;
    public Image newAvatarDisplay;

    [Header("UI")]
    public Button[] avatarButtons; // 6 кнопок
    public Button confirmButton;
    public Button backButton;

    private int currentSelection = 0;

    void Start()
    {
        // Показываем текущий аватар
        oldAvatarDisplay.sprite = avatars[AvatarManager.Instance.selectedAvatarIndex];

        // Назначаем обработчики для кнопок аватарок
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
        // Сохраняем и отправляем на сервер (заглушка)
        AvatarManager.Instance.SaveAvatar(currentSelection);
        Debug.Log($"Аватар {currentSelection} сохранён!");

        // Переходим обратно
        SceneManager.LoadScene("NewsScene"); // Или другая сцена
    }

    void GoBack()
    {
        SceneManager.LoadScene("SettingsScene"); // Откуда пришли
    }
}
