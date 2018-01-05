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
namespace GreenPipes.Pipes
{
    using System;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// An active AgentContext is being used within a pipe. It is discarded once the pipe Send completes.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ActiveAgentContext<TContext> :
        ActiveAgentContextHandle<TContext>
        where TContext : class, PipeContext
    {
        readonly AgentContextHandle<TContext> _agentContext;
        readonly Task<TContext> _context;

        public ActiveAgentContext(AgentContextHandle<TContext> agentContext, TContext context)
        {
            _agentContext = agentContext;
            _context = Task.FromResult(context);
        }

        bool AgentContextHandle<TContext>.IsInactive => _agentContext.IsInactive;

        Task<TContext> AgentContextHandle<TContext>.Context => _context;

        Task AgentContextHandle<TContext>.Disavow()
        {
            return TaskUtil.Completed;
        }

        Task ActiveAgentContextHandle<TContext>.Faulted(Exception exception)
        {
            return _agentContext.Disavow();
        }
    }
}