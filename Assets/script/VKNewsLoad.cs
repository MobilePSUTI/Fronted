using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class VKResponse
{
    public Response response;
}

[System.Serializable]
public class Response
{
    public List<Post> items;
    public List<Group> groups;
}

[System.Serializable]
public class Post
{
    public string text;
    public long date;
    public List<Attachment> attachments;
    public long owner_id;
}

[System.Serializable]
public class Attachment
{
    public string type;
    public Photo photo;
}

[System.Serializable]
public class Photo
{
    public List<PhotoSize> sizes;
}

[System.Serializable]
public class PhotoSize
{
    public string type;
    public string url;
    public int width;
    public int height;
}

[System.Serializable]
public class Group
{
    public long id;
    public string name;
    public string screen_name;
    public int is_closed;
    public string type;
    public string photo_50;
    public string photo_100;
    public string photo_200;
}

public class VKNewsLoad : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public string accessToken = "2e1a194f2e1a194f2e1a194f0d2d3078ac22e1a2e1a194f49ac02415f6ad1570cce36f8";
    public int groupId = 17785357;
    public Transform newsContainer; // ��� ������ ���� Content ������ ScrollView
    public ScrollRect scrollRect; // ������ �� ScrollRect

    // ��������� ���������� ����������
    public float scrollSpeed = 1f;
    public float elasticity = 0.1f;
    public float decelerationRate = 0.95f; // �������� ���������� �������

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isDragging = false;
    private Vector2 velocity;

    void Start()
    {
        StartCoroutine(GetNewsFromVK(groupId));

        // ������������� ���������� ����������
        if (scrollRect != null && newsContainer != null)
        {
            startPosition = (newsContainer as RectTransform).anchoredPosition;
            targetPosition = startPosition;
        }
    }

    void Update()
    {
        if (!isDragging)
        {
            // ��������� �������
            if (velocity.magnitude > 0.1f)
            {
                (newsContainer as RectTransform).anchoredPosition += velocity * Time.deltaTime;
                velocity *= decelerationRate;
            }
            else
            {
                // ������� ����������� � ������� �������
                (newsContainer as RectTransform).anchoredPosition = Vector2.Lerp((newsContainer as RectTransform).anchoredPosition, targetPosition, elasticity);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        Vector2 delta = eventData.delta;
        (newsContainer as RectTransform).anchoredPosition += new Vector2(0, delta.y * scrollSpeed);
        velocity = delta * scrollSpeed;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float minY = startPosition.y;
        float maxY = startPosition.y + ((newsContainer as RectTransform).rect.height - (scrollRect.viewport as RectTransform).rect.height);

        targetPosition = (newsContainer as RectTransform).anchoredPosition;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
    }

    IEnumerator GetNewsFromVK(int groupId)
    {
        string encodedAccessToken = UnityWebRequest.EscapeURL(accessToken);
        string url = $"https://api.vk.com/method/wall.get?owner_id=-{groupId}&access_token={encodedAccessToken}&v=5.199&count=100&extended=1";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("������: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("JSON �����: " + jsonResponse);
            ProcessNews(jsonResponse);
        }
    }

    void ProcessNews(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON ����� ���� ��� ����� null.");
            return;
        }

        var response = JsonUtility.FromJson<VKResponse>(json);

        if (response == null || response.response == null || response.response.items == null || response.response.groups == null)
        {
            Debug.LogError("�������� JSON ����� ��� ������ ������.");
            return;
        }

        // ��������� ������� �� ����� Resources
        GameObject newsItemWithPhotoPrefab = Resources.Load<GameObject>("Panel_news");
        GameObject newsItemWithoutPhotoPrefab = Resources.Load<GameObject>("Panel_news_netPhoto");

        if (newsItemWithPhotoPrefab == null || newsItemWithoutPhotoPrefab == null)
        {
            Debug.LogError("������� �� ������� � ����� Resources.");
            return;
        }

        foreach (var post in response.response.items)
        {
            if (post != null)
            {
                // ���������, ���� �� ����� ��� �����������
                bool hasText = !string.IsNullOrEmpty(post.text);
                bool hasImage = post.attachments != null && post.attachments.Any(a => a.type == "photo" && a.photo != null && a.photo.sizes != null);

                // ���� ��� �� ������, �� �����������, ���������� ���� ����
                if (!hasText && !hasImage)
                {
                    Debug.LogWarning($"���� � ID = {post.owner_id} ��������: ��� ������ � �����������.");
                    continue;
                }

                var group = response.response.groups.FirstOrDefault(g => g.id == -post.owner_id);

                if (group == null)
                {
                    Debug.LogWarning($"���������� � ID = {Math.Abs(post.owner_id)} �� �������.");
                    continue; // ���������� ���� ����, ���� ���������� �� �������
                }

                // �������� ������ � ����������� �� ������� �����������
                GameObject newsItemPrefab = hasImage
                    ? newsItemWithPhotoPrefab // ������ � ������������
                    : newsItemWithoutPhotoPrefab; // ������ ��� �����������

                // ������ ����� ������� �������
                GameObject newsItem = Instantiate(newsItemPrefab, newsContainer);

                // ������� ���������� ������ �������
                var nameGroup = newsItem.transform.Find("Name_group")?.GetComponent<Text>();
                var dateTimeText = newsItem.transform.Find("DateTimeText")?.GetComponent<Text>();
                var newsText = newsItem.transform.Find("Text (Legacy)")?.GetComponent<Text>();
                var fotoGroup = newsItem.transform.Find("foto_group")?.GetComponent<RawImage>();
                var foto = newsItem.transform.Find("Foto")?.GetComponent<RawImage>();

                if (nameGroup == null)
                {
                    Debug.LogError("Name_group �� ������ � �������.");
                    continue; // ���������� ���� ����, ���� ��������� �� ������
                }
                if (dateTimeText == null)
                {
                    Debug.LogError("DateTimeText �� ������ � �������.");
                    continue;
                }
                if (newsText == null)
                {
                    Debug.LogError("Text (Legacy) �� ������ � �������.");
                    continue;
                }

                // ������������� ������ ����������
                nameGroup.text = group.name; // ���������� ���� name �� JSON
                StartCoroutine(LoadGroupImage(group.photo_200, fotoGroup));

                // ������������� ����� ����� (���� ����)
                newsText.text = hasText ? post.text : "";

                // ������������� ���� �����
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(post.date).ToLocalTime();
                dateTimeText.text = dateTime.ToString("dd.MM.yyyy HH:mm");

                // ���� ���� �����������, ��������� ���
                if (hasImage)
                {
                    var photoSize = post.attachments.First(a => a.type == "photo").photo.sizes.FirstOrDefault(s => s.type == "x");
                    if (photoSize != null && !string.IsNullOrEmpty(photoSize.url))
                    {
                        StartCoroutine(LoadImage(photoSize.url, foto));
                    }
                }
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(newsContainer as RectTransform);
        // ��������� ScrollRect
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1; // �������� ��������� ������
        }
    }

    IEnumerator LoadGroupImage(string url, RawImage targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("������: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            targetImage.texture = texture;
        }
    }

    IEnumerator LoadImage(string url, RawImage targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("������: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            targetImage.texture = texture;
        }
    }
}