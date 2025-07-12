using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

#if UNITY_ANDROID || UNITY_IOS
using NativeFilePickerNamespace;
#endif

[System.Serializable]
public class ServerFileInfo
{
    public string name;
    public string size;
    public string category;
    public string url;
}

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string message;
    public List<ServerFileInfo> files;
}

public class AdvancedFileUploader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject filePanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private Button selectFileButton;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button viewFilesButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text fileNameText;
    [SerializeField] private TMP_Text fileSizeText;
    [SerializeField] private RawImage filePreview;
    [SerializeField] private ScrollRect textPreviewScroll;
    [SerializeField] private TMP_Text textPreviewContent;
    [SerializeField] private Transform filesListContainer;
    [SerializeField] private GameObject fileItemPrefab;

    [Header("Server Settings")]
    [SerializeField] private string serverBaseURL = "https://yourserver.com/api";
    [SerializeField] private float requestTimeout = 30f;
    [SerializeField] private string studentId = "student_123"; // Замените на реальный ID

    private string currentFilePath = "";
    private string currentFileName = "";
    private string currentFileCategory = "";
    private long currentFileSize = 0;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Инициализация кнопок категорий
        foreach (Button btn in categoryButtons)
        {
            string category = btn.name.Replace("Button_", "");
            btn.onClick.AddListener(() => OpenFilePanel(category));
        }

        // Назначение обработчиков кнопок
        selectFileButton.onClick.AddListener(OpenFilePicker);
        uploadButton.onClick.AddListener(UploadFile);
        viewFilesButton.onClick.AddListener(ShowUploadedFiles);
        backButton.onClick.AddListener(ReturnToMainMenu);

        // Начальное состояние UI
        filePanel.SetActive(false);
    }

    public void OpenFilePanel(string category)
    {
        currentFileCategory = category;
        mainPanel.SetActive(false);
        filePanel.SetActive(true);
        ClearFileSelection();
        UpdateStatus($"Выбрана категория: {category}");
    }

    public void ReturnToMainMenu()
    {
        filePanel.SetActive(false);
        mainPanel.SetActive(true);
        ClearFileSelection();
    }

    public void OpenFilePicker()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Выберите файл", "", "*");
        if (!string.IsNullOrEmpty(path)) ProcessSelectedFile(path);
#elif UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.PickFile((path) => {
            if (!string.IsNullOrEmpty(path)) ProcessSelectedFile(path);
            else UpdateStatus("Выбор файла отменен");
        }, new string[] { "*/*" });
#endif
    }

    private void ProcessSelectedFile(string path)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Length > 25 * 1024 * 1024)
            {
                UpdateStatus("Ошибка: Файл слишком большой (макс. 25MB)");
                return;
            }

            currentFilePath = path;
            currentFileName = fileInfo.Name;
            currentFileSize = fileInfo.Length;

            UpdateFileInfoUI();
            UpdateStatus($"Файл выбран: {currentFileName}");
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Ошибка: {e.Message}");
        }
    }

    public void UploadFile()
    {
        if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
        {
            UpdateStatus("Сначала выберите файл");
            return;
        }

        StartCoroutine(UploadFileCoroutine());
    }

    private IEnumerator UploadFileCoroutine()
    {
        UpdateStatus("Загрузка файла...");

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(currentFilePath);
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Ошибка чтения файла: {e.Message}");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("category", currentFileCategory);
        form.AddField("student_id", studentId);
        form.AddBinaryData("file", fileData, currentFileName);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverBaseURL}/upload", form))
        {
            www.timeout = (int)requestTimeout;
            yield return www.SendWebRequest();


            if (www.result != UnityWebRequest.Result.Success)
            {
                UpdateStatus($"Ошибка: {www.error}");
                yield break;
            }

            try
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response.success)
                {
                    UpdateStatus("Файл успешно загружен!");
                    ClearFileSelection();
                }
                else
                {
                    UpdateStatus($"Ошибка сервера: {response.message}");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Ошибка обработки ответа: {e.Message}");
            }
        }
    }

    public void ShowUploadedFiles()
    {
        StartCoroutine(FetchUploadedFiles());
    }

    private IEnumerator FetchUploadedFiles()
    {
        ClearFilesList();
        UpdateStatus("Загрузка списка файлов...");

        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverBaseURL}/get_files", form))
        {
            www.timeout = (int)requestTimeout;
            yield return www.SendWebRequest();


            if (www.result != UnityWebRequest.Result.Success)
            {
                UpdateStatus($"Ошибка: {www.error}");
                yield break;
            }

            try
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response.success && response.files != null)
                {
                    UpdateStatus($"Найдено файлов: {response.files.Count}");
                    PopulateFilesList(response.files);
                }
                else
                {
                    UpdateStatus($"Ошибка: {response.message}");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Ошибка обработки ответа: {e.Message}");
            }
        }
    }

    private void PopulateFilesList(List<ServerFileInfo> files)
    {
        foreach (ServerFileInfo file in files)
        {
            if (file == null) continue;

            GameObject fileItem = Instantiate(fileItemPrefab, filesListContainer);
            FileListItem item = fileItem.GetComponent<FileListItem>();

            if (item != null)
            {
                item.Initialize(
                    file.name,
                    file.size,
                    file.category,
                    file.url,
                    this
                );
            }
        }
    }

    public IEnumerator DownloadAndViewFile(string url, string filename)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filename))
        {
            UpdateStatus("Неверные параметры файла");
            yield break;
        }

        UpdateStatus($"Загрузка {filename}...");

        // Этап 1: Загрузка файла с сервера
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = (int)requestTimeout;

        // Отправляем запрос вне блока try-catch
        yield return www.SendWebRequest();


        // Проверяем результат запроса
        if (www.result != UnityWebRequest.Result.Success)
        {
            UpdateStatus($"Ошибка загрузки: {www.error}");
            www.Dispose();
            yield break;
        }

        // Этап 2: Сохранение файла
        string savePath = Path.Combine(Application.persistentDataPath, filename);
        bool fileSaved = false;

        try
        {
            File.WriteAllBytes(savePath, www.downloadHandler.data);
            fileSaved = true;
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Ошибка сохранения: {e.Message}");
        }
        finally
        {
            www.Dispose();
        }

        if (!fileSaved)
        {
            yield break;
        }

        // Обновляем информацию о файле
        currentFilePath = savePath;
        currentFileName = filename;
        currentFileSize = new FileInfo(savePath).Length;
        UpdateFileInfoUI();

        // Этап 3: Отображение файла
        yield return StartCoroutine(DisplayFileContent());
    }

    private IEnumerator DisplayFileContent()
    {
        if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
        {
            UpdateStatus("Файл не найден");
            yield break;
        }

        string extension = Path.GetExtension(currentFilePath).ToLower();

        if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
        {
            filePreview.gameObject.SetActive(true);
            textPreviewScroll.gameObject.SetActive(false);

            string fileUrl = "file://" + currentFilePath;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fileUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    if (filePreview.texture != null)
                        Destroy(filePreview.texture);

                    filePreview.texture = DownloadHandlerTexture.GetContent(www);
                    UpdateStatus("Изображение загружено");
                }
                else
                {
                    UpdateStatus($"Ошибка: {www.error}");
                }
            }
        }
        else if (extension == ".txt")
        {
            filePreview.gameObject.SetActive(false);
            textPreviewScroll.gameObject.SetActive(true);

            try
            {
                textPreviewContent.text = File.ReadAllText(currentFilePath);
                UpdateStatus("Файл загружен");
            }
            catch (System.Exception e)
            {
                textPreviewContent.text = $"Ошибка чтения: {e.Message}";
                UpdateStatus("Ошибка чтения файла");
            }
        }
        else
        {
            filePreview.gameObject.SetActive(false);
            textPreviewScroll.gameObject.SetActive(true);
            textPreviewContent.text = $"Файл: {currentFileName}\nРазмер: {FormatFileSize(currentFileSize)}\n\nДля просмотра скачайте файл";
            UpdateStatus("Информация о файле");
        }
    }

    private void UpdateFileInfoUI()
    {
        fileNameText.text = currentFileName;
        fileSizeText.text = FormatFileSize(currentFileSize);
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[FileUploader] {message}");
    }

    private void ClearFileSelection()
    {
        currentFilePath = "";
        currentFileName = "";
        currentFileSize = 0;
        UpdateFileInfoUI();
    }

    private void ClearFilesList()
    {
        if (filesListContainer == null) return;

        foreach (Transform child in filesListContainer)
        {
            if (child != null && child.gameObject != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}