using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StudentButton : MonoBehaviour
{
    private Student studentData;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnStudentClicked);
    }

    public void SetStudentData(Student student)
    {
        studentData = student;
    }

    private void OnStudentClicked()
    {
        if (studentData != null)
        {
            UserSession.CurrentUser = studentData;
            SceneManager.LoadScene("PrSkillsStudent");
        }
        else
        {
            Debug.LogError("Данные студента не загружены");
        }
    }
}