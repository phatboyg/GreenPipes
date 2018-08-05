﻿// Copyright 2012-2018 Chris Patterson
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
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;


    public class DelegatePropertyInitializerSpecification<T, TProperty> :
        InitializerSpecification<T, TProperty>
        where T : class
    {
        readonly PropertyValueProvider<T, TProperty> _provider;

        public DelegatePropertyInitializerSpecification(Expression<Func<T, TProperty>> propertyExpression, PropertyValueProvider<T, TProperty> provider)
            : base(propertyExpression)
        {
            _provider = provider;
        }

        public DelegatePropertyInitializerSpecification(PropertyInfo property, PropertyValueProvider<T, TProperty> provider)
            : base(property)
        {
            _provider = provider;
        }

        protected override IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }

        public override void Apply(IInitializerBuilder<T> builder)
        {
            var initializer = new DelegatePropertyInitializer<T, TProperty>(_provider);

            builder.Property<TProperty>(Property.Name).Add(initializer);
        }
    }
}