using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints
{
    public class RootEndpoint : EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            string rabbit =
                "    @@@@@@@                                            \r\n" +
                "    @@+ -#@@@@@@@                                      \r\n" +
                "@@@@@@@        +%@@@@@@                                \r\n" +
                "@@@@@@@@*            :#@@@                             \r\n" +
                " @@@@@@@@@@@@@%=         @@                            \r\n" +
                " @@@@@@@@@@@@@@@@@@@@=   @@                            \r\n" +
                " @@@@@@@@@@@@@@@@@@@@@    @@                           \r\n" +
                "  @@@@@@@@@@@@@@@@@@@@*   %@@                          \r\n" +
                "  @@@@@@@@@@@@@@@@@@@@@    @@                          \r\n" +
                "   @@@@@@@@@@@@@@@@@@@@%   *@@                         \r\n" +
                "   @@@@@@@@@@@@@@@@@@@@@    @@@@@@@@                   \r\n" +
                "    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                 \r\n" +
                "     @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@               \r\n" +
                "     @@@@@@@@@@@@@@@@@@@@@@@@@@ @@@@@@@@@@             \r\n" +
                "        @@@@@@@@@@@@@@@@@@@@@@@    *@@@@@@@@           \r\n" +
                "               @@@@@@@@@@@@@@@@@@=  @@@@@@@@@          \r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@       \r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@      \r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@     \r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@     \r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@     \r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@       \r\n" +
                "                     @@@@@@@@@@@@@@@@@@@@@@@@@         \r\n" +
                "                       @@@@@@@@@@@@@@@@                \r\n" +
                "                         @@@@@@@                       \r\n" +
                "                                                       \r\n" +
                "                                                       \r\n" +
                "                        Crazy Lizard                   \r\n" +
                "                         PRODUCTION                    \r\n" +
                "                           ONLINE                      \r\n" +
                "                                                       \r\n" +
                "                                                       \r\n";

            HttpContext.Response.ContentType = "text/plain";
            await Send.StringAsync(rabbit, cancellation: cancellationToken);
        }
    }
}
