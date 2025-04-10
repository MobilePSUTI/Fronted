using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AvatarLoader : MonoBehaviour
{
    public Image avatarImage;
    public GameObject loadingIndicator;
    private DatabaseManager databaseManager;

    void Start()
    {
        databaseManager = gameObject.AddComponent<DatabaseManager>();
        databaseManager.Initialize("localhost", "psuti_app", "developer", "developer_password");

        // Получаем ID текущего пользователя (пример)
        int currentUserId = 5; // Замените на реальный ID
        LoadAvatar(currentUserId);
    }

    public void LoadAvatar(int userId)
    {
        StartCoroutine(LoadAvatarCoroutine(userId));
    }

    IEnumerator LoadAvatarCoroutine(int userId)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        avatarImage.gameObject.SetActive(false);

        try
        {
            byte[] avatarData = databaseManager.GetUserAvatar(userId);

            if (avatarData == null || avatarData.Length == 0)
            {
                Debug.LogWarning("Аватар не найден или пустой");
                yield break;
            }

            // Создаем текстуру
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(avatarData))
            {
                Debug.LogError("Не удалось загрузить текстуру из байтов");
                yield break;
            }

            // Создаем спрайт
            avatarImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            avatarImage.gameObject.SetActive(true);
            Debug.Log("Аватар успешно загружен и отображен");
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }
    }
}