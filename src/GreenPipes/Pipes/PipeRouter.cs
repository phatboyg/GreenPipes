namespace GreenPipes.Pipes
{
    using Contexts;


    public class PipeRouter :
        DynamicRouter<PipeContext>,
        IPipeRouter
    {
        public PipeRouter()
            : base(new PipeContextConverterFactory())
        {
        }
    }
}
