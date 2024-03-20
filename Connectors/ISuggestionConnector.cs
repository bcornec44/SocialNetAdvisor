namespace SocialNetAdvisor.Connectors;

internal interface ISuggestionConnector
{
    Task<string> GetSuggestion(string context);
    void Initialize();
}