namespace Common.Connectors;

public class SuggestionMockConnector : ISuggestionConnector
{
    public void Initialize()
    {
    }
    public async IAsyncEnumerable<string> GetSuggestion(string context)
    {
        await Task.Delay(1000);
        yield return context;
    }
}
