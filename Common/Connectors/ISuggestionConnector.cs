namespace Common.Connectors;

public interface ISuggestionConnector
{
    IAsyncEnumerable<string> GetSuggestion(string context);
    void Initialize();
}