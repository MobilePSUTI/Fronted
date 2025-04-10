using UnityEngine;
using System.Collections.Generic;

public static class UserSession
{
    public static Student CurrentStudent { get; set; }
    public static Teacher CurrentTeacher { get; set; }
    public static List<Group> Groups { get; set; }
    public static int SelectedGroupId { get; set; }

    // Добавлено новое свойство
    public static object CurrentUser
    {
        get => CurrentStudent ?? (object)CurrentTeacher;
        set
        {
            if (value is Student student)
                CurrentStudent = student;
            else if (value is Teacher teacher)
                CurrentTeacher = teacher;
        }
    }
}