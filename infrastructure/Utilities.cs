namespace infrastructure;
public class Utilities
{
    private static readonly Uri Uri = new Uri(Environment.GetEnvironmentVariable("pgconn")!);

    public static readonly string
        ProperlyFormattedConnectionString = string.Format(
            "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=5;",
            Uri.Host,
            Uri.AbsolutePath.Trim('/'),
            Uri.UserInfo.Split(':')[0],
            Uri.UserInfo.Split(':')[1],
            Uri.Port > 0 ? Uri.Port : 5432);

    public static readonly Uri ProductionUri  = new Uri(Environment.GetEnvironmentVariable("pgconn")!);

    public static readonly string
        ProductionDatabaseConnectionString = string.Format(
            "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=5;",
            ProductionUri.Host,
            ProductionUri.AbsolutePath.Trim('/'),
            ProductionUri.UserInfo.Split(':')[0],
            ProductionUri.UserInfo.Split(':')[1],
            ProductionUri.Port > 0 ? ProductionUri.Port : 5432);
}
