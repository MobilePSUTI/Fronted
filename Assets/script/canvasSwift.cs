using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class canvasSwift : MonoBehaviour
{
    public CanvasGroup[] pages; // Массив страниц
    public Image[] buttonImages; // Массив картинок на кнопках
    public Sprite[] activeSprites; // Спрайты для активного состояния кнопок
    public Sprite[] inactiveSprites; // Спрайты для неактивного состояния кнопок

    public GameObject[] topMenus; // Массив верхних меню (0, 1, 2, 3, 4)
    public GameObject defaultTopMenu; // Верхнее меню по умолчанию (для страниц 0, 2, 3, 4)
    public GameObject specialTopMenu; // Верхнее меню для страницы 1

    private int currentPageIndex = 0; // Индекс текущей страницы
    public float fadeDuration = 1f; // Длительность перехода

    void Start()
    {
        // Инициализация: показываем первую страницу и активируем первую кнопку
        ShowPage(currentPageIndex);
        UpdateButtonSprites(currentPageIndex);
        UpdateTopMenu(currentPageIndex); // Обновляем верхнее меню
    }

    // Метод для переключения на конкретную страницу
    public void SwitchToPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Length || pageIndex == currentPageIndex)
            return; // Проверка на корректность индекса

        StartCoroutine(FadeOutPage(currentPageIndex)); // Скрываем текущую страницу
        StartCoroutine(FadeInPage(pageIndex)); // Показываем новую страницу
        currentPageIndex = pageIndex; // Обновляем индекс текущей страницы

        // Обновляем картинки на кнопках
        UpdateButtonSprites(pageIndex);

        // Обновляем верхнее меню
        UpdateTopMenu(pageIndex);
    }

    // Показать страницу
    private void ShowPage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        page.alpha = 1f;
        page.interactable = true;
        page.blocksRaycasts = true;
    }

    // Скрыть страницу
    private void HidePage(int pageIndex)
    {
        CanvasGroup page = pages[pageIndex];
        page.alpha = 0f;
        page.interactable = false;
        page.blocksRaycasts = false;
    }

    // для плавного исчезновения страницы
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

    // для плавного появления страницы
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

    // Обновление картинок на кнопках
    private void UpdateButtonSprites(int activePageIndex)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == activePageIndex)
            {
                // Устанавливаем активный спрайт для текущей кнопки
                buttonImages[i].sprite = activeSprites[i];
            }
            else
            {
                // Устанавливаем неактивный спрайт для остальных кнопок
                buttonImages[i].sprite = inactiveSprites[i];
            }
        }
    }

    // Обновление верхнего меню
    private void UpdateTopMenu(int pageIndex)
    {
        // Отключаем все верхние меню
        foreach (var menu in topMenus)
        {
            if (menu != null)
                menu.SetActive(false);
        }

        // Включаем нужное верхнее меню
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

