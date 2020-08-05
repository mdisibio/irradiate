using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Irradiate.Tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        IWeatherForecastRepo _repo;

        public WeatherForecastController(IWeatherForecastRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return _repo.Get(5);
        }

        [HttpGet]
        [Route("{count}")]
        public IEnumerable<WeatherForecast> GetCount(int count)
        {
            return _repo.Get(count);
        }

        [HttpGet]
        [Route("async/{count}")]
        public async Task<IEnumerable<WeatherForecast>> GetCountAsync(int count)
        {
            return await _repo.GetAsync(count);
        }

        [HttpGet]
        [Route("async/delayed")]
        public async Task<IEnumerable<WeatherForecast>> GetCountAsyncDelayed()
        {
            return await _repo.GetAsyncDelayed();
        }
    }
}
