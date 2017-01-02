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
namespace GreenPipes
{
    using System;


    /// <summary>
    /// Filter exceptions for policies that act based on an exception
    /// </summary>
    public interface IExceptionFilter :
        IProbeSite
    {
        /// <summary>
        /// Returns true if the exception matches the filter and the policy should
        /// be applied to the exception.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>True if the exception matches the filter, otherwise false.</returns>
        bool Match(Exception exception);
    }
}