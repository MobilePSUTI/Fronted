using UnityEngine;

public class NotificationButton : MonoBehaviour
{
    public void OnNotificationClick()
    {
        NotificationManager.Instance?.ToggleNotificationPanel();
    }
}
