using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
namespace Common.Connectors;

public class SuggestionApiConnector : ISuggestionConnector
{
    private string _url = "http://localhost:5000/api/suggestion";
    private HttpClient? _client;

    public void Initialize()
    {
        _client = new HttpClient();
    }

    public async IAsyncEnumerable<string> GetSuggestion(string context)
    {
        var isEmpty = true;
        string message = "Error on server side, please contact your admnistrator";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Content = new StringContent(JsonSerializer.Serialize(new {Context = context}), Encoding.UTF8, "application/json")
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
                var response = JsonSerializer.Deserialize<ApiResponseStream>(line);
                if (!string.IsNullOrEmpty(response?.Text))
                {
                    result = response.Text;
                }
            }
            catch (Exception exception)
            {
                result = exception.Message;
            }
            yield return result;
        }
    }

    public class ApiResponseStream
    {
        [JsonPropertyName("Text")]
        public string Text { get; set; }
    }
}
