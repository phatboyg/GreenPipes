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


    public class WriteProperty<T, TProperty> :
        IWriteProperty<T, TProperty>
    {
        Action<T, TProperty> _setMethod;

        public WriteProperty(Type implementationType, string propertyName)
        {
            if (typeof(T).GetTypeInfo().IsValueType)
                throw new ArgumentException("The entity type must be a reference type");

            var propertyInfo = implementationType.GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("The implementation does not have a property named: " + propertyName);

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
                throw new ArgumentException("The property does not have an accessible set method");

            void SetUsingReflection(T entity, TProperty property) => setMethod.Invoke(entity, new object[] {property});

            void Initialize(T entity, TProperty property)
            {
                Interlocked.Exchange(ref _setMethod, SetUsingReflection);

                SetUsingReflection(entity, property);

                Task.Factory.StartNew(() => GenerateExpressionSetMethod(implementationType, setMethod),
                    CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }

            _setMethod = Initialize;
        }

        public void Set(T content, TProperty value)
        {
            _setMethod(content, value);
        }

        async Task GenerateExpressionSetMethod(Type implementationType, MethodInfo setMethod)
        {
            await Task.Yield();

            try
            {
                var fastSetMethod = CompileSetMethod(implementationType, setMethod);

                Interlocked.Exchange(ref _setMethod, fastSetMethod);
            }
            catch (Exception ex)
            {
#if !NETCORE
                if (Trace.Listeners.Count > 0)
                    Trace.WriteLine(ex.Message);
#endif
            }
        }

        static Action<T, TProperty> CompileSetMethod(Type implementationType, MethodInfo setMethod)
        {
            try
            {
                var instance = Expression.Parameter(typeof(T), "instance");
                var value = Expression.Parameter(typeof(TProperty), "value");
                var cast = Expression.TypeAs(instance, implementationType);

                var call = Expression.Call(cast, setMethod, value);

                var lambdaExpression = Expression.Lambda<Action<T, TProperty>>(call, instance, value);

                return ExpressionCompiler.Compile<Action<T, TProperty>>(lambdaExpression);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to compile SetMethod for property {setMethod.Name} on entity {typeof(T).Name}", ex);
            }
        }
    }
}