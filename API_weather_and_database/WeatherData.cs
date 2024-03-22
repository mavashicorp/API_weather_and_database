using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_weather_and_database
{
    public class WeatherData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Temp { get; set; }

        public double WindSpeed { get; set; }
    }
}
