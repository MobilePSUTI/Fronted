using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudentListManager : MonoBehaviour
{
    public GameObject studentPrefab; // ������ ��� ����������� �������� (��������, ������ ��� ��������� ����)
    public Transform studentParent; // ������������ ������ ��� ��������� (��������, Panel)
    public DatabaseManager dbManager; // ������ �� ��� DatabaseManager

    void Start()
    {
        dbManager = gameObject.AddComponent<DatabaseManager>();
        // ������������� � ����������� � ���� ������
        dbManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // ������� ���� ���������
        dbManager.Connect();

        // �������� ������ ��������� �� ���������� group_id
        List<Student> students = dbManager.GetStudentsByGroup(UserSession.SelectedGroupId);

        if (students != null && students.Count > 0)
        {
            // ���������� ���������
            CreateStudentList(students);
        }
        else
        {
            Debug.LogError("������ ��������� ���� ��� �� ��� ��������.");
        }

        // ���������� �� ���� ������
        dbManager.Disconnect();
    }

    public void CreateStudentList(List<Student> students)
    {
        foreach (Student student in students)
        {
            GameObject studentUI = Instantiate(studentPrefab, studentParent);

            // �������� ��������� ��������� ��� ����������� ������ ��������
            Text studentText = studentUI.GetComponentInChildren<Text>();

            // ��������� ������ � ������� ��������
            string studentInfo = $"ID: {student.Id}, ���: {student.First}, �������: {student.Last}, ��������: {student.Second}";

            // ������������� �����
            studentText.text = studentInfo;
        }
    }
}