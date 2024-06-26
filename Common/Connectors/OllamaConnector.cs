﻿using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Threading;
namespace Common.Connectors;

public class OllamaConnector : ISuggestionConnector
{
    private Dictionary<string, string> _templateByPersonality = new Dictionary<string, string>();
    private string _url = "http://localhost:11434/api/chat";
    private HttpClient? _client;

    public void Initialize()
    {
        _templateByPersonality[string.Empty] = "I'm going to write you a post that I read on social networks. I want you to read it and I want you to answer it for me, to comment on it. I want you to write as someone on the internet would write. The post has a strong opinion, I want you to convince the author of the post that either his information is inaccurate or that the opposite opinion is better. Just give me the answer in French, nothing in English. Here's the post: ";
        _client = new HttpClient();
    }

    public async IAsyncEnumerable<string> GetSuggestion(string context, string personality, CancellationToken cancellationToken)
    {
        var isEmpty = true;
        var template = GetTemplate(personality);
        string message = "Error on server side, please contact your admnistrator";
        var chatRequest = new ChatRequest
        {
            Model = "mistral",
            Messages = new List<Message>
            {
                new Message
                {
                    Role = "user",
                    Content = template + context
                }
            }
        };

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
        };
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead;
        using HttpResponseMessage response = await _client.SendAsync(request, completionOption, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            yield return message;
        }
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (var suggestion in ParseStream(stream, cancellationToken))
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

    private string GetTemplate(string personality)
    {
        if (_templateByPersonality.TryGetValue(personality, out var template))
        {
            return template;
        }
        return _templateByPersonality[string.Empty];
    }

    private async IAsyncEnumerable<string> ParseStream(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            string result = string.Empty;
            try
            {
                var response = JsonSerializer.Deserialize<ChatResponseStream>(line);
                if (response?.Message?.Content != null && !string.IsNullOrEmpty(response.Message.Content))
                {
                    result = response.Message.Content;
                }
            }
            catch (Exception exception)
            {
                result = exception.Message;
            }
            yield return result;
        }
    }

    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public IList<Message> Messages { get; set; }
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = true;

    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("images")]
        public string[] Images { get; set; }
    }
    public class ChatResponseStream
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
