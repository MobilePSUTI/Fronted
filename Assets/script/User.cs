public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public string First { get; set; }
    public string Last { get; set; }
    public string Second { get; set; }
    public string Role { get; set; }
    public byte[] avatarData { get; set; }
}

public class Student : User
{
    public string GroupName { get; set; }
}

public class Teacher : User
{
    // Дополнительные свойства преподавателя при необходимости
}
