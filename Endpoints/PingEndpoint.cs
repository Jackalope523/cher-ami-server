using CherAmiAPI.Interfaces;
using FastEndpoints;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints
{
    public class PingEndpoint(IUserRepository userRepository) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/ping");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await userRepository.AnyUsersAsync(cancellationToken);
                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
               await Send.AcceptedAtAsync<PingEndpoint>(cancellation: cancellationToken);
            }
        }
    }
}
