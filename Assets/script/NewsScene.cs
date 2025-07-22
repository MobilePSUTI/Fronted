using UnityEngine;
using UnityEngine.UI;

public class NewsScene : MonoBehaviour
{
    public Image avatarDisplay;
    [SerializeField] private Sprite defaultAvatar; // Аватар по умолчанию

    void Start()
    {
        // Проверяем наличие AvatarManager
        if (AvatarManager.Instance == null)
        {
            Debug.LogError("AvatarManager не найден!");
            SetDefaultAvatar();
            return;
        }

        // Получаем текущий аватар
        Sprite currentAvatar = AvatarManager.Instance.GetCurrentAvatar();

        if (currentAvatar != null)
        {
            avatarDisplay.sprite = currentAvatar;
        }
        else
        {
            Debug.LogWarning("Аватар не найден, используем стандартный");
            SetDefaultAvatar();
        }
    }

    void SetDefaultAvatar()
    {
        if (defaultAvatar != null)
        {
            avatarDisplay.sprite = defaultAvatar;
        }
    }
}