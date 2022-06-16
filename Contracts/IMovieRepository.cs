using Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IMovieRepository
    {
        Task<List<Movie>> GetMoviesAsync();

        Task AddMovie(Movie movie);

        Task DeleteMovie();

        Task UpdateMovie(Movie movie);
    }
}
