using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
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
        if (UserSession.CurrentStudent != null)
        {
            Debug.Log($"Текущий пользователь: {UserSession.CurrentStudent.Name}");
        }
    }

    public void OnLoginButtonClick()
    {
        string login = loginInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Логин и пароль не могут быть пустыми.";
            return;
        }

        StartCoroutine(LoginStudent(login, password));
    }

    IEnumerator LoginStudent(string login, string password)
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

        Debug.Log("Проверка логина и пароля...");

        // Аутентифицируем студента
        Student student;
        if (databaseManager.AuthenticateStudent(login, password, out student))
        {
            // Сохраняем данные о студенте в статическом классе
            UserSession.CurrentStudent = student;
            Debug.Log($"Студент {student.Name} успешно авторизован.");
            errorText.text = "";

            // Загружаем новости в кэш
            yield return StartCoroutine(GetNewsFromVK());

            // Переходим на сцену с новостями
            yield return StartCoroutine(LoadNewsSceneAsync());
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

    IEnumerator LoadNewsSceneAsync()
    {
        // Асинхронная загрузка сцены с новостями
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StudentsScene");

        // Ждем завершения загрузки
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void OnNewsButtonClick()
    {
        if (UserSession.CurrentStudent == null)
        {
            errorText.text = "Сначала войдите в систему.";
            return;
        }

        StartCoroutine(LoadNewsBeforeTransition());
    }

    IEnumerator LoadNewsBeforeTransition()
    {
        // Показываем индикатор загрузки
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Загружаем новости из ВКонтакте
        yield return StartCoroutine(GetNewsFromVK());

        // Переходим на сцену с новостями
        yield return StartCoroutine(LoadNewsSceneAsync());

        // Скрываем индикатор загрузки
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
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