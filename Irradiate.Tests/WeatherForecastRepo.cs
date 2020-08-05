using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irradiate.Tests
{
    public interface IWeatherForecastRepo
    {
        IEnumerable<WeatherForecast> Get(int count);
        Task<IEnumerable<WeatherForecast>> GetAsync(int count);
        Task<IEnumerable<WeatherForecast>> GetAsyncDelayed();
    }

    public class WeatherForecastRepo : IWeatherForecastRepo
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public IEnumerable<WeatherForecast> Get(int count)
        {
            var rng = new Random();
            return Enumerable.Range(1, count).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        public async Task<IEnumerable<WeatherForecast>> GetAsync(int count)
        {
            return await Task.FromResult(Get(count));
        }

        public async Task<IEnumerable<WeatherForecast>> GetAsyncDelayed()
        {
            await Task.Delay(500);
            return await Task.FromResult(Get(1));
        }
    }
}
