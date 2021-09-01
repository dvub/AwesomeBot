using System;

public class Server
{
    public ulong? Id { get; set; }
    public string Prefix { get; set; }
    public string Greeting { get; set; }
    public GreetingType GreetingType { get; set; }
    public ulong? GreetingChannelId { get; set; }
    public ulong? CommandChannelId { get; set; }
}

public enum GreetingType
{
    Channel,
    DM,
    Disabled
}