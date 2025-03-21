using UnityEngine;
using System.Collections.Generic;

public static class UserSession
{
    public static Student CurrentStudent { get; set; }
    public static Teacher CurrentTeacher { get; set; }
    public static List<Group> Groups { get; set; } // �������� ��� �� List<Group>
    public static int SelectedGroupId { get; set; }
}