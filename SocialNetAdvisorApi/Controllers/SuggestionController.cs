using Common.Connectors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SocialNetAdvisorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuggestionController : ControllerBase
{
    private readonly ISuggestionConnector _suggestionConnector;

    public SuggestionController()
    {
        _suggestionConnector = new OllamaConnector();
        _suggestionConnector.Initialize();
    }

    [HttpPost]
    public async Task GetSuggestion([FromBody] string context, string personality, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("The context cannot be empty.");
            return;
        }

        Response.ContentType = "application/stream+json";

        try
        {
            await foreach (var suggestion in _suggestionConnector.GetSuggestion(context, personality, cancellationToken))
            {
                if (!Response.HasStarted)
                {
                    Response.StatusCode = 200;
                }

                await Response.WriteAsync(JsonSerializer.Serialize(new { Text = suggestion }));
                await Response.Body.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync("An error occurred while processing your request."+ex.Message);
            }
        }
    }
}
