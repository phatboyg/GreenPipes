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
namespace GreenPipes
{
    using System.Collections.Generic;
    using System.ComponentModel;


    /// <summary>
    /// A specification, that can be validated as part of a configurator, is used
    /// to allow nesting and chaining of specifications while ensuring that all aspects
    /// of the configuration are verified correct.
    /// </summary>
    public interface ISpecification
    {
        /// <summary>
        /// Validate the specification, ensuring that a successful build will occur.
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<ValidationResult> Validate();
    }
}