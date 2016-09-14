using System;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes.Filters;
using GreenPipes.Payloads;
using GreenPipes.Pipes;
using NUnit.Framework;

namespace GreenPipes.Tests
{
    public class DynamicRouter_Specs
    {
        bool _aWasCalled;
        bool _bWasCalled;
        IDynamicRouter<VendorContext> _router;

        [SetUp]
        public void SetUp()
        {
            _aWasCalled = false;
            _bWasCalled = false;

            _router = new DynamicRouter<VendorContext>(new VendorConverterFactory());

            var pipeA = Pipe.New<VendorContext<VendorARecord>>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    _aWasCalled = true;
                }); 
                
            });
            var pipeB = Pipe.New<VendorContext<VendorBRecord>>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    _bWasCalled = true;
                });
            });

            _router.ConnectPipe(pipeA);
            _router.ConnectPipe(pipeB);


            Console.WriteLine(_router.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task A()
        {
            await _router.Send(new Vendor<VendorARecord>("A"));

            Assert.That(_aWasCalled, Is.True);
            Assert.That(_bWasCalled, Is.False);
        }

        [Test]
        public async Task B()
        {
            await _router.Send(new Vendor<VendorBRecord>("B"));

            Assert.That(_aWasCalled, Is.False);
            Assert.That(_bWasCalled, Is.True);
        }

        public class VendorConverterFactory : IPipeContextConverterFactory<VendorContext>
        {
            public IPipeContextConverter<VendorContext, TOutput> GetConverter<TOutput>()
                where TOutput : class, PipeContext
            {
                var innerType = typeof(TOutput).GetClosingArguments(typeof(VendorContext<>)).Single();

                return
                    (IPipeContextConverter<VendorContext, TOutput>)
                        Activator.CreateInstance(typeof(Converter<>).MakeGenericType(innerType));
            }

            class Converter<TVendor> :
                IPipeContextConverter<VendorContext, VendorContext<TVendor>>
                where TVendor : class, VendorRecord
            {
                bool IPipeContextConverter<VendorContext, VendorContext<TVendor>>.TryConvert(VendorContext input,
                    out VendorContext<TVendor> output)
                {
                    output = input as VendorContext<TVendor>;

                    return output != null;
                }
            }
        }


        public interface VendorContext : PipeContext
        {
            string RawData { get; }
        }

        public interface VendorContext<TVendorRecord> : VendorContext where TVendorRecord : VendorRecord
        {
            TVendorRecord Record { get; }
        }

        public class Vendor : BasePipeContext, VendorContext
        {
            public Vendor(string rawData) : base(new PayloadCache())
            {
                RawData = rawData;
            }

            public string RawData { get; set; }
        }

        public class Vendor<TVendorRecord> : BasePipeContext, VendorContext<TVendorRecord>
            where TVendorRecord : VendorRecord
        {
            public Vendor(string rawData) : base(new PayloadCache())
            {
                RawData = rawData;
            }

            public string RawData { get; set; }
            public TVendorRecord Record { get; set; }
        }

        public interface VendorRecord
        {
        }

        public class VendorARecord : VendorRecord
        {
        }

        public class VendorBRecord : VendorRecord
        {
        }
    }
}