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
    using System;


    /// <summary>
    /// A connect handle is returned by a non-asynchronous resource that supports
    /// disconnection (such as removing an observer, etc.)
    /// </summary>
    public interface ConnectHandle :
        IDisposable
    {
        /// <summary>
        /// Explicitly disconnect the handle without waiting for it to be disposed. If the 
        /// connection is disconnected, the disconnect will be ignored when the handle is disposed.
        /// </summary>
        void Disconnect();
    }
}