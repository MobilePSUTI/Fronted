using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject settingsPanelPrefab; // Префаб панели настроек
    public bool keepActiveBetweenScenes = true; // Оставлять панель открытой при смене сцены?

    private GameObject _currentPanel;
    private Canvas _currentCanvas;
    private bool _isPanelOpen = false;

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожаем при загрузке новой сцены

            SceneManager.sceneLoaded += OnSceneLoaded; // Подписываемся на событие загрузки сцены
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
            return;
        }
    }

    // Вызывается при загрузке новой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ищем Canvas в новой сцене
        _currentCanvas = FindObjectOfType<Canvas>();

        if (_currentPanel != null)
        {
            // Переносим панель на новый Canvas
            _currentPanel.transform.SetParent(_currentCanvas.transform, false);
            ResetPanelTransform();

            // Восстанавливаем состояние (открыта/закрыта)
            _currentPanel.SetActive(_isPanelOpen);
        }
    }

    // Открыть/закрыть панель
    public void ToggleSettingsPanel()
    {
        if (_currentPanel == null && _currentCanvas != null)
        {
            // Создаем панель, если её нет
            _currentPanel = Instantiate(settingsPanelPrefab, _currentCanvas.transform);
            ResetPanelTransform();

            // Настраиваем кнопку "Назад"
            var backButton = _currentPanel.GetComponentInChildren<Button>(true);
            if (backButton != null)
                backButton.onClick.AddListener(CloseSettingsPanel);
        }

        _isPanelOpen = !_isPanelOpen;
        if (_currentPanel != null)
            _currentPanel.SetActive(_isPanelOpen);
    }

    // Закрыть панель
    public void CloseSettingsPanel()
    {
        _isPanelOpen = false;
        if (_currentPanel != null)
            _currentPanel.SetActive(false);
    }

    // Сброс трансформа панели (чтобы растягивалась на весь экран)
    private void ResetPanelTransform()
    {
        if (_currentPanel == null) return;

        RectTransform rt = _currentPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}