using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Lib.Commands;
using NServiceBus;
using NServiceBus.Transport.AzureServiceBus;
using JsonSerializer = NServiceBus.JsonSerializer;

namespace NSBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var config = new EndpointConfiguration("TestApp");
            var transportConnectionString = ConfigurationManager.ConnectionStrings["asb"].ConnectionString;

            var transport = config.UseTransport<AzureServiceBusTransport>();

            transport
                .ConnectionString(transportConnectionString)
                .UseForwardingTopology()
                .BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            //transport.MessageReceivers().AutoRenewTimeout(TimeSpan.FromMinutes(50));
            //transport.MessageReceivers().PrefetchCount(1);
            //            transport.Queues().EnableBatchedOperations(false);

            // According to your note in TestMessageHandler, processing can take somewhere between one second and one and a half minute.
            // To accommodate that, the lock duration should be set to two minutes.
            // By default the lock is set to 30 seconds, which is way too short than the emulated processing (delay of 35 seconds in your handler).
            transport.Queues().LockDuration(TimeSpan.FromMinutes(2));

            config.Conventions()
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("Lib.Commands"));

            config.Recoverability().Delayed(x => x.NumberOfRetries(0));
            config.Recoverability().Immediate(x => x.NumberOfRetries(0));

            // ASB doesn't need this legacy retries mechanism.
            // It is removed in version 8 of ASB transport.
            config.Recoverability().DisableLegacyRetriesSatellite();

            // No need to limit concurrency to one. It only hurts your performance
//            config.LimitMessageProcessingConcurrencyTo(1);

            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();
            config.UseSerialization<JsonSerializer>();
            config.SendFailedMessagesTo("error");

            var endpoint = await Endpoint.Start(config)
                .ConfigureAwait(false);

            var messagesToSendTasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                var task = endpoint.SendLocal(new TestCommand { Data = $"simple-{i}" });
                messagesToSendTasks.Add(task);
            }

            await Task.WhenAll(messagesToSendTasks)
                .ConfigureAwait(false);
            
            Console.ReadKey();

            await endpoint.Stop()
                .ConfigureAwait(false);
        }
    }
}
