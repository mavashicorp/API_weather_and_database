
//сделать консольное приложение

//1. ввести с клавиатуры название города
//2. отправить запрос на получение прогноза погоды
//3. вывести погоду на консоль
//4. записать в базу объект погода для города
//5. вывести на консоль погоду по городу

using MySql.Data.MySqlClient;
using Newtonsoft.Json;



namespace API_weather_and_database
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        const string connectionString = "server=localhost;user=root;password=1488;database=weatherdb;";



        static async Task Main(string[] args)
        {
            string location;
            do
            {
                Console.WriteLine("\nВведите название города:");
                location = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(location))
                {
                    Console.WriteLine("\nВы ввели пустую строку. Пожалуйста, введите название города.");
                }
            } while (string.IsNullOrWhiteSpace(location));

            string apiKey = "34747752906cc3846e5748a60c5d8956"; // Ключ к API OpenWeatherMap

            var weather = await FetchWeatherFromApi(location, apiKey); //получаю погоду с OpenWeather


            // вывод данных о погоде с экземпляра класса
            Console.WriteLine("\nВывод данных о погоде с экземпляра класса");
            Console.WriteLine($"Погода в городе {location}:");
            Console.WriteLine($"Описание: {weather.Description}");
            Console.WriteLine($"Температура: {weather.Temp} °C");
            Console.WriteLine($"Ветер: {weather.WindSpeed} м/с");


            SaveWeatherData(location, weather.Description, weather.Temp, weather.WindSpeed); //сохнаяняю в базу данных


            // Вывод погоды из базы данных
            Console.WriteLine("\nПогода введенного города из базы данных:");
            GetWeatherDataFromDatabase(location);

        }
        //получаю погоду с OpenWeather
        static async Task<WeatherData> FetchWeatherFromApi(string location, string apiKey)
        {
            HttpResponseMessage response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apiKey}&units=metric&lang=ru");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(content);
                return new WeatherData
                {
                    Name = result.name,
                    Description = result.weather[0].description,
                    Temp = result.main.temp,
                    WindSpeed = result.wind.speed
                };

            }
            else
            {
                throw new Exception("\nНе удалось получить данные о погоде.");
            }
        }


        //сохнаяняю в базу данных
        static void SaveWeatherData(string location, string Description, double Temp, double WindSpeed)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //создаю таблицу
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Weather (Id INT AUTO_INCREMENT PRIMARY KEY, City VARCHAR(255), Description VARCHAR(255), Temperature DOUBLE, WindSpeed DOUBLE)";
                using (MySqlCommand command = new MySqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                //наполняю таблицу
                string insertQuery = "INSERT INTO Weather (City, Description, Temperature, WindSpeed) VALUES (@City, @Description, @Temperature, @WindSpeed)";
                using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@City", location);
                    command.Parameters.AddWithValue("@Description", Description);
                    command.Parameters.AddWithValue("@Temperature", Temp);
                    command.Parameters.AddWithValue("@WindSpeed", WindSpeed);
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("\nДанные записаны в базу: weatherdb");//check
            }
        }


        static void GetWeatherDataFromDatabase(string location)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT Id, City, Description, Temperature, WindSpeed FROM Weather WHERE City = @location";
                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@location", location);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = Convert.ToString(reader["Id"]);
                            string city = Convert.ToString(reader["City"]);
                            string description = Convert.ToString(reader["Description"]);
                            double temperature = Convert.ToDouble(reader["Temperature"]);
                            double windspeed = Convert.ToDouble(reader["WindSpeed"]);

                            Console.WriteLine($"Id: {id}");
                            Console.WriteLine($"Город: {city}");
                            Console.WriteLine($"Описание: {description}");
                            Console.WriteLine($"Температура: {temperature} °C");
                            Console.WriteLine($"Ветер: {windspeed} м/с");
                        }
                    }
                }
            }

        }
    }
}



 




