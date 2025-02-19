using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class canvasSwift : MonoBehaviour
{
    public CanvasGroup[] pages; // ������ �������
    public Image[] buttonImages; // ������ �������� �� �������
    public Sprite[] activeSprites; // ������� ��� ��������� ��������� ������
    public Sprite[] inactiveSprites; // ������� ��� ����������� ��������� ������

    public GameObject[] topMenus; // ������ ������� ���� (0, 1, 2, 3, 4)
    public GameObject defaultTopMenu; // ������� ���� �� ��������� (��� ������� 0, 2, 3, 4)
    public GameObject specialTopMenu; // ������� ���� ��� �������� 1

    private int currentPageIndex = 0; // ������ ������� ��������
    public float fadeDuration = 1f; // ������������ ��������

    void Start()
    {
        // �������������: ���������� ������ �������� � ���������� ������ ������
        ShowPage(currentPageIndex);
        UpdateButtonSprites(currentPageIndex);
        UpdateTopMenu(currentPageIndex); // ��������� ������� ����
    }

    // ����� ��� ������������ �� ���������� ��������
    public void SwitchToPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Length || pageIndex == currentPageIndex)
            return; // �������� �� ������������ �������

        StartCoroutine(FadeOutPage(currentPageIndex)); // �������� ������� ��������
        StartCoroutine(FadeInPage(pageIndex)); // ���������� ����� ��������
        currentPageIndex = pageIndex; // ��������� ������ ������� ��������

        // ��������� �������� �� �������
        UpdateButtonSprites(pageIndex);

        // ��������� ������� ����
        UpdateTopMenu(pageIndex);
    }

    // �������� ��������
    private void ShowPage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        page.alpha = 1f;
        page.interactable = true;
        page.blocksRaycasts = true;
    }

    // ������ ��������
    private void HidePage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        page.alpha = 0f;
        page.interactable = false;
        page.blocksRaycasts = false;
    }

    // ��� �������� ������������ ��������
    private System.Collections.IEnumerator FadeOutPage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        float startAlpha = page.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            page.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }

        page.alpha = 0f;
        page.interactable = false;
        page.blocksRaycasts = false;
    }

    // ��� �������� ��������� ��������
    private System.Collections.IEnumerator FadeInPage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        float startAlpha = page.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            page.alpha = Mathf.Lerp(startAlpha, 1f, time / fadeDuration);
            yield return null;
        }

        page.alpha = 1f;
        page.interactable = true;
        page.blocksRaycasts = true;
    }

    // ���������� �������� �� �������
    private void UpdateButtonSprites(int activePageIndex)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == activePageIndex)
            {
                // ������������� �������� ������ ��� ������� ������
                buttonImages[i].sprite = activeSprites[i];
            }
            else
            {
                // ������������� ���������� ������ ��� ��������� ������
                buttonImages[i].sprite = inactiveSprites[i];
            }
        }
    }

    // ���������� �������� ����
    private void UpdateTopMenu(int pageIndex)
    {
        // ��������� ��� ������� ����
        foreach (var menu in topMenus)
        {
            if (menu != null)
                menu.SetActive(false);
        }

        // �������� ������ ������� ����
        if (pageIndex == 1)
        {
            if (specialTopMenu != null)
                specialTopMenu.SetActive(true);
        }
        else
        {
            if (defaultTopMenu != null)
                defaultTopMenu.SetActive(true);
        }
    }
}

