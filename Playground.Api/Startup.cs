using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Api.Sheets;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallback(async context =>
                {
                    var spreadsheetId = (context.Request.Headers.FirstOrDefault(d => d.Value.ToString()
                        .ToLowerInvariant()
                        .StartsWith("ssid->")).Value.ToString() ?? "").Replace("ssid->", "");

                    if (spreadsheetId == "")
                    {
                        await WriteErrorJsonResponse(context, 401, "Identificador da planilha não foi localizado.");
                        return;
                    }

                    var sheetsService = await SheetServiceFactory.CreateSheetsService();
                    var endpoint = await SheetReader.GetEndpointFromSheet(sheetsService, spreadsheetId, 
                        context.Request.Method,
                        context.Request.Path.Value + context.Request.QueryString.Value);

                    if (endpoint is null)
                    {
                        await WriteErrorJsonResponse(context, 404, "Endpoint não encontrado.");
                        return;
                    }

                    await WriteJsonResponse(context, endpoint.ResponseCode, endpoint.ResponseBody);
                });
            });
        }

        private static async Task WriteErrorJsonResponse(HttpContext context, int statusCode, string msgError)
        {
            await WriteJsonResponse(context, statusCode, string.Concat("{ \"error\": \"", msgError, "\" }"));
        }

        private static async Task WriteJsonResponse(HttpContext context, int statusCode, string responseBody)
        {
            var byteResponse = Encoding.UTF8.GetBytes(responseBody);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = byteResponse.Length;

            var memStream = new MemoryStream(byteResponse);
            memStream.Position = memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(context.Response.Body);
        }
    }
}
