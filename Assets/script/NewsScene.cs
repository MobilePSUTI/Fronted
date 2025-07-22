using UnityEngine;
using UnityEngine.UI;

public class NewsScene : MonoBehaviour
{
    public Image avatarDisplay;
    [SerializeField] private Sprite defaultAvatar; // ������ �� ���������

    void Start()
    {
        // ��������� ������� AvatarManager
        if (AvatarManager.Instance == null)
        {
            Debug.LogError("AvatarManager �� ������!");
            SetDefaultAvatar();
            return;
        }

        // �������� ������� ������
        Sprite currentAvatar = AvatarManager.Instance.GetCurrentAvatar();

        if (currentAvatar != null)
        {
            avatarDisplay.sprite = currentAvatar;
        }
        else
        {
            Debug.LogWarning("������ �� ������, ���������� �����������");
            SetDefaultAvatar();
        }
    }

    void SetDefaultAvatar()
    {
        if (defaultAvatar != null)
        {
            avatarDisplay.sprite = defaultAvatar;
        }
    }
}