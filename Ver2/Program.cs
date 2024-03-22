using System;
using System.Data.SQLite; // Для работы с базой данных SQLite
using System.Net; // Для отправки HTTP запросов
using Newtonsoft.Json; // Для работы с JSON данными

namespace WeatherApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Шаг 1: Ввод названия города с клавиатуры
            Console.WriteLine("Введите название города:");
            string city = Console.ReadLine();

            string apiKey = "34747752906cc3846e5748a60c5d8956"; // Ваш API ключ OpenWeatherMap
            string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";

            // Шаг 2: Отправка запроса на получение прогноза погоды
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string json = webClient.DownloadString(apiUrl);
                    dynamic data = JsonConvert.DeserializeObject(json);

                    // Парсим данные о погоде из JSON
                    string weatherDescription = data.weather[0].description;
                    double temperature = Convert.ToDouble(data.main.temp);

                    // Шаг 3: Вывод погоды на консоль
                    Console.WriteLine($"Погода в городе {city}:");
                    Console.WriteLine($"Описание: {weatherDescription}");
                    Console.WriteLine($"Температура: {temperature:F2} °C");

                    // Шаг 4: Запись в базу данных объекта погода для города
                    SaveWeatherData(city, weatherDescription, temperature);
                }
                catch (WebException)
                {
                    Console.WriteLine("Ошибка: город не найден или проблемы с сетью.");
                }
            }

            // Шаг 5: Вывод погоды по городу из базы данных
            Console.WriteLine("\nПогода введенного города из базы данных:");
            GetWeatherDataFromDatabase(city);
        }

        // Метод для сохранения данных о погоде в базу данных
        static void SaveWeatherData(string city, string description, double temperature)
        {
            // Подключаемся к базе данных SQLite
            string connectionString = "Data Source=weather.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Создаем таблицу, если она не существует
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Weather (City TEXT, Description TEXT, Temperature REAL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Вставляем данные о погоде в таблицу
                string insertQuery = "INSERT INTO Weather (City, Description, Temperature) VALUES (@City, @Description, @Temperature)";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@City", city);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Temperature", temperature);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для получения данных о погоде из базы данных
        static void GetWeatherDataFromDatabase(string city)
        {
            // Подключаемся к базе данных SQLite
            string connectionString = "Data Source=weather.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Извлекаем данные о погоде для указанного города
                string selectQuery = "SELECT Description, Temperature FROM Weather WHERE City = @City";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@City", city);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string description = reader["Description"].ToString();
                            double temperature = Convert.ToDouble(reader["Temperature"]);
                            Console.WriteLine($"Описание: {description}");
                            Console.WriteLine($"Температура: {temperature:F2} °C");
                        }
                    }
                }
            }
        }
    }
}
