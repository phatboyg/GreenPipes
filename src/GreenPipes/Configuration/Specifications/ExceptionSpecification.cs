// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
            foreach (var exceptionType in exceptionTypes)
            {
                _exceptionFilter.Includes += exception => exceptionType.IsInstanceOfType(exception);
            }
        }

        void IExceptionConfigurator.Handle<T>()
        {
            _exceptionFilter.Includes += exception => exception is T;
        }

        void IExceptionConfigurator.Handle<T>(Func<T, bool> filter)
        {
            _exceptionFilter.Includes += exception => exception is T && filter((T)exception);
        }

        void IExceptionConfigurator.Ignore(params Type[] exceptionTypes)
        {
            foreach (var exceptionType in exceptionTypes)
            {
                _exceptionFilter.Excludes += exception => exceptionType.IsInstanceOfType(exception);
            }
        }

        void IExceptionConfigurator.Ignore<T>()
        {
            _exceptionFilter.Excludes += exception => exception is T;
        }

        void IExceptionConfigurator.Ignore<T>(Func<T, bool> filter)
        {
            _exceptionFilter.Excludes += exception => exception is T && filter((T)exception);
        }
    }
}