using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GroupButtonCreator : MonoBehaviour
{
    public GameObject buttonPrefab; // ������ ������
    public Transform buttonParent; // ������������ ������ ��� ������ (��������, Panel)
    public DatabaseManager dbManager; // ������ �� ��� DatabaseManager

    void Start()
    {
        dbManager = gameObject.AddComponent<DatabaseManager>();
        // ������������� � ����������� � ���� ������
        dbManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // ������� ���� ���������
        dbManager.Connect();

        // �������� ������ ����� ��� List<Group>
        List<Group> groups = dbManager.GetAllGroups();

        // ������� ������
        CreateButtons(groups);

        // ���������� �� ���� ������
        dbManager.Disconnect();
    }

    public void CreateButtons(List<Group> groups)
    {
        foreach (Group group in groups)
        {
            GameObject button = Instantiate(buttonPrefab, buttonParent);
            button.GetComponentInChildren<Text>().text = group.Title; // ���������� Title �� ������� Group

            // �������� group.Id ��� ������� �� ������
            button.GetComponent<Button>().onClick.AddListener(() => OnGroupButtonClick(group.Title, group.Id));
        }
    }

    void OnGroupButtonClick(string groupName, int groupId)
    {
        Debug.Log("Group clicked: " + groupName);

        // ��������� ��������� group_id � UserSession
        UserSession.SelectedGroupId = groupId;

        // ��������� �� ����� �� ������� ���������
        SceneManager.LoadScene("PrListStudents");
    }
}