using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudentListManager : MonoBehaviour
{
    public GameObject studentPrefab; // Префаб для отображения студента (например, кнопка или текстовое поле)
    public Transform studentParent; // Родительский объект для студентов (например, Panel)
    public DatabaseManager dbManager; // Ссылка на ваш DatabaseManager

    void Start()
    {
        dbManager = gameObject.AddComponent<DatabaseManager>();
        // Инициализация и подключение к базе данных
        dbManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // Укажите свои параметры
        dbManager.Connect();

        // Получаем список студентов по выбранному group_id
        List<Student> students = dbManager.GetStudentsByGroup(UserSession.SelectedGroupId);

        if (students != null && students.Count > 0)
        {
            // Отображаем студентов
            CreateStudentList(students);
        }
        else
        {
            Debug.LogError("Список студентов пуст или не был загружен.");
        }

        // Отключение от базы данных
        dbManager.Disconnect();
    }

    public void CreateStudentList(List<Student> students)
    {
        foreach (Student student in students)
        {
            GameObject studentUI = Instantiate(studentPrefab, studentParent);

            // Получаем текстовый компонент для отображения данных студента
            Text studentText = studentUI.GetComponentInChildren<Text>();

            // Формируем строку с данными студента
            string studentInfo = $"ID: {student.Id}, Имя: {student.First}, Фамилия: {student.Last}, Отчество: {student.Second}";

            // Устанавливаем текст
            studentText.text = studentInfo;
        }
    }
}