using System.Security.Cryptography;
using System.Text;

namespace Itzyolotl.Api.Security;

// TEMPORAL: se reemplaza por Identity y politicas por rol en la Fase 4. No desplegar asi.
public sealed class StaffKeyFilter(string? expectedKey) : IEndpointFilter
{
    public const string HeaderName = "X-Staff-Key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (string.IsNullOrEmpty(expectedKey))
        {
            return Results.Problem(
                title: "Acceso del personal no configurado",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expectedKey));

        return matches ? await next(context) : Results.Unauthorized();
    }
}