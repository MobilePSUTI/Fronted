using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FileListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text fileNameText;
    [SerializeField] private TMP_Text fileSizeText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private Button downloadButton;
    [SerializeField] private Button viewButton;

    private string fileUrl;
    private AdvancedFileUploader fileUploader;

    public void Initialize(string name, string size, string category, string url, AdvancedFileUploader uploader)
    {
        fileNameText.text = name;
        fileSizeText.text = size;
        categoryText.text = category;
        fileUrl = url;
        fileUploader = uploader;

        downloadButton.onClick.AddListener(DownloadFile);
        viewButton.onClick.AddListener(ViewFile);
    }

    private void DownloadFile()
    {
        // Реализация скачивания файла на устройство
        Application.OpenURL(fileUrl);
    }

    private void ViewFile()
    {
        StartCoroutine(fileUploader.DownloadAndViewFile(fileUrl, fileNameText.text));
    }
}