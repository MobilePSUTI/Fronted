using UnityEngine;
using System.Data;
using MySqlConnector;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    private static string connectionString;
    private static MySqlConnection connection;

    // ������������� ����������� � ���� ������
    public void Initialize(string server, string database, string userId, string password)
    {
        connectionString = $"Server={server};Database={database};User ID={userId};Password={password};Pooling=false;";
        connection = new MySqlConnection(connectionString);
    }

    // ����������� � ���� ������
    public bool Connect()
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                Debug.Log("����������� � ���� ������ �������.");
            }
            return true;
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ ����������� � ���� ������: " + ex.Message);
            return false;
        }
    }

    // ���������� �� ���� ������
    public void Disconnect()
    {
        if (connection != null && connection.State == ConnectionState.Open)
        {
            connection.Close();
            Debug.Log("���������� �� ���� ������.");
        }
    }

    // �������������� ��������
    public bool AuthenticateStudent(string login, string password, out Student student)
    {
        student = null;
        if (connection.State != ConnectionState.Open)
        {
            Debug.LogError("��� ����������� � ���� ������.");
            return false;
        }

        string query = "SELECT * FROM students WHERE email = @login AND password = @password";
        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password", password);

        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    student = new Student
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("username"),
                        Login = reader.GetString("email"),
                        // �������� ������ ����, ���� ����������
                    };
                    return true;
                }
                else
                {
                    Debug.Log("������� �� ������.");
                    return false;
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ ��� ���������� �������: " + ex.Message);
            return false;
        }
    }

    // �������������� �������������
    public bool AuthenticateTeacher(string login, string password, out Teacher teacher)
    {
        teacher = null;
        if (connection.State != ConnectionState.Open)
        {
            Debug.LogError("��� ����������� � ���� ������.");
            return false;
        }

        string query = "SELECT * FROM Teachers WHERE Login = @login AND Password = @password";
        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password", password);

        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    teacher = new Teacher
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Login = reader.GetString("Login"),
                        // �������� ������ ����, ���� ����������
                    };
                    return true;
                }
                else
                {
                    Debug.Log("������������� �� ������.");
                    return false;
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ ��� ���������� �������: " + ex.Message);
            return false;
        }
    }

    // ��������� ������ ���� ����� �� ���� ������
    public List<Group> GetAllGroups()
    {
        List<Group> groups = new List<Group>();

        if (connection.State != ConnectionState.Open)
        {
            Debug.LogError("��� ����������� � ���� ������.");
            return groups;
        }

        string query = "SELECT id, title FROM `groups`"; // �����������, ��� � ��� ���� ������� id � title
        MySqlCommand command = new MySqlCommand(query, connection);

        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Group group = new Group
                    {
                        Id = reader.GetInt32("id"),
                        Title = reader.GetString("title")
                    };
                    groups.Add(group);
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ ��� ���������� �������: " + ex.Message);
        }

        return groups;
    }

    public List<Student> GetStudentsByGroup(int groupId)
    {
        List<Student> students = new List<Student>();

        if (connection.State != ConnectionState.Open)
        {
            Debug.LogError("��� ����������� � ���� ������.");
            return students;
        }

        // ������ � ������������ ������ students � groups
        string query = @"
        SELECT s.Id, s.first_name, s.last_name, s.second_name
        FROM students s
        JOIN `groups` g ON s.group_id = g.id
        WHERE g.id = @groupId;
    ";

        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@groupId", groupId);

        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Student student = new Student
                    {
                        Id = reader.GetInt32("Id"),
                        First = reader.GetString("first_name"),
                        Last = reader.GetString("last_name"),
                        Second = reader.GetString("second_name"),
                    };
                    students.Add(student);
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ ��� ���������� �������: " + ex.Message);
        }

        return students;
    }
}