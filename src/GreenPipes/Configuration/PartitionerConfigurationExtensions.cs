namespace GreenPipes
{
    using System;
    using System.Text;
    using Partitioning;
    using Specifications;


    public static class PartitionerConfigurationExtensions
    {
        /// <summary>
        /// Create a partitioner which can be used across multiple partitioner filters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount"></param>
        /// <returns></returns>
        public static IPartitioner CreatePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount)
            where T : class, PipeContext
        {
            return new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount, Func<T, Guid> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                return keyProvider(context).ToByteArray();
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner that is shared</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner, Func<T, Guid> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                return keyProvider(context).ToByteArray();
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        /// <param name="encoding"></param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount, Func<T, string> keyProvider, Encoding encoding = null)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            var textEncoding = encoding ?? Encoding.UTF8;

            byte[] Provider(T context)
            {
                var key = keyProvider(context) ?? "";
                return textEncoding.GetBytes(key);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner that is shared</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        /// <param name="encoding"></param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner, Func<T, string> keyProvider,
            Encoding encoding = null)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            var textEncoding = encoding ?? Encoding.UTF8;

            byte[] Provider(T context)
            {
                var key = keyProvider(context) ?? "";
                return textEncoding.GetBytes(key);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount, Func<T, long> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                var key = keyProvider(context);
                return BitConverter.GetBytes(key);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner that is shared</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner, Func<T, long> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                var key = keyProvider(context);
                return BitConverter.GetBytes(key);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitionCount">The number of partitions to use when distributing message delivery</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, int partitionCount, Func<T, byte[]> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                return keyProvider(context);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitionCount);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner that is shared</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        public static void UsePartitioner<T>(this IPipeConfigurator<T> configurator, IPartitioner partitioner, Func<T, byte[]> keyProvider)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            byte[] Provider(T context)
            {
                return keyProvider(context);
            }

            var specification = new PartitionerPipeSpecification<T>(Provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }
    }
}
