namespace Common.Connectors;

internal class SuggestionMockConnector : ISuggestionConnector
{
    public void Initialize()
    {
    }
    public async IAsyncEnumerable<string> GetSuggestion(string context)
    {
        await Task.Delay(10000);
        yield return context;
    }
}
