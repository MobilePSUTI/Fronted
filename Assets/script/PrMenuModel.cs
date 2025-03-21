using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PrMenuModel : MonoBehaviour
{
    public GameObject loadingIndicator; // Индикатор загрузки
    public InputField loginInput; // Поле для ввода логина
    public InputField passwordInput; // Поле для ввода пароля
    public Text errorText; // Текст для отображения ошибок

    private DatabaseManager databaseManager;

    void Start()
    {
        databaseManager = gameObject.AddComponent<DatabaseManager>();
        databaseManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // Укажите свои параметры

        // Восстанавливаем данные о текущем пользователе
        if (UserSession.CurrentTeacher != null)
        {
            Debug.Log($"Текущий преподаватель: {UserSession.CurrentTeacher.Name}");
        }
    }

    // Метод для входа преподавателя
    public void OnTeacherLoginButtonClick()
    {
        string login = loginInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Логин и пароль не могут быть пустыми.";
            return;
        }

        StartCoroutine(LoginTeacher(login, password));
    }

    IEnumerator LoginTeacher(string login, string password)
    {
        // Показываем индикатор загрузки
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        Debug.Log("Попытка подключения к базе данных...");

        // Подключаемся к базе данных
        if (!databaseManager.Connect())
        {
            Debug.Log("Ошибка подключения к базе данных.");
            errorText.text = "Ошибка подключения к базе данных.";
            yield break;
        }

        Debug.Log("Проверка логина и пароля преподавателя...");

        // Аутентифицируем преподавателя
        Teacher teacher;
        if (databaseManager.AuthenticateTeacher(login, password, out teacher))
        {
            // Сохраняем данные о преподавателе в статическом классе
            UserSession.CurrentTeacher = teacher;
            List<Group> groups = databaseManager.GetAllGroups(); // Теперь типы совпадают
            Debug.Log($"Преподаватель {teacher.Name} успешно авторизован.");
            errorText.text = "";

            // Загружаем новости в кэш
            yield return StartCoroutine(GetNewsFromVK());

            // Переходим на сцену преподавателя
            yield return StartCoroutine(LoadTeacherSceneAsync());
        }
        else
        {
            Debug.Log("Неверный логин или пароль.");
            errorText.text = "Неверный логин или пароль.";
        }

        // Скрываем индикатор загрузки
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    IEnumerator LoadTeacherSceneAsync()
    {
        // Асинхронная загрузка сцены преподавателя
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PrepodModel");

        // Ждем завершения загрузки
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator GetNewsFromVK()
    {
        // Используем VKNewsLoad для загрузки новостей
        var vkNewsLoad = gameObject.AddComponent<VKNewsLoad>();
        yield return StartCoroutine(vkNewsLoad.GetNewsFromVK(0, 100)); // Загружаем все новости

        // Сохраняем данные в кэш
        NewsDataCache.CachedPosts = vkNewsLoad.allPosts;
        NewsDataCache.CachedVKGroups = vkNewsLoad.groupDictionary;

        // Уничтожаем временный компонент
        Destroy(vkNewsLoad);
    }
}