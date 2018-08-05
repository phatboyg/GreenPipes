namespace GreenPipes.Mapping
{
    using System.Threading;
    using System.Threading.Tasks;


    public interface IObjectInitializer<TObject>
        where TObject : class
    {
        /// <summary>
        /// Initialize the object, using the input
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TObject> Initialize(CancellationToken cancellationToken);
    }
}