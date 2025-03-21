using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
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
        if (UserSession.CurrentStudent != null)
        {
            Debug.Log($"������� ������������: {UserSession.CurrentStudent.Name}");
        }
    }

    public void OnLoginButtonClick()
    {
        string login = loginInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            errorText.text = "����� � ������ �� ����� ���� �������.";
            return;
        }

        StartCoroutine(LoginStudent(login, password));
    }

    IEnumerator LoginStudent(string login, string password)
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

        Debug.Log("�������� ������ � ������...");

        // ��������������� ��������
        Student student;
        if (databaseManager.AuthenticateStudent(login, password, out student))
        {
            // ��������� ������ � �������� � ����������� ������
            UserSession.CurrentStudent = student;
            Debug.Log($"������� {student.Name} ������� �����������.");
            errorText.text = "";

            // ��������� ������� � ���
            yield return StartCoroutine(GetNewsFromVK());

            // ��������� �� ����� � ���������
            yield return StartCoroutine(LoadNewsSceneAsync());
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

    IEnumerator LoadNewsSceneAsync()
    {
        // ����������� �������� ����� � ���������
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StudentsScene");

        // ���� ���������� ��������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void OnNewsButtonClick()
    {
        if (UserSession.CurrentStudent == null)
        {
            errorText.text = "������� ������� � �������.";
            return;
        }

        StartCoroutine(LoadNewsBeforeTransition());
    }

    IEnumerator LoadNewsBeforeTransition()
    {
        // ���������� ��������� ��������
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // ��������� ������� �� ���������
        yield return StartCoroutine(GetNewsFromVK());

        // ��������� �� ����� � ���������
        yield return StartCoroutine(LoadNewsSceneAsync());

        // �������� ��������� ��������
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
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