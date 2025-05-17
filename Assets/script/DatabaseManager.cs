//using UnityEngine;
//using System.Data;
//using MySqlConnector;
//using System.Collections.Generic;

//public class DatabaseManager : MonoBehaviour
//{
//    private static string connectionString;
//    private static MySqlConnection connection;

//    // Инициализация подключения к базе данных
//    public void Initialize(string server, string database, string userId, string password)
//    {
//        connectionString = $"Server={server};Database={database};User ID={userId};Password={password};Pooling=false;";
//        connection = new MySqlConnection(connectionString);
//    }

//    // Подключение к базе данных
//    public bool Connect()
//    {
//        try
//        {
//            if (connection.State != ConnectionState.Open)
//            {
//                connection.Open();
//                Debug.Log("Подключение к базе данных успешно.");
//            }
//            return true;
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка подключения к базе данных: " + ex.Message);
//            return false;
//        }
//    }

//    // Отключение от базы данных
//    public void Disconnect()
//    {
//        if (connection != null && connection.State == ConnectionState.Open)
//        {
//            connection.Close();
//            Debug.Log("Отключение от базы данных.");
//        }
//    }

//    // Аутентификация студента
//    public bool AuthenticateStudent(string login, string password, out Student student)
//    {
//        student = null;
//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return false;
//        }

//        string query = "SELECT * FROM students WHERE email = @login AND password = @password";
//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@login", login);
//        command.Parameters.AddWithValue("@password", password);

//        try
//        {
//            using (MySqlDataReader reader = command.ExecuteReader())
//            {
//                if (reader.Read())
//                {
//                    student = new Student
//                    {
//                        Id = reader.GetInt32("Id"),
//                        Name = reader.GetString("username"),
//                        Login = reader.GetString("email"),
//                        // Добавьте другие поля, если необходимо
//                    };
//                    return true;
//                }
//                else
//                {
//                    Debug.Log("Студент не найден.");
//                    return false;
//                }
//            }
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при выполнении запроса: " + ex.Message);
//            return false;
//        }
//    }

//    // Аутентификация преподавателя
//    public bool AuthenticateTeacher(string login, string password, out Teacher teacher)
//    {
//        teacher = null;
//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return false;
//        }

//        string query = "SELECT * FROM Teachers WHERE Login = @login AND Password = @password";
//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@login", login);
//        command.Parameters.AddWithValue("@password", password);

//        try
//        {
//            using (MySqlDataReader reader = command.ExecuteReader())
//            {
//                if (reader.Read())
//                {
//                    teacher = new Teacher
//                    {
//                        Id = reader.GetInt32("Id"),
//                        Name = reader.GetString("Name"),
//                        Login = reader.GetString("Login"),
//                        // Добавьте другие поля, если необходимо
//                    };
//                    return true;
//                }
//                else
//                {
//                    Debug.Log("Преподаватель не найден.");
//                    return false;
//                }
//            }
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при выполнении запроса: " + ex.Message);
//            return false;
//        }
//    }

//    // Получение списка всех групп из базы данных
//    public List<Group> GetAllGroups()
//    {
//        List<Group> groups = new List<Group>();

//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return groups;
//        }

//        string query = "SELECT id, title FROM `groups`"; // Предположим, что у вас есть колонки id и title
//        MySqlCommand command = new MySqlCommand(query, connection);

//        try
//        {
//            using (MySqlDataReader reader = command.ExecuteReader())
//            {
//                while (reader.Read())
//                {
//                    Group group = new Group
//                    {
//                        Id = reader.GetInt32("id"),
//                        Title = reader.GetString("title")
//                    };
//                    groups.Add(group);
//                }
//            }
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при выполнении запроса: " + ex.Message);
//        }

//        return groups;
//    }

//    public List<Student> GetStudentsByGroup(int groupId) // Принимаем groupId как int
//    {
//        List<Student> students = new List<Student>();

//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return students;
//        }

//        // Используем group_id как число
//        string query = "SELECT * FROM students WHERE group_id = @groupId";
//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@groupId", groupId); // Передаем groupId как число

//        try
//        {
//            using (MySqlDataReader reader = command.ExecuteReader())
//            {
//                while (reader.Read())
//                {
//                    Student student = new Student
//                    {
//                        Id = reader.GetInt32("Id"),
//                        First = reader.GetString("first_name"),
//                        Last = reader.GetString("last_name"),
//                        Second = reader.GetString("second_name"),
//                        // Добавьте другие поля, если необходимо
//                    };
//                    students.Add(student);
//                }
//            }
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при выполнении запроса: " + ex.Message);
//        }

//        return students;
//    }
//    // Добавляем в класс DatabaseManager следующие методы:

//    public byte[] GetUserAvatar(int userId)
//    {
//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return null;
//        }

//        string query = "SELECT avatar FROM Students WHERE Id = @userId";
//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@userId", userId);

//        try
//        {
//            object result = command.ExecuteScalar();
//            return result as byte[];
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при получении аватара: " + ex.Message);
//            return null;
//        }
//    }

//    public bool CheckEmailExists(string email)
//    {
//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базе данных.");
//            return false;
//        }

//        string query = "SELECT COUNT(*) FROM Students WHERE email = @email";
//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@email", email);

//        try
//        {
//            long count = (long)command.ExecuteScalar();
//            return count > 0;
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при проверке email: " + ex.Message);
//            return false;
//        }
//    }

//    public bool RegisterNewStudent(string email, string password, string firstName, string lastName, string secondName, int groupId, byte[] avatar)
//    {
//        if (connection.State != ConnectionState.Open)
//        {
//            Debug.LogError("Нет подключения к базу данных.");
//            return false;
//        }

//        string query = @"INSERT INTO Students 
//                    (email, password, first_name, last_name, second_name, group_id, avatar) 
//                    VALUES 
//                    (@email, @password, @firstName, @lastName, @secondName, @groupId, @avatar)";

//        MySqlCommand command = new MySqlCommand(query, connection);
//        command.Parameters.AddWithValue("@email", email);
//        command.Parameters.AddWithValue("@password", password);
//        command.Parameters.AddWithValue("@firstName", firstName);
//        command.Parameters.AddWithValue("@lastName", lastName);
//        command.Parameters.AddWithValue("@secondName", secondName);
//        command.Parameters.AddWithValue("@groupId", groupId);
//        command.Parameters.AddWithValue("@avatar", avatar);

//        try
//        {
//            int rowsAffected = command.ExecuteNonQuery();
//            return rowsAffected > 0;
//        }
//        catch (MySqlException ex)
//        {
//            Debug.LogError("Ошибка при регистрации студента: " + ex.Message);
//            return false;
//        }
//    }
//}