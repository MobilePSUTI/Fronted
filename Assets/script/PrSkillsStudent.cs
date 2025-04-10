using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PrSkillsStudent : MonoBehaviour
{
    public Text studentNameText;
    public Text studentGroupText;

    void Start()
    {
        if (UserSession.CurrentUser is Student student)
        {
            DisplayStudentInfo(student);
        }
        else
        {
            Debug.LogError("Данные студента не загружены");
            SceneManager.LoadScene("PrVedomosti");
        }
    }

    private void DisplayStudentInfo(Student student)
    {
        studentNameText.text = $"{student.Last} {student.First} {student.Second}";
        studentGroupText.text = $"{student.GroupName}";
    }
}