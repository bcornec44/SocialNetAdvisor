namespace Common.Connectors;

public class SuggestionMockConnector : ISuggestionConnector
{
    public void Initialize()
    {
    }
    
    public async IAsyncEnumerable<string> GetSuggestion(string context, string personality, CancellationToken cancellationToken)
    {
        yield return personality;
        await Task.Delay(100, cancellationToken);
        foreach (var chunk in SplitStringInChunks(context, 7))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return chunk;
            await Task.Delay(100, cancellationToken);
        }
    }

    static List<string> SplitStringInChunks(string input, int chunkSize)
    {
        List<string> chunks = [];
        for (int i = 0; i < input.Length; i += chunkSize)
        {
            if (i + chunkSize <= input.Length)
                chunks.Add(input.Substring(i, chunkSize));
            else
                chunks.Add(input[i..]);
        }
        return chunks;
    }
}
