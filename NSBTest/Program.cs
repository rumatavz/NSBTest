using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib;
using Lib.Commands;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.MessageMutator;
using NServiceBus.Serializers.Json;
using NServiceBus.Transport.AzureServiceBus;
using JsonSerializer = NServiceBus.JsonSerializer;

namespace NSBTest
{
    class Program
    {
        private static IEndpointInstance _bus;

        static void Main(string[] args)
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
            transport.Queues().EnableBatchedOperations(false);

            config.Conventions()
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("Lib.Commands"));

            config.Recoverability().Delayed(x => x.NumberOfRetries(0));
            config.Recoverability().Immediate(x => x.NumberOfRetries(0));

            config.LimitMessageProcessingConcurrencyTo(1);

            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();
            config.UseSerialization<JsonSerializer>();
            config.SendFailedMessagesTo("error");

            var bus = Endpoint.Start(config);

            _bus = bus.Result;
            for (int i = 0; i < 20; i++)
            {
                _bus.SendLocal(new TestCommand() {Data = "simple"});
            }
            
            Console.ReadKey();
        }
    }
}
