// Copyright 2012-2016 Chris Patterson
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
    using System.Text;
    using Partitioning;
    using Specifications;


    public static class PartitionerPipeConfigurationExtensions
    {
        /// <summary>
        /// Partitions the pipe using the key provider, ensuring that contexts with the same key are delivered
        /// through the same partition in a single-threaded context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount,
            Func<T, Guid> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            PartitionKeyProvider<T> provider = context => keyProvider(context).ToByteArray();

            var specification = new PartitionerPipeSpecification<T>(provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Partitions the pipe using the key provider, ensuring that contexts with the same key are delivered
        /// through the same partition in a single-threaded context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner to share</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner,
            Func<T, Guid> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            PartitionKeyProvider<T> provider = context => keyProvider(context).ToByteArray();

            var specification = new PartitionerPipeSpecification<T>(provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Partitions the pipe using the key provider, ensuring that contexts with the same key are delivered
        /// through the same partition in a single-threaded context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        /// <param name="encoding">The text encoding to use to convert the string to byte[] (defaults to UTF8)</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount,
            Func<T, string> keyProvider, Encoding encoding = null)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            Encoding textEncoding = encoding ?? Encoding.UTF8;

            PartitionKeyProvider<T> provider = context =>
            {
                string key = keyProvider(context) ?? "";
                return textEncoding.GetBytes(key);
            };

            var specification = new PartitionerPipeSpecification<T>(provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Partitions the pipe using the key provider, ensuring that contexts with the same key are delivered
        /// through the same partition in a single-threaded context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner to share</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        /// <param name="encoding">The text encoding to use to convert the string to byte[] (defaults to UTF8)</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner,
            Func<T, string> keyProvider, Encoding encoding = null)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            Encoding textEncoding = encoding ?? Encoding.UTF8;

            PartitionKeyProvider<T> provider = context =>
            {
                string key = keyProvider(context) ?? "";
                return textEncoding.GetBytes(key);
            };

            var specification = new PartitionerPipeSpecification<T>(provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }
    }
}