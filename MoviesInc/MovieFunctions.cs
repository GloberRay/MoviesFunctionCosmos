using Azure.Messaging.ServiceBus;
using Contracts;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace MoviesInc
{
    public class MovieFunctions
    {
        private readonly IMovieRepository _cosmosRepository;
        private readonly ServiceBusClient _serviceBusClient;

        public MovieFunctions(IMovieRepository cosmosRepository, ServiceBusClient serviceBusClient)
        {
            _cosmosRepository = cosmosRepository;
            _serviceBusClient = serviceBusClient;
        }

        [FunctionName("GetMovies")]
        public async Task<IActionResult> GetMovies(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request GetMovies.");

            var movies = await _cosmosRepository.GetMoviesAsync();
            
            return new OkObjectResult(movies);
        }

        [FunctionName("AddMovie")]
        public async Task AddMovie(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "movie")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request AddMovie.");

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            Movie movie = JsonConvert.DeserializeObject<Movie>(content);

            await _cosmosRepository.AddMovie(movie);
        }

        [FunctionName("Delete_Movies_Registred_1Hour_Ago")]
        public async Task DeleteMovies([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("C# Timer trigger function processed a request Delete_Movies_Registred_1Hour_Ago");

            await _cosmosRepository.DeleteMovie();
        }

        [FunctionName("UpdateMovie")]
        public async Task UpdateMovie([HttpTrigger(AuthorizationLevel.Function, "put", Route = "movie")] HttpRequest req,
            ILogger log)
        {
            var sender = _serviceBusClient.CreateSender("update-movie");
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var message = new ServiceBusMessage(content);
            await sender.SendMessageAsync(message);
        }

        [FunctionName("UpdateServiceBus")]
        public async Task Run([ServiceBusTrigger("update-movie", Connection = "ServiceBusConnection")] string movieJson, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {movieJson}");

            var movie = JsonConvert.DeserializeObject<Movie>(movieJson);

            await _cosmosRepository.UpdateMovie(movie);
        }
    }
}
