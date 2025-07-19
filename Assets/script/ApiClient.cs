// ApiClient.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ApiClient : MonoBehaviour
{
    private const string BaseUrl = "https://your-server-url.com/api";
    private static ApiClient _instance;

    public static ApiClient Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("ApiClient");
                _instance = obj.AddComponent<ApiClient>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public IEnumerator Login(string email, string password, Action<bool, string> callback)
    {
        var loginData = new { email = email, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/auth/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                callback?.Invoke(true, response.token);
            }
            else
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                callback?.Invoke(false, error?.message ?? "Connection error");
            }
        }
    }

    public IEnumerator GetUserData(string userId, string token, Action<bool, UserSession.UserData> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/users/{userId}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                UserSession.UserData userData = JsonUtility.FromJson<UserSession.UserData>(request.downloadHandler.text);
                callback?.Invoke(true, userData);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        }
    }

    public IEnumerator DownloadAvatar(string url, Action<bool, Texture2D> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                callback?.Invoke(true, texture);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        }
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string token;
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string message;
    }
}