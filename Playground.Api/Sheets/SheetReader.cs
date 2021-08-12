using Google.Apis.Sheets.v4;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playground.Api.Sheets
{
    public class SheetReader
    {
        public static async Task<Models.Endpoint> GetEndpointFromSheet(SheetsService service, string spreadsheetId, 
            string method, string endpointRegex)
        {
            try
            {
                var getRequest = service.Spreadsheets.Get(spreadsheetId);
                var getResponse = await getRequest.ExecuteAsync();

                if (!getResponse.Sheets.Any())
                    return default;

                var sheet = getResponse.Sheets.FirstOrDefault();

                var range = sheet.Properties.Title + "!A1:" + "D";

                var entriesRequest = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var entriesResponse = await entriesRequest.ExecuteAsync();

                var endpoint = new Models.Endpoint();

                foreach (var value in entriesResponse.Values)
                {
                    if (value[0].ToString().ToLowerInvariant() != method.ToLowerInvariant())
                        continue;
                    else
                        endpoint.Method = value[0].ToString();

                    if (!Regex.IsMatch(endpointRegex, value[1].ToString(), RegexOptions.IgnoreCase))
                        continue;
                    else
                        endpoint.Url = value[1].ToString();

                    endpoint.ResponseCode = Convert.ToInt32(value[2].ToString());
                    endpoint.ResponseBody = value[3].ToString();

                    return endpoint;
                }
            }
            catch
            {
                return new Models.Endpoint
                {
                    ResponseCode = 500,
                    ResponseBody = "{ \"error\": \"Erro ao tentar criar o endpoint.\" }"
                };
            }

            return null;
        }
    }
}
