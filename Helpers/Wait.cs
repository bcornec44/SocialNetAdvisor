namespace SocialNetAdvisor.Helpers;

internal static class Wait
{
    private const double timeSeed = .3;
    private static Random Random = new Random(DateTime.Now.Day);

    public static void S(double minSeconds, double maxSeconds)
    {
        int delayInMilliseconds = Random.Next((int)(minSeconds * 1000), (int)(maxSeconds * 1000) + 1);
        Task.Delay(delayInMilliseconds).Wait();
    }

    public static void S(double seconds = 1)
    {
        S(seconds, seconds + timeSeed);
    }

    public static void M(double minMinutes, double maxMinutes)
    {
        S(minMinutes * 60, maxMinutes * 60);
    }

    public static void M(double minutes = 1)
    {
        M(minutes, minutes + timeSeed);
    }

    public static async Task S(double minSeconds, double maxSeconds, CancellationToken cancellationToken)
    {
        int delayInMilliseconds = Random.Next((int)(minSeconds * 1000), (int)(maxSeconds * 1000) + 1);
        await Task.Delay(delayInMilliseconds, cancellationToken);
    }

    public static async Task S(double seconds, CancellationToken cancellationToken)
    {
        await S(seconds, seconds + timeSeed, cancellationToken);
    }

    public static async Task M(double minMinutes, double maxMinutes, CancellationToken cancellationToken)
    {
        await S(minMinutes * 60, maxMinutes * 60, cancellationToken);
    }

    public static async Task M(double minutes, CancellationToken cancellationToken)
    {
        await M(minutes, minutes + timeSeed, cancellationToken);
    }
}
