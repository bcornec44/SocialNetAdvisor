namespace Common.Connectors;

public interface ISuggestionConnector
{
    IAsyncEnumerable<string> GetSuggestion(string context, CancellationToken cancellationToken);
    void Initialize();
}