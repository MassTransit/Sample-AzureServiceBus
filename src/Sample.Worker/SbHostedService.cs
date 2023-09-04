using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MassTransit.Transports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using static MassTransit.MessageHeaders;

namespace Sample.Worker
{
    public class SbHostedService : IHostedService
    {
        private readonly ILogger<SbHostedService> _logger;
        private readonly ServiceBusClient _client;
        private ServiceBusSessionProcessor _processor;
        private int _index;

        public SbHostedService(IConfiguration configuration, ILogger<SbHostedService> logger)
        {
            _logger = logger;
            var connectionString = configuration.GetConnectionString("AzureServiceBus");
            _client = new ServiceBusClient(connectionString);   
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var _serviceBusProcessorOptions = new ServiceBusSessionProcessorOptions
            {
                AutoCompleteMessages = true,
                MaxConcurrentSessions = 3
            };

            _processor = _client.CreateSessionProcessor("sample.contracts/ordersubmitted", "order-submitted-hosted", _serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");
 
            return Task.CompletedTask;
        }

        private Task ProcessMessagesAsync(ProcessSessionMessageEventArgs arg)
        {

            var myPayload = arg.Message.Body.ToObjectFromJson<OrderSubmitted>();
            var id= Interlocked.Increment(ref _index);
            var correlationId = arg.Message.CorrelationId;
            var sessionId = arg.Message.SessionId;;

            _logger.LogInformation("OrderSubmittedConsumer: Started Order Submitted: {OrderId}. DateTime: {now}. Correlation: {correlationId}. ID: {id}"
                , sessionId, DateTime.Now.ToString("o"), correlationId,id);
            Thread.Sleep(1000);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client?.DisposeAsync().AsTask()!;
        }
    }
}