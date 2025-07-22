using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour
{
    public void OnLogoutClicked()
    {
        // ������� ����������� ������ ������ (��������, ����� �����������)
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.Save();

        // ��������� ����� ����� (�������� "LoginScene" �� ��� ����� �����)
        SceneManager.LoadScene("MainAuth");
    }
}
