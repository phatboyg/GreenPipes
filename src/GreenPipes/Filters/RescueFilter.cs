// Copyright 2012-2018 Chris Patterson
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
namespace GreenPipes.Filters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;


    /// <summary>
    /// Rescue catches an exception, and if the exception matches the exception filter,
    /// passes control to the rescue pipe.
    /// </summary>
    /// <typeparam name="TContext">The context type</typeparam>
    /// <typeparam name="TRescueContext"></typeparam>
    public class RescueFilter<TContext, TRescueContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
        where TRescueContext : class, PipeContext
    {
        readonly IExceptionFilter _exceptionFilter;
        readonly RescueContextFactory<TContext, TRescueContext> _rescueContextFactory;
        readonly IPipe<TRescueContext> _rescuePipe;

        public RescueFilter(IPipe<TRescueContext> rescuePipe, IExceptionFilter exceptionFilter,
            RescueContextFactory<TContext, TRescueContext> rescueContextFactory)
        {
            _rescuePipe = rescuePipe;
            _exceptionFilter = exceptionFilter;
            _rescueContextFactory = rescueContextFactory;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("rescue");

            _rescuePipe.Probe(scope);
        }

        [DebuggerNonUserCode]
        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            try
            {
                await next.Send(context).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (!_exceptionFilter.Match(ex.GetBaseException()))
                    throw;

                var rescueContext = _rescueContextFactory(context, ex);

                await _rescuePipe.Send(rescueContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!_exceptionFilter.Match(ex))
                    throw;

                var rescueContext = _rescueContextFactory(context, ex);

                await _rescuePipe.Send(rescueContext).ConfigureAwait(false);
            }
        }
    }
}