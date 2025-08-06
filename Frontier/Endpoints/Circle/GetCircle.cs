using FastEndpoints;
using Frontier.Contracts.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetCircle(ICircleService circles) : Endpoint<CircleIdRequest, CircleShard>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            //long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //CircleShard circleShard = await circles.GetCircleInformationAsync(userId, request.CircleId);

            //if (circleShard == null)
            //    await Send.NotFoundAsync(cancellationToken);

            //await Send.OkAsync(circleShard, cancellationToken);
            throw new NotImplementedException();
        }
    }
}