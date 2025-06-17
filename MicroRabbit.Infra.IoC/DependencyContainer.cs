using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace MicroRabbit.Infra.IoC;

public static class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services)
    {
        //Domain Bus
        services.AddSingleton<IEventBus, RabbitMQBus>();
      
        //Subscriptions
        //services.AddTransient<TransferEventHandler>();

        //Domain Events
        //services.AddTransient<IEventHandler<Transfer.Domain.Upgrade.Events.TransferCreatedEvent>, TransferEventHandler>();

        //Domain Banking Commands
        //services.AddTransient<IRequestHandler<CreateTransferCommand, bool>, TransferCommandHandler>();

        //Application Services
        //services.AddTransient<IAccountService, AccountService>();
        //services.AddTransient<ITransferService, TransferService>();

        //Data
        //services.AddTransient<IAccountRepository, AccountRepository>();
        //services.AddTransient<ITransferRepository, TransferRepository>();
        //services.AddTransient<BankingDbContext>();
        //services.AddTransient<TransferDbContext>();
    }
}
