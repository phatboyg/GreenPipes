using System;
using System.Threading.Tasks;
using GreenPipes.Filters;
using GreenPipes.Pipes;
using NUnit.Framework;

namespace GreenPipes.Tests
{
    public class DynamicRouter_Specs
    {
            [Test]
            public async Task Dispatching_a_pipe_by_type()
            {
                var dr = new DynamicRouter<Cxt>(new CxtFac());
            }
        
    }

    public class CxtFac : IPipeContextConverterFactory<Cxt>
    {
        IPipeContextConverter<Cxt, TOutput> IPipeContextConverterFactory<Cxt>.GetConverter<TOutput>()
        {
            throw new NotImplementedException();
        }
    }

    public class Cxt : 
        BasePipeContext,
        PipeContext
    {
        
    }
}