using System;

/// <summary>
/// Server model for database.
/// </summary>
public class Server
{
    public ulong? Id { get; set; }
    public string Prefix { get; set; }
    public string Greeting { get; set; }
    public GreetingType GreetingType { get; set; }
    public ulong? GreetingChannelId { get; set; }
    public ulong? CommandChannelId { get; set; }
}
/// <summary>
/// Enum for greeting type.
/// </summary>
public enum GreetingType
{
    Channel,
    DM,
    Disabled
}