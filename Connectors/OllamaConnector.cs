using OllamaSharp;
namespace SocialNetAdvisor.Connectors;

internal class OllamaConnector : ISuggestionConnector
{
    private string _template = string.Empty;
    private OllamaApiClient _ollamaApiClient;

    public void Initialize()
    {
        _template = "Je vais t'ecrire un post que j'ai lu sur les reseau sociaux. je veux que tu le lises et je souhaite que tu y repondes a ma place, que tu fasses un commentaire. Je veux que tu ecrives comme quelqu'un sur internet ecrirait. Le post a une opinion tranchee, je veux que tu arrives a convaincre l'auteur du post que soit ses informations sont inexactes, soit que l'opinion opposee est meilleure. Donne moi juste la reponse en francais, rien en anglais. voici le post : ";
        _ollamaApiClient = new OllamaApiClient("http://localhost:11434")
        {
            SelectedModel = "mistral"
        };
    }

    public async Task<string> GetSuggestion(string context)
    {
        string message = "";

        var chat = _ollamaApiClient.Chat(stream =>
        {
            message += stream.Message?.Content ?? "";
        });

        await chat.Send(_template + context);
        return message;
    }
}
