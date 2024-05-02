using System.Text.Json;
using System.Text;
namespace Common.Connectors;

public class SuggestionApiConnector : ISuggestionConnector
{
    private string _url = "http://raspberrypi.local:5000/api/Suggestion";
    private HttpClient? _client;

    public void Initialize()
    {
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(3)
        };
    }

    public async IAsyncEnumerable<string> GetSuggestion(string context, CancellationToken cancellationToken)
    {
        var isEmpty = true;
        string message = "Error on server side, please contact your admnistrator";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Content = new StringContent(JsonSerializer.Serialize(context), Encoding.UTF8, "application/json")
        };
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead;
        using HttpResponseMessage response = await _client.SendAsync(request, completionOption);

        if (!response.IsSuccessStatusCode)
        {
            yield return message;
        }
        var stream = await response.Content.ReadAsStreamAsync();
        await foreach (var suggestion in ParseStream(stream))
        {
            if (isEmpty)
            {
                isEmpty = false;
            }

            yield return suggestion;
        }

        if (isEmpty)
        {
            yield return message;
        }
    }

    private async IAsyncEnumerable<string> ParseStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            string result = string.Empty;
            try
            {
                var suggestion = JsonSerializer.Deserialize<JsonElement>(line);
                if (suggestion.TryGetProperty("Text", out JsonElement textElement))
                {
                    result = textElement.GetString()??string.Empty;
                }
            }
            catch (Exception exception)
            {
                result = exception.Message;
            }
            yield return result;
        }
    }
}
