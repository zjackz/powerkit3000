using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HappyTools.WebApp.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMimeTypes = new[] { "multipart/form-data" };

            if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => fileUploadMimeTypes.Contains(x.Key)))
            {
                return;
            }

            var fileParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(IFormFile))
                .ToList();

            if (fileParameters.Any())
            {
                operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name,
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            })
                    }
                };
            }
        }
    }
}
