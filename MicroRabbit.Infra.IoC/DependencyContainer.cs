using MediatR;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain.CommandHandlers;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MicroRabbit.Infra.IoC;

public static class DependencyContainer
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankingDbContext>(options =>
         options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        //Domain Bus
        services.AddSingleton<IEventBus, RabbitMQBus>();

        //Subscriptions
        //services.AddTransient<TransferEventHandler>();

        //Domain Events
        //services.AddTransient<IEventHandler<Transfer.Domain.Upgrade.Events.TransferCreatedEvent>, TransferEventHandler>();

        //Domain Banking Commands
         services.AddTransient<IRequestHandler<CreateTransferCommand, bool>, TransferCommandHandler>();

        //Application Services
        services.AddTransient<IAccountService, AccountService>();
        //services.AddTransient<ITransferService, TransferService>();

        //Data
         services.AddTransient<IAccountRepository, AccountRepository>();
        //services.AddTransient<ITransferRepository, TransferRepository>();
     

        //services.AddTransient<TransferDbContext>();
    }
}
