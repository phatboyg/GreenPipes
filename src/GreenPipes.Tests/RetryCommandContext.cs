// Copyright 2012-2017 Chris Patterson
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
    using System;
    using System.Threading;
    using Contracts;
    using GreenPipes.Internals.Extensions;


    public class RetryCommandContext :
        CommandContext,
        CommandRetryContext
    {
        readonly CommandContext _context;

        public RetryCommandContext(CommandContext context)
        {
            _context = context;
        }

        public DateTime Timestamp => _context.Timestamp;

        public CancellationToken CancellationToken => _context.CancellationToken;

        public bool HasPayloadType(Type payloadType)
        {
            return _context.HasPayloadType(payloadType);
        }

        public bool TryGetPayload<TPayload>(out TPayload payload) where TPayload : class
        {
            return _context.TryGetPayload(out payload);
        }

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory) where TPayload : class
        {
            return _context.GetOrAddPayload(payloadFactory);
        }

        public T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory) where T : class
        {
            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }

        public int RetryAttempt { get; set; }

        public RetryCommandContext CreateNext()
        {
            return new RetryCommandContext(_context);
        }

        public virtual TContext CreateNext<TContext>()
            where TContext : RetryCommandContext
        {
            throw new InvalidOperationException("This is only supported by a derived type");
        }
    }


    public class RetryCommandContext<T> :
        RetryCommandContext,
        CommandContext<T>
        where T : class
    {
        readonly CommandContext<T> _context;

        public RetryCommandContext(CommandContext<T> context)
            : base(context)
        {
            _context = context;
        }

        T CommandContext<T>.Command => _context.Command;

        public override TContext CreateNext<TContext>()
        {
            return new RetryCommandContext<T>(_context) as TContext
                ?? throw new ArgumentException($"The context type is not valid: {TypeCache<T>.ShortName}");
        }
    }
}