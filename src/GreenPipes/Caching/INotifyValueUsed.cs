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
namespace GreenPipes.Caching
{
    using System;


    /// <summary>
    /// If a cached value implments this interface, the cache will attach itself to the
    /// event so the value can signal usage to update the lifetime of the value.
    /// </summary>
    public interface INotifyValueUsed
    {
        /// <summary>
        /// Should be raised by the value when used, to keep it alive in the cache.
        /// </summary>
        event Action Used;
    }
}