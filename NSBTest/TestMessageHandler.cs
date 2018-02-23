using System;
using System.Threading.Tasks;
using Lib.Commands;
using NServiceBus;

namespace NSBTest
{
    public class TestMessageHandler : IHandleMessages<TestCommand>
    {
        public async Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"{DateTime.Now} begin ({message.Data})");
            await Task.Delay(TimeSpan.FromSeconds(35)).ConfigureAwait(false); //actualy it's a call to some lib. in reality it works from 1 sec to 1.5 minutes.
            Console.WriteLine($"{DateTime.Now} end ({message.Data})");
        }
    }
}