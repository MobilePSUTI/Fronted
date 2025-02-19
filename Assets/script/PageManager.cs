using UnityEngine;

public class PageManager : MonoBehaviour
{
    // ������ (��������)
    public GameObject mainPanel;
    public GameObject mainStudentPanel;
    public GameObject mainPrepodovatelPanel;

    // ����� ��� �������� Main
    public void ShowMainPanel()
    {
        mainStudentPanel.SetActive(false);
        mainPrepodovatelPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ����� ��� �������� ��������
    public void ShowMainStudentPanel()
    {
        mainStudentPanel.SetActive(true);
        mainPrepodovatelPanel.SetActive(false);
        mainPanel.SetActive(false);
    }

    // ����� ��� �������� �������������
    public void ShowMainPrepodovatelPanel()
    {
        mainStudentPanel.SetActive(false);
        mainPrepodovatelPanel.SetActive(true);
        mainPanel.SetActive(false);
    }


}