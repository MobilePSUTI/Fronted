using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance;
    public int selectedAvatarIndex = 0;
    public Sprite[] avatars; // Добавляем массив спрайтов аватарок

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAvatar();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveAvatar(int index)
    {
        selectedAvatarIndex = index;
        PlayerPrefs.SetInt("AvatarIndex", index);
    }

    void LoadAvatar()
    {
        selectedAvatarIndex = PlayerPrefs.GetInt("AvatarIndex", 0);
    }

    // Метод для получения текущего аватара
    public Sprite GetCurrentAvatar()
    {
        if (avatars != null && avatars.Length > selectedAvatarIndex)
        {
            return avatars[selectedAvatarIndex];
        }
        return null;
    }
}