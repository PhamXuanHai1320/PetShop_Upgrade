namespace PetShop_Upgrade.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; init; } = string.Empty;
    public int Port { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string VirtualHost { get; init; } = string.Empty;
    public string ExchangeName { get; init; } = string.Empty;
    public string QueueName { get; init; } = string.Empty;
}