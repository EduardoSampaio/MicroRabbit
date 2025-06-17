using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus;

public sealed class RabbitMQBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;

    public RabbitMQBus(IMediator mediator)
    {
        _mediator = mediator;
        _handlers = [];
        _eventTypes = [];
    }

    public async Task Publish<T>(T @event) where T : Event
    {
        var factory = new ConnectionFactory()
        {
            UserName = "admin",
            HostName = "localhost",
            Password = "admin",
            Port = 5672,
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var eventName = @event.GetType().Name;

        await channel.QueueDeclareAsync(eventName, false, false, false, null);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: eventName, body);

    }

    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }

    public async Task SubscribeAsync<T, TH>()
        where T : Event
        where TH : IEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        if (!_eventTypes.Contains(typeof(T)))
        {
            _eventTypes.Add(typeof(T));
        }

        if (!_handlers.ContainsKey(eventName))
        {
            _handlers.Add(eventName, []);
        }

        if (_handlers[eventName].Any(h => h == handlerType))
        {
            throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
        }

        _handlers[eventName].Add(handlerType);

        await StartBasicConsumeAsync<T>();
    }

    private async Task StartBasicConsumeAsync<T>() where T : Event
    {
        var factory = new ConnectionFactory()
        {
            UserName = "admin",
            HostName = "localhost",
            Password = "admin",
            Port = 5672
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var eventName = typeof(T).Name;

        await channel.QueueDeclareAsync(eventName, false, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += Consumer_Received;
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        var eventName = @event.RoutingKey;
        var message = Encoding.UTF8.GetString(@event.Body.ToArray());

        try
        {
            await ProcessEvent(eventName, message).ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_handlers.ContainsKey(eventName))
        {
            var subscriptions = _handlers[eventName];
            foreach(var sub in subscriptions)
            {
                var handler = Activator.CreateInstance(sub) as IEventHandler;
                if (handler is null) continue;
                var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                if (eventType is null) continue;
                var @event = JsonSerializer.Deserialize(message, eventType);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                await (Task)concreteType.GetMethod("Handle")!.Invoke(handler, [@event])!;
            }
        }
    }
}
