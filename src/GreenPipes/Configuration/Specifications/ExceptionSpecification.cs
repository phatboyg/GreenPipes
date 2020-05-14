namespace GreenPipes.Specifications
{
    using System;
    using Configurators;
    using Policies.ExceptionFilters;


    public abstract class ExceptionSpecification :
        IExceptionConfigurator
    {
        readonly CompositeFilter<Exception> _exceptionFilter;

        protected ExceptionSpecification()
        {
            _exceptionFilter = new CompositeFilter<Exception>();
            Filter = new CompositeExceptionFilter(_exceptionFilter);
        }

        protected IExceptionFilter Filter { get; }

        void IExceptionConfigurator.Handle(params Type[] exceptionTypes)
        {
            _exceptionFilter.Includes += exception => Match(exception, exceptionTypes);
        }

        void IExceptionConfigurator.Handle<T>()
        {
            _exceptionFilter.Includes += exception => Match(exception, typeof(T));
        }

        void IExceptionConfigurator.Handle<T>(Func<T, bool> filter)
        {
            _exceptionFilter.Includes += exception => Match(exception, filter);
        }

        void IExceptionConfigurator.Ignore(params Type[] exceptionTypes)
        {
            _exceptionFilter.Excludes += exception => Match(exception, exceptionTypes);
        }

        void IExceptionConfigurator.Ignore<T>()
        {
            _exceptionFilter.Excludes += exception => Match(exception, typeof(T));
        }

        void IExceptionConfigurator.Ignore<T>(Func<T, bool> filter)
        {
            _exceptionFilter.Excludes += exception => Match(exception, filter);
        }

        static bool Match(Exception exception, params Type[] exceptionTypes)
        {
            var baseException = exception.GetBaseException();

            for (var i = 0; i < exceptionTypes.Length; i++)
            {
                if (exceptionTypes[i].IsInstanceOfType(exception))
                    return true;

                if (exceptionTypes[i].IsInstanceOfType(baseException))
                    return true;
            }

            return false;
        }

        static bool Match<T>(Exception exception, Func<T, bool> filter)
            where T : Exception
        {
            if (exception is T exceptionOfT)
                return filter(exceptionOfT);

            var baseException = exception.GetBaseException();

            exceptionOfT = baseException as T;
            if (exceptionOfT != null)
                return filter(exceptionOfT);

            return false;
        }
    }
}
