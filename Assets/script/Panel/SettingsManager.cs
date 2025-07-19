using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject settingsPanelPrefab; // ������ ������ ��������
    public bool keepActiveBetweenScenes = true; // ��������� ������ �������� ��� ����� �����?

    private GameObject _currentPanel;
    private Canvas _currentCanvas;
    private bool _isPanelOpen = false;

    private void Awake()
    {
        // ���������� Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ���������� ��� �������� ����� �����

            SceneManager.sceneLoaded += OnSceneLoaded; // ������������� �� ������� �������� �����
        }
        else
        {
            Destroy(gameObject); // ���������� ���������
            return;
        }
    }

    // ���������� ��� �������� ����� �����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� Canvas � ����� �����
        _currentCanvas = FindObjectOfType<Canvas>();

        if (_currentPanel != null)
        {
            // ��������� ������ �� ����� Canvas
            _currentPanel.transform.SetParent(_currentCanvas.transform, false);
            ResetPanelTransform();

            // ��������������� ��������� (�������/�������)
            _currentPanel.SetActive(_isPanelOpen);
        }
    }

    // �������/������� ������
    public void ToggleSettingsPanel()
    {
        if (_currentPanel == null && _currentCanvas != null)
        {
            // ������� ������, ���� � ���
            _currentPanel = Instantiate(settingsPanelPrefab, _currentCanvas.transform);
            ResetPanelTransform();

            // ����������� ������ "�����"
            var backButton = _currentPanel.GetComponentInChildren<Button>(true);
            if (backButton != null)
                backButton.onClick.AddListener(CloseSettingsPanel);
        }

        _isPanelOpen = !_isPanelOpen;
        if (_currentPanel != null)
            _currentPanel.SetActive(_isPanelOpen);
    }

    // ������� ������
    public void CloseSettingsPanel()
    {
        _isPanelOpen = false;
        if (_currentPanel != null)
            _currentPanel.SetActive(false);
    }

    // ����� ���������� ������ (����� ������������� �� ���� �����)
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
        // ������������ �� ������� ��� �����������
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}