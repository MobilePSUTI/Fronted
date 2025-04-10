using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MySqlConnector;
using System.IO;

public class StudentRegistration : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField secondNameInput;
    public Dropdown groupDropdown;
    public Text errorText;
    public GameObject loadingIndicator;

    // Аватарка
    public GameObject registrationPanel;
    public GameObject avatarSelectionPanel;
    public Image[] avatarOptions;
    public Image selectedAvatarImage;
    private int selectedAvatarIndex = -1;
    private Texture2D[] avatarTextures;

    private DatabaseManager databaseManager;
    private List<Group> groups;

    void Start()
    {
        databaseManager = gameObject.AddComponent<DatabaseManager>();
        databaseManager.Initialize("localhost", "psuti_app", "developer", "developer_password");
        StartCoroutine(LoadGroups());

        // Инициализация панели выбора аватара
        avatarSelectionPanel.SetActive(false);
        LoadAvatarOptions();
    }

    void LoadAvatarOptions()
    {
        // Загрузка доступных аватарок из Resources
        avatarTextures = Resources.LoadAll<Texture2D>("Avatars");

        // Проверка, что текстуры загружены и доступны для чтения
        if (avatarTextures == null || avatarTextures.Length == 0)
        {
            Debug.LogError("Не удалось загрузить текстуры аватаров");
            return;
        }

        // Настройка отображения вариантов аватарок
        for (int i = 0; i < avatarOptions.Length; i++)
        {
            if (i < avatarTextures.Length)
            {
                // Проверяем, что текстура доступна для чтения
                if (avatarTextures[i] == null || !avatarTextures[i].isReadable)
                {
                    Debug.LogError($"Текстура аватара {i} не доступна для чтения");
                    continue;
                }

                int index = i;
                avatarOptions[i].sprite = Sprite.Create(avatarTextures[i],
                    new Rect(0, 0, avatarTextures[i].width, avatarTextures[i].height),
                    new Vector2(0.5f, 0.5f));

                // Добавляем обработчик клика
                Button btn = avatarOptions[i].GetComponent<Button>();
                btn.onClick.AddListener(() => SelectAvatar(index));
            }
            else
            {
                avatarOptions[i].gameObject.SetActive(false);
            }
        }
    }

    void SelectAvatar(int index)
    {
        selectedAvatarIndex = index;
        selectedAvatarImage.sprite = Sprite.Create(avatarTextures[index],
            new Rect(0, 0, avatarTextures[index].width, avatarTextures[index].height),
            new Vector2(0.5f, 0.5f));
    }

    IEnumerator LoadGroups()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        errorText.text = "";
        groupDropdown.ClearOptions();
        groupDropdown.interactable = false;

        if (!databaseManager.Connect())
        {
            errorText.text = "Ошибка подключения к базе данных.";
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            yield break;
        }

        groups = databaseManager.GetAllGroups();
        databaseManager.Disconnect();

        // Добавляем placeholder первым элементом
        groupDropdown.options.Add(new Dropdown.OptionData("Выберите группу"));

        if (groups != null && groups.Count > 0)
        {
            foreach (var group in groups)
            {
                groupDropdown.options.Add(new Dropdown.OptionData(group.Title));
            }
            groupDropdown.interactable = true;
        }
        else
        {
            errorText.text = "Не удалось загрузить список групп.";
        }

        groupDropdown.value = 0;
        groupDropdown.RefreshShownValue();

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    public void OnRegisterButtonClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string secondName = secondNameInput.text.Trim();

        if (groupDropdown.options.Count == 0 || groupDropdown.value < 0)
        {
            errorText.text = "Необходимо выбрать группу.";
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            errorText.text = "Все обязательные поля должны быть заполнены.";
            return;
        }
        if (groupDropdown.value == 0)
        {
            errorText.text = "Необходимо выбрать группу.";
            return;
        }

        if (!IsValidEmail(email))
        {
            errorText.text = "Введите корректный email адрес.";
            return;
        }

        // Переход к выбору аватара вместо немедленной регистрации
        registrationPanel.SetActive(false);
        avatarSelectionPanel.SetActive(true);
        errorText.text = "";
    }

    public void OnConfirmAvatarButtonClick()
    {
        if (selectedAvatarIndex == -1)
        {
            errorText.text = "Пожалуйста, выберите аватар.";
            return;
        }

        // Проверяем, что группы загружены и выбрана валидная группа
        if (groups == null || groups.Count == 0 || groupDropdown.value <= 0)
        {
            errorText.text = "Необходимо выбрать группу.";
            Debug.LogError("Ошибка: группы не загружены или не выбрана");
            return;
        }

        // Получаем данные из полей ввода
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string secondName = secondNameInput.text.Trim();

        // Получаем ID группы (учитываем, что первый элемент - placeholder)
        int selectedGroupIndex = groupDropdown.value - 1;
        if (selectedGroupIndex < 0 || selectedGroupIndex >= groups.Count)
        {
            errorText.text = "Ошибка выбора группы.";
            Debug.LogError($"Неверный индекс группы: {selectedGroupIndex}, всего групп: {groups.Count}");
            return;
        }

        int groupId = groups[selectedGroupIndex].Id;

        // Конвертируем аватар в байты
        byte[] avatarBytes = avatarTextures[selectedAvatarIndex].EncodeToPNG();

        StartCoroutine(RegisterStudent(email, password, firstName, lastName, secondName, groupId, avatarBytes));
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    IEnumerator RegisterStudent(string email, string password,string firstName, string lastName, string secondName, int groupId, byte[] avatar_path)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        bool registrationSuccess = false;
        string errorMessage = string.Empty;

        try
        {
            if (!databaseManager.Connect())
            {
                errorMessage = "Ошибка подключения к базе данных.";
                yield break;
            }

            // Проверка существования email
            bool emailExists = databaseManager.CheckEmailExists(email);

            if (emailExists)
            {
                errorMessage = "Пользователь с таким email уже существует.";
                yield break;
            }

            // Регистрация нового студента с аватаром
            registrationSuccess = databaseManager.RegisterNewStudent(email, password, firstName, lastName, secondName, groupId, avatar_path);
        }
        catch (MySqlException ex)
        {
            errorMessage = "Ошибка при регистрации: " + ex.Message;
        }
        finally
        {
            databaseManager.Disconnect();
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            errorText.text = errorMessage;
        }
        else if (registrationSuccess)
        {
            errorText.text = "Регистрация успешна!";
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            errorText.text = "Ошибка при регистрации.";
        }

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    public void OnBackButtonClick()
    {
        if (avatarSelectionPanel.activeSelf)
        {
            avatarSelectionPanel.SetActive(false);
            registrationPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}