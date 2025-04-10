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

    // ��������
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

        // ������������� ������ ������ �������
        avatarSelectionPanel.SetActive(false);
        LoadAvatarOptions();
    }

    void LoadAvatarOptions()
    {
        // �������� ��������� �������� �� Resources
        avatarTextures = Resources.LoadAll<Texture2D>("Avatars");

        // ��������, ��� �������� ��������� � �������� ��� ������
        if (avatarTextures == null || avatarTextures.Length == 0)
        {
            Debug.LogError("�� ������� ��������� �������� ��������");
            return;
        }

        // ��������� ����������� ��������� ��������
        for (int i = 0; i < avatarOptions.Length; i++)
        {
            if (i < avatarTextures.Length)
            {
                // ���������, ��� �������� �������� ��� ������
                if (avatarTextures[i] == null || !avatarTextures[i].isReadable)
                {
                    Debug.LogError($"�������� ������� {i} �� �������� ��� ������");
                    continue;
                }

                int index = i;
                avatarOptions[i].sprite = Sprite.Create(avatarTextures[i],
                    new Rect(0, 0, avatarTextures[i].width, avatarTextures[i].height),
                    new Vector2(0.5f, 0.5f));

                // ��������� ���������� �����
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
            errorText.text = "������ ����������� � ���� ������.";
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            yield break;
        }

        groups = databaseManager.GetAllGroups();
        databaseManager.Disconnect();

        // ��������� placeholder ������ ���������
        groupDropdown.options.Add(new Dropdown.OptionData("�������� ������"));

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
            errorText.text = "�� ������� ��������� ������ �����.";
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
            errorText.text = "���������� ������� ������.";
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            errorText.text = "��� ������������ ���� ������ ���� ���������.";
            return;
        }
        if (groupDropdown.value == 0)
        {
            errorText.text = "���������� ������� ������.";
            return;
        }

        if (!IsValidEmail(email))
        {
            errorText.text = "������� ���������� email �����.";
            return;
        }

        // ������� � ������ ������� ������ ����������� �����������
        registrationPanel.SetActive(false);
        avatarSelectionPanel.SetActive(true);
        errorText.text = "";
    }

    public void OnConfirmAvatarButtonClick()
    {
        if (selectedAvatarIndex == -1)
        {
            errorText.text = "����������, �������� ������.";
            return;
        }

        // ���������, ��� ������ ��������� � ������� �������� ������
        if (groups == null || groups.Count == 0 || groupDropdown.value <= 0)
        {
            errorText.text = "���������� ������� ������.";
            Debug.LogError("������: ������ �� ��������� ��� �� �������");
            return;
        }

        // �������� ������ �� ����� �����
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string secondName = secondNameInput.text.Trim();

        // �������� ID ������ (���������, ��� ������ ������� - placeholder)
        int selectedGroupIndex = groupDropdown.value - 1;
        if (selectedGroupIndex < 0 || selectedGroupIndex >= groups.Count)
        {
            errorText.text = "������ ������ ������.";
            Debug.LogError($"�������� ������ ������: {selectedGroupIndex}, ����� �����: {groups.Count}");
            return;
        }

        int groupId = groups[selectedGroupIndex].Id;

        // ������������ ������ � �����
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
                errorMessage = "������ ����������� � ���� ������.";
                yield break;
            }

            // �������� ������������� email
            bool emailExists = databaseManager.CheckEmailExists(email);

            if (emailExists)
            {
                errorMessage = "������������ � ����� email ��� ����������.";
                yield break;
            }

            // ����������� ������ �������� � ��������
            registrationSuccess = databaseManager.RegisterNewStudent(email, password, firstName, lastName, secondName, groupId, avatar_path);
        }
        catch (MySqlException ex)
        {
            errorMessage = "������ ��� �����������: " + ex.Message;
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
            errorText.text = "����������� �������!";
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            errorText.text = "������ ��� �����������.";
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