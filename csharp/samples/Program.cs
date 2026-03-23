namespace SampleConsumer;

/// <summary>
/// Provides a tiny consumer entrypoint used to smoke-test the local analyzer package.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Runs the sample consumer.
    /// </summary>
    private static void Main()
    {
        int count = 1;

        if (count > 0)
        {
            System.Console.WriteLine(count);
        }
    }
}
