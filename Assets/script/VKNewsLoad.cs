using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VKResponse
{
    public Response response;
}

[System.Serializable]
public class Response
{
    public List<Post> items;
}

[System.Serializable]
public class Post
{
    public string text;
    public long date;
    public List<Attachment> attachments;
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

public class VKNewsLoad : MonoBehaviour
{
    public string accessToken = "2e1a194f2e1a194f2e1a194f0d2d3078ac22e1a2e1a194f49ac02415f6ad1570cce36f8";
    public int groupId = 17785357; // ID сообщества
    public GameObject newsItemPrefab; // Префаб для элемента новости
    public Transform newsContainer; // Контейнер для новостей

    void Start()
    {
        StartCoroutine(GetNewsFromVK(groupId));
    }

    IEnumerator GetNewsFromVK(int groupId)
    {
        string encodedAccessToken = UnityWebRequest.EscapeURL(accessToken);
        string url = $"https://api.vk.com/method/wall.get?owner_id=-{groupId}&access_token={encodedAccessToken}&domain=itclub_psuti&v=5.131";

        Debug.Log("Request URL: " + url); // Логируем URL

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("JSON Response: " + jsonResponse); // Логируем JSON
            ProcessNews(jsonResponse);
        }
    }

    void ProcessNews(string json)
    {
        var response = JsonUtility.FromJson<VKResponse>(json);

        if (response == null || response.response == null || response.response.items == null)
        {
            Debug.LogError("Invalid JSON response or empty data.");
            return;
        }

        foreach (var post in response.response.items)
        {
            if (post != null)
            {
                CreateNewsItem(post.text, post.date, post.attachments);
            }
        }
    }

    void CreateNewsItem(string text, long date, List<Attachment> attachments)
    {
        GameObject newsItem = Instantiate(newsItemPrefab, newsContainer);
        newsItem.GetComponentInChildren<Text>().text = text;

        // Преобразуем Unix timestamp в дату и время
        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(date).ToLocalTime();
        newsItem.transform.Find("DateTimeText").GetComponent<Text>().text = dateTime.ToString("dd.MM.yyyy HH:mm");

        // Проверка наличия вложений и изображений
        if (attachments != null && attachments.Any(a => a.type == "photo" && a.photo != null && a.photo.sizes != null))
        {
            // Ищем размер "x" (604px в высоту)
            var photoSize = attachments.First(a => a.type == "photo").photo.sizes.FirstOrDefault(s => s.type == "x");
            if (photoSize != null && !string.IsNullOrEmpty(photoSize.url))
            {
                StartCoroutine(LoadImage(photoSize.url, newsItem));
            }
            else
            {
                Debug.LogWarning("No valid photo size found in this post.");
            }
        }
        else
        {
            Debug.LogWarning("No valid photo attachment found in this post.");
        }
    }

    IEnumerator LoadImage(string url, GameObject newsItem)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("Image URL is empty.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            newsItem.transform.Find("Foto").GetComponent<RawImage>().texture = texture;
        }
    }
}