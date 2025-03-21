using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GroupButtonCreator : MonoBehaviour
{
    public GameObject buttonPrefab; // Префаб кнопки
    public Transform buttonParent; // Родительский объект для кнопок (например, Panel)
    public DatabaseManager dbManager; // Ссылка на ваш DatabaseManager

    void Start()
    {
        dbManager = gameObject.AddComponent<DatabaseManager>();
        // Инициализация и подключение к базе данных
        dbManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // Укажите свои параметры
        dbManager.Connect();

        // Получаем список групп как List<Group>
        List<Group> groups = dbManager.GetAllGroups();

        // Создаем кнопки
        CreateButtons(groups);

        // Отключение от базы данных
        dbManager.Disconnect();
    }

    public void CreateButtons(List<Group> groups)
    {
        foreach (Group group in groups)
        {
            GameObject button = Instantiate(buttonPrefab, buttonParent);
            button.GetComponentInChildren<Text>().text = group.Title; // Используем Title из объекта Group

            // Передаем group.Id при нажатии на кнопку
            button.GetComponent<Button>().onClick.AddListener(() => OnGroupButtonClick(group.Title, group.Id));
        }
    }

    void OnGroupButtonClick(string groupName, int groupId)
    {
        Debug.Log("Group clicked: " + groupName);

        // Сохраняем выбранный group_id в UserSession
        UserSession.SelectedGroupId = groupId;

        // Переходим на сцену со списком студентов
        SceneManager.LoadScene("PrListStudents");
    }
}