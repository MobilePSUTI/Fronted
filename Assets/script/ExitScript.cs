using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour
{
    public void OnLogoutClicked()
    {
        // Очищаем сохраненные данные игрока (например, токен авторизации)
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.Save();

        // Загружаем сцену входа (замените "LoginScene" на имя вашей сцены)
        SceneManager.LoadScene("MainAuth");
    }
}
