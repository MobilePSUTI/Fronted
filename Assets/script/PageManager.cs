using UnityEngine;

public class PageManager : MonoBehaviour
{
    // Панели (страницы)
    public GameObject mainPanel;
    public GameObject mainStudentPanel;
    public GameObject mainPrepodovatelPanel;

    // Метод для открытия Main
    public void ShowMainPanel()
    {
        mainStudentPanel.SetActive(false);
        mainPrepodovatelPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // Метод для открытия студента
    public void ShowMainStudentPanel()
    {
        mainStudentPanel.SetActive(true);
        mainPrepodovatelPanel.SetActive(false);
        mainPanel.SetActive(false);
    }

    // Метод для открытия преподавателя
    public void ShowMainPrepodovatelPanel()
    {
        mainStudentPanel.SetActive(false);
        mainPrepodovatelPanel.SetActive(true);
        mainPanel.SetActive(false);
    }


}