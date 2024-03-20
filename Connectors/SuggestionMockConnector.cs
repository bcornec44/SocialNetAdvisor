namespace SocialNetAdvisor.Connectors;

internal class SuggestionMockConnector : ISuggestionConnector
{
    public void Initialize()
    {
    }

    public async Task<string> GetSuggestion(string context)
    {
        await Task.Delay(10000);
        return context;
    }
}
