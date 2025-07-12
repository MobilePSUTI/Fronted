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
    [SerializeField] private string studentId = "student_123"; // �������� �� �������� ID

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
        // ������������� ������ ���������
        foreach (Button btn in categoryButtons)
        {
            string category = btn.name.Replace("Button_", "");
            btn.onClick.AddListener(() => OpenFilePanel(category));
        }

        // ���������� ������������ ������
        selectFileButton.onClick.AddListener(OpenFilePicker);
        uploadButton.onClick.AddListener(UploadFile);
        viewFilesButton.onClick.AddListener(ShowUploadedFiles);
        backButton.onClick.AddListener(ReturnToMainMenu);

        // ��������� ��������� UI
        filePanel.SetActive(false);
    }

    public void OpenFilePanel(string category)
    {
        currentFileCategory = category;
        mainPanel.SetActive(false);
        filePanel.SetActive(true);
        ClearFileSelection();
        UpdateStatus($"������� ���������: {category}");
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
        string path = UnityEditor.EditorUtility.OpenFilePanel("�������� ����", "", "*");
        if (!string.IsNullOrEmpty(path)) ProcessSelectedFile(path);
#elif UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.PickFile((path) => {
            if (!string.IsNullOrEmpty(path)) ProcessSelectedFile(path);
            else UpdateStatus("����� ����� �������");
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
                UpdateStatus("������: ���� ������� ������� (����. 25MB)");
                return;
            }

            currentFilePath = path;
            currentFileName = fileInfo.Name;
            currentFileSize = fileInfo.Length;

            UpdateFileInfoUI();
            UpdateStatus($"���� ������: {currentFileName}");
        }
        catch (System.Exception e)
        {
            UpdateStatus($"������: {e.Message}");
        }
    }

    public void UploadFile()
    {
        if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
        {
            UpdateStatus("������� �������� ����");
            return;
        }

        StartCoroutine(UploadFileCoroutine());
    }

    private IEnumerator UploadFileCoroutine()
    {
        UpdateStatus("�������� �����...");

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(currentFilePath);
        }
        catch (System.Exception e)
        {
            UpdateStatus($"������ ������ �����: {e.Message}");
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
                UpdateStatus($"������: {www.error}");
                yield break;
            }

            try
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response.success)
                {
                    UpdateStatus("���� ������� ��������!");
                    ClearFileSelection();
                }
                else
                {
                    UpdateStatus($"������ �������: {response.message}");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"������ ��������� ������: {e.Message}");
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
        UpdateStatus("�������� ������ ������...");

        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverBaseURL}/get_files", form))
        {
            www.timeout = (int)requestTimeout;
            yield return www.SendWebRequest();


            if (www.result != UnityWebRequest.Result.Success)
            {
                UpdateStatus($"������: {www.error}");
                yield break;
            }

            try
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response.success && response.files != null)
                {
                    UpdateStatus($"������� ������: {response.files.Count}");
                    PopulateFilesList(response.files);
                }
                else
                {
                    UpdateStatus($"������: {response.message}");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"������ ��������� ������: {e.Message}");
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
            UpdateStatus("�������� ��������� �����");
            yield break;
        }

        UpdateStatus($"�������� {filename}...");

        // ���� 1: �������� ����� � �������
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = (int)requestTimeout;

        // ���������� ������ ��� ����� try-catch
        yield return www.SendWebRequest();


        // ��������� ��������� �������
        if (www.result != UnityWebRequest.Result.Success)
        {
            UpdateStatus($"������ ��������: {www.error}");
            www.Dispose();
            yield break;
        }

        // ���� 2: ���������� �����
        string savePath = Path.Combine(Application.persistentDataPath, filename);
        bool fileSaved = false;

        try
        {
            File.WriteAllBytes(savePath, www.downloadHandler.data);
            fileSaved = true;
        }
        catch (System.Exception e)
        {
            UpdateStatus($"������ ����������: {e.Message}");
        }
        finally
        {
            www.Dispose();
        }

        if (!fileSaved)
        {
            yield break;
        }

        // ��������� ���������� � �����
        currentFilePath = savePath;
        currentFileName = filename;
        currentFileSize = new FileInfo(savePath).Length;
        UpdateFileInfoUI();

        // ���� 3: ����������� �����
        yield return StartCoroutine(DisplayFileContent());
    }

    private IEnumerator DisplayFileContent()
    {
        if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
        {
            UpdateStatus("���� �� ������");
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
                    UpdateStatus("����������� ���������");
                }
                else
                {
                    UpdateStatus($"������: {www.error}");
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
                UpdateStatus("���� ��������");
            }
            catch (System.Exception e)
            {
                textPreviewContent.text = $"������ ������: {e.Message}";
                UpdateStatus("������ ������ �����");
            }
        }
        else
        {
            filePreview.gameObject.SetActive(false);
            textPreviewScroll.gameObject.SetActive(true);
            textPreviewContent.text = $"����: {currentFileName}\n������: {FormatFileSize(currentFileSize)}\n\n��� ��������� �������� ����";
            UpdateStatus("���������� � �����");
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