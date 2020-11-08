using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudioFreesia.Vivideo.Server
{
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        private readonly string basePath;

        public SwaggerDocumentFilter(string basePath) => this.basePath = basePath;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var api in context.ApiDescriptions)
            {
                api.RelativePath = this.basePath + api.RelativePath;
            }
        }
    }
}
