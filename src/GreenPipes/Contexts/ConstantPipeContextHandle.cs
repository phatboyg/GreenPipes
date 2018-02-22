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
namespace GreenPipes.Contexts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    public class ConstantPipeContextHandle<TContext> :
        PipeContextHandle<TContext>
        where TContext : class, PipeContext
    {
        readonly TContext _context;
        bool _disposed;

        public ConstantPipeContextHandle(TContext context)
        {
            _context = context;

            Context = Task.FromResult(context);
        }

        async Task IAsyncDisposable.DisposeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
                return;

            if (_context is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync(cancellationToken).ConfigureAwait(false);

            if (_context is IDisposable disposable)
                disposable.Dispose();

            _disposed = true;
        }

        bool PipeContextHandle<TContext>.IsDisposed => _disposed;

        public Task<TContext> Context { get; }
    }
}