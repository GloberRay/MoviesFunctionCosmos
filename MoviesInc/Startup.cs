using Contracts;
using CosmosService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(MoviesInc.Startup))]

namespace MoviesInc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Best practice use singleton for cosmos
            builder.Services.AddSingleton<IMovieRepository, MovieRepository>();

            builder.Services.AddAzureClients(
                builder =>
                {
                    builder.AddServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnection"));
                });
        }
    }
}
