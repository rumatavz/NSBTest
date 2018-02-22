using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Lib.Commands;
using NServiceBus;

namespace NSBTest
{
    public class TestMessageHandler : IHandleMessages<TestCommand>
    {
        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine(DateTime.Now + " begin");
            Thread.Sleep(TimeSpan.FromSeconds(35)); //actualy it's a call to some lib. in reality it works from 1 sec to 1.5 minutes.
            Console.WriteLine(DateTime.Now + " end");
            return Task.FromResult(0);
        }
    }
}