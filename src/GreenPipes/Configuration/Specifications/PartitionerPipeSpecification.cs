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
namespace GreenPipes.Specifications
{
    using System.Collections.Generic;
    using Filters;
    using Partitioning;


    public class PartitionerPipeSpecification<T> :
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly PartitionKeyProvider<T> _keyProvider;
        readonly int _partitionCount;
        readonly IPartitioner _partitioner;

        public PartitionerPipeSpecification(PartitionKeyProvider<T> keyProvider, int partitionCount)
        {
            _keyProvider = keyProvider;

            _partitionCount = partitionCount;
        }

        public PartitionerPipeSpecification(PartitionKeyProvider<T> keyProvider, IPartitioner partitioner)
        {
            _keyProvider = keyProvider;

            _partitioner = partitioner;
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            var partitioner = _partitioner ?? new Partitioner(_partitionCount, new Murmur3UnsafeHashGenerator());

            builder.AddFilter(new PartitionFilter<T>(_keyProvider, partitioner));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_keyProvider == null)
                yield return this.Failure("KeyProvider", "must not be null");
            if (_partitioner == null && _partitionCount < 1)
                yield return this.Failure("PartitionCount", "must be >= 1");
        }
    }
}