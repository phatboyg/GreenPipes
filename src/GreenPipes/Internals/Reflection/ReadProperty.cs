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
namespace GreenPipes.Internals.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;


    public class ReadProperty<T, TProperty> :
        IReadProperty<T, TProperty>
    {
        Func<T, TProperty> _getMethod;

        public ReadProperty(Type implementationType, string propertyName)
        {
            if (typeof(T).GetTypeInfo().IsValueType)
                throw new ArgumentException("The entity type must be a reference type");

            var propertyInfo = implementationType.GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("The implementation does not have a property named: " + propertyName);

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException("The property does not have an accessible get method");

            TProperty GetUsingReflection(T entity) => (TProperty)getMethod.Invoke(entity, null);

            TProperty Initialize(T entity)
            {
                Interlocked.Exchange(ref _getMethod, GetUsingReflection);

                Task.Factory.StartNew(() => GenerateExpressionGetMethod(implementationType, getMethod),
                    CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

                return GetUsingReflection(entity);
            }

            _getMethod = Initialize;
        }

        public TProperty Get(T content)
        {
            return _getMethod(content);
        }

        async Task GenerateExpressionGetMethod(Type implementationType, MethodInfo getMethod)
        {
            await Task.Yield();

            try
            {
                var method = CompileGetMethod(implementationType, getMethod);

                Interlocked.Exchange(ref _getMethod, method);
            }
            catch (Exception ex)
            {
#if !NETCORE
                if (Trace.Listeners.Count > 0)
                    Trace.WriteLine(ex.Message);
#endif
            }
        }

        static Func<T, TProperty> CompileGetMethod(Type implementationType, MethodInfo getMethod)
        {
            try
            {
                var instance = Expression.Parameter(typeof(T), "instance");
                var cast = Expression.TypeAs(instance, implementationType);

                var call = Expression.Call(cast, getMethod);

                var lambdaExpression = Expression.Lambda<Func<T, TProperty>>(call, instance);

                return ExpressionCompiler.Compile<Func<T, TProperty>>(lambdaExpression);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to compile get method for property {getMethod.Name} on entity {typeof(T).Name}", ex);
            }
        }
    }
}