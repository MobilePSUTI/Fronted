using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PrMenuModel : MonoBehaviour
{
    public GameObject loadingIndicator; // ��������� ��������
    public InputField loginInput; // ���� ��� ����� ������
    public InputField passwordInput; // ���� ��� ����� ������
    public Text errorText; // ����� ��� ����������� ������

    private DatabaseManager databaseManager;

    void Start()
    {
        databaseManager = gameObject.AddComponent<DatabaseManager>();
        databaseManager.Initialize("localhost", "psuti_app", "developer", "developer_password"); // ������� ���� ���������

        // ��������������� ������ � ������� ������������
        if (UserSession.CurrentTeacher != null)
        {
            Debug.Log($"������� �������������: {UserSession.CurrentTeacher.Name}");
        }
    }

    // ����� ��� ����� �������������
    public void OnTeacherLoginButtonClick()
    {
        string login = loginInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            errorText.text = "����� � ������ �� ����� ���� �������.";
            return;
        }

        StartCoroutine(LoginTeacher(login, password));
    }

    IEnumerator LoginTeacher(string login, string password)
    {
        // ���������� ��������� ��������
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        Debug.Log("������� ����������� � ���� ������...");

        // ������������ � ���� ������
        if (!databaseManager.Connect())
        {
            Debug.Log("������ ����������� � ���� ������.");
            errorText.text = "������ ����������� � ���� ������.";
            yield break;
        }

        Debug.Log("�������� ������ � ������ �������������...");

        // ��������������� �������������
        Teacher teacher;
        if (databaseManager.AuthenticateTeacher(login, password, out teacher))
        {
            // ��������� ������ � ������������� � ����������� ������
            UserSession.CurrentTeacher = teacher;
            List<Group> groups = databaseManager.GetAllGroups(); // ������ ���� ���������
            Debug.Log($"������������� {teacher.Name} ������� �����������.");
            errorText.text = "";

            // ��������� ������� � ���
            yield return StartCoroutine(GetNewsFromVK());

            // ��������� �� ����� �������������
            yield return StartCoroutine(LoadTeacherSceneAsync());
        }
        else
        {
            Debug.Log("�������� ����� ��� ������.");
            errorText.text = "�������� ����� ��� ������.";
        }

        // �������� ��������� ��������
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    IEnumerator LoadTeacherSceneAsync()
    {
        // ����������� �������� ����� �������������
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PrepodModel");

        // ���� ���������� ��������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator GetNewsFromVK()
    {
        // ���������� VKNewsLoad ��� �������� ��������
        var vkNewsLoad = gameObject.AddComponent<VKNewsLoad>();
        yield return StartCoroutine(vkNewsLoad.GetNewsFromVK(0, 100)); // ��������� ��� �������

        // ��������� ������ � ���
        NewsDataCache.CachedPosts = vkNewsLoad.allPosts;
        NewsDataCache.CachedVKGroups = vkNewsLoad.groupDictionary;

        // ���������� ��������� ���������
        Destroy(vkNewsLoad);
    }
}