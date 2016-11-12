// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Tests
{
    using System.Threading.Tasks;
    using Contexts;
    using NUnit.Framework;


    [TestFixture]
    public class Using_a_request_pipe
    {
        [Test]
        public async Task Should_support_a_simple_request_response_conversation()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<PurchaseTicket, TicketReceipt> requestPipe = pipe.CreateRequestPipe<PurchaseTicket, TicketReceipt>();

            var receipt = await requestPipe.Send(new PurchaseTicket() {EventName = "The Big Party", Quantity = 2}).Result();

            Assert.That(receipt.OrderNumber, Is.EqualTo("42"));
        }

        [Test]
        public async Task Should_support_pipe_configuration_on_the_result()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<PurchaseTicket, TicketReceipt> requestPipe = pipe.CreateRequestPipe<PurchaseTicket, TicketReceipt>(x =>
            {
                x.UseExecute(context => context.Result.AuthorizationCode = "8675309");
            });

            var receipt = await requestPipe.Send(new PurchaseTicket() {EventName = "The Big Party", Quantity = 2}).Result();

            Assert.That(receipt.OrderNumber, Is.EqualTo("42"));
            Assert.That(receipt.AuthorizationCode, Is.EqualTo("8675309"));
        }

        [Test]
        public async Task Should_support_multiple_result_types()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<PurchaseTicket> requestPipe = pipe.CreateRequestPipe<PurchaseTicket>(
                x => x.Result<TicketReceipt>(),
                x => x.Result<EventSoldOut>());

            ResultContext resultContext = await requestPipe.Send(new PurchaseTicket() {EventName = "Golden State Warriors", Quantity = 4});

            TicketReceipt receipt;
            Assert.That(resultContext.TryGetResult(out receipt), Is.False);

            EventSoldOut soldOut;
            Assert.That(resultContext.TryGetResult(out soldOut), Is.True);

            Assert.That(soldOut.EventName, Is.EqualTo("Golden State Warriors"));
        }

        [Test]
        public async Task Should_support_the_same_thing_a_different_way()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<PurchaseTicket, TicketReceipt> requestPipe = pipe.CreateRequestPipe<PurchaseTicket, TicketReceipt>();

            var receipt = await requestPipe.Send(new PurchaseTicket() {EventName = "The Big Party", Quantity = 2}).Result();

            Assert.That(receipt.OrderNumber, Is.EqualTo("42"));
        }

        static IPipe<RequestContext> CreateHandlerPipe()
        {
            IPipe<RequestContext> pipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseDispatch(new RequestConverterFactory(), d =>
                {
                    d.Handle<PurchaseTicket>(h =>
                    {
                        h.UseExecute(context =>
                        {
                            if (context.Request.EventName == "Golden State Warriors")
                            {
                               context.TrySetResult(new EventSoldOut {EventName = context.Request.EventName});
                            }

                            context.TrySetResult(new TicketReceipt
                            {
                                EventName = context.Request.EventName,
                                Quantity = context.Request.Quantity,
                                OrderNumber = "42"
                            });
                        });
                    });
                });
            });
            return pipe;
        }


        class PurchaseTicket
        {
            public string EventName { get; set; }
            public int Quantity { get; set; }
        }


        class TicketReceipt
        {
            public string EventName { get; set; }
            public int Quantity { get; set; }
            public string OrderNumber { get; set; }
            public string AuthorizationCode { get; set; }
        }


        class EventSoldOut
        {
            public string EventName { get; set; }
        }
    }
}