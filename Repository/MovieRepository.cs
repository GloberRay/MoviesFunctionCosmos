using Contracts;
using Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CosmosService
{
    public class MovieRepository : IMovieRepository
    {
        private readonly string _endpointUri;
        private readonly string _primaryKey;
        private readonly string _containerId;
        private readonly string _databaseId;
        private CosmosClient _cosmosClient;
        private Container _container;
        private Database _database;

        public MovieRepository()
        {
            _endpointUri = Environment.GetEnvironmentVariable("EndpointUri");
            _primaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
            _containerId = Environment.GetEnvironmentVariable("ContainerId");
            _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
            _cosmosClient = new CosmosClient(_endpointUri, _primaryKey);
            GetStarted();
        }

        private void GetStarted()
        {
            _database = _cosmosClient.GetDatabase(_databaseId);
            _container = _database.GetContainer(_containerId);
        }

        public async Task AddMovie(Movie movie)
        {
            try
            {
                ItemResponse<Movie> response = await _container.ReadItemAsync<Movie>(movie.Id, new PartitionKey(movie.Title));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                movie.CreatedDate = DateTime.UtcNow;
                await _container.CreateItemAsync<Movie>(movie, new PartitionKey(movie.Title));
                _cosmosClient.Dispose();
            }
        }

        public async Task<List<Movie>> GetMoviesAsync()
        {
            // Linq implementation (sync)
            List<Movie> movies = _container.GetItemLinqQueryable<Movie>(true).ToList();

            //Query implementation (async)
            //var query = "SELECT * FROM c";
            //QueryDefinition queryDefinition = new QueryDefinition(query);
            //using FeedIterator<Movie> iterator = _container.GetItemQueryIterator<Movie>(queryDefinition);
            //List<Movie> movies = new List<Movie>();
            //while(iterator.HasMoreResults)
            //{
            //    FeedResponse<Movie> currentResultSet = await iterator.ReadNextAsync();
            //    foreach (var movie in currentResultSet)
            //    {
            //        movies.Add(movie);
            //    }
            //}

            _cosmosClient.Dispose();

            return movies;
        }

        public async Task DeleteMovie()
        {
            List<Movie> movies = _container.GetItemLinqQueryable<Movie>(true).Where(m => m.CreatedDate <= DateTime.UtcNow.AddHours(-1)).ToList();

            foreach (var movie in movies)
            {
                await _container.DeleteItemAsync<Movie>(movie.Id, new PartitionKey(movie.Title));
            }

            _cosmosClient.Dispose();
        }

        public async Task UpdateMovie(Movie movie)
        {
            try
            {
                ItemResponse<Movie> movieData = await _container.ReadItemAsync<Movie>(movie.Id, new PartitionKey(movie.Title));
                var itemBody = movieData.Resource;
                itemBody.Category = movie.Category;
                itemBody.IsRegistered = true;
                movieData = await _container.ReplaceItemAsync<Movie>(itemBody, itemBody.Id, new PartitionKey(itemBody.Title));
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }
    }
}
