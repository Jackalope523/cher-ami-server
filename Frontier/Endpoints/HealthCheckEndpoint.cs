using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints
{
    public class HealthCheckEndpoint : EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await Send.OkAsync("" +
                "   @@@@@@@                                        \r\n" +
                "    @@+ -#@@@@@@@                                 \r\n" +
                "@@@@@@@        +%@@@@@@                           \r\n" +
                "@@@@@@@@*            :#@@@                        \r\n" +
                " @@@@@@@@@@@@@%=         @@                       \r\n" +
                " @@@@@@@@@@@@@@@@@@@@=   @@                       \r\n" +
                " @@@@@@@@@@@@@@@@@@@@@    @@                      \r\n" +
                "  @@@@@@@@@@@@@@@@@@@@*   %@@                     \r\n" +
                "  @@@@@@@@@@@@@@@@@@@@@    @@                     \r\n" +
                "   @@@@@@@@@@@@@@@@@@@@%   *@@                    \r\n" +
                "   @@@@@@@@@@@@@@@@@@@@@    @@@@@@@@              \r\n" +
                "    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@            \r\n" +
                "     @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@          \r\n" +
                "     @@@@@@@@@@@@@@@@@@@@@@@@@@ @@@@@@@@@@        \r\n" +
                "        @@@@@@@@@@@@@@@@@@@@@@@    *@@@@@@@@      \r\n" +
                "               @@@@@@@@@@@@@@@@@@=  @@@@@@@@@     \r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@  \r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@ \r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n" +
                "                   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@@@@@@@  \r\n" +
                "                     @@@@@@@@@@@@@@@@@@@@@@@@@    \r\n" +
                "                       @@@@@@@@@@@@@@@@           \r\n" +
                "                         @@@@@@@                  \r\n" +
                "                                                  \r\n" +
                "                                                  \r\n" +
                "                 Lazy Lizard Backend              \r\n" +
                "                     PRODUCTION                   \r\n" +
                "                       ONLINE                     \r\n" +
                "                                                  \r\n" +
                "                                                  \r\n");
        }
              
    }
}
