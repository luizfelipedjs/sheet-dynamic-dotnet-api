using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Playground.Api.Sheets
{
    public class SheetServiceFactory
    {
        public static async Task<SheetsService> CreateSheetsService(CancellationToken token = default)
        {
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

            GoogleCredential credential = await GoogleCredential.FromStreamAsync(stream, token);

            credential = credential.CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

            SheetsService service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Playground_Api"
            });

            return service;
        }
    }
}
