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
namespace GreenPipes
{
    /// <summary>
    /// A dynamic router is a pipe on which additional pipes can be connected and context is 
    /// routed through the pipe based upon the output requirements of the connected pipes. It is built
    /// around the dynamic filter, which is the central point of the router.
    /// </summary>
    public interface IDynamicRouter<in TContext> :
        IPipe<TContext>,
        IPipeConnector,
        IObserverConnector
        where TContext : class, PipeContext
    {
    }


    /// <summary>
    /// A dynamic router is a pipe on which additional pipes can be connected and context is 
    /// routed through the pipe based upon the output requirements of the connected pipes. It is built
    /// around the dynamic filter, which is the central point of the router.
    /// </summary>
    public interface IDynamicRouter<in TContext, in TKey> :
        IDynamicRouter<TContext>,
        IKeyPipeConnector<TKey>
        where TContext : class, PipeContext
    {
    }
}