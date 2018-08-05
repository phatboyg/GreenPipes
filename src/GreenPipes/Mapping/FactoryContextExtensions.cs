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
namespace GreenPipes.Mapping
{
    public static class FactoryContextExtensions
    {
        /// <summary>
        /// Allows the tuple structure of the factory context to be deconstructed into the object, purely syntax sugar, probably not the
        /// smartest thing to do
        /// </summary>
        /// <param name="factoryContext"></param>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        public static void Deconstruct<T>(this InitializerContext<T> factoryContext, out T value, out InitializerContext<T> context)
            where T : class
        {
            value = factoryContext.Object;
            context = factoryContext;
        }
    }
}