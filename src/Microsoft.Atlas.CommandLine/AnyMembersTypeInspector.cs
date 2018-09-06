// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine
{
    internal class AnyMembersTypeInspector : ITypeInspector
    {
        private ITypeInspector _inner;

        public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            var anyPropertyInfo = GetAnyMembersPropertyInfo(type);

            if (anyPropertyInfo == null)
            {
                foreach (var propertyDescriptor in _inner.GetProperties(type, container))
                {
                    var thing = propertyDescriptor.Read(container);

                    yield return propertyDescriptor;
                }
            }
            else
            {
                foreach (var propertyDescriptor in _inner.GetProperties(type, container))
                {
                    if (!string.Equals(propertyDescriptor.Name, anyPropertyInfo.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var thing = propertyDescriptor.Read(container);

                        yield return propertyDescriptor;
                    }
                }

                var any = anyPropertyInfo.GetValue(container) as IDictionary;
                if (any != null)
                {
                    foreach (var name in any.Keys)
                    {
                        yield return new AnyMembersPropertyDescriptor(anyPropertyInfo, Convert.ToString(name));
                    }
                }
            }
        }

        public IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)
        {
            // TODO: think about conflicts
            var descriptor = _inner.GetProperty(type, container, name, ignoreUnmatched: true);

            if (descriptor == null)
            {
                var anyMembersPropertyInfo = GetAnyMembersPropertyInfo(type);
                if (anyMembersPropertyInfo != null)
                {
                    return new AnyMembersPropertyDescriptor(anyMembersPropertyInfo, name);
                }
            }

            return _inner.GetProperty(type, container, name, ignoreUnmatched);
        }

        internal static AnyMembersTypeInspector Create(ITypeInspector inner)
        {
            return new AnyMembersTypeInspector { _inner = inner };
        }

        private PropertyInfo GetAnyMembersPropertyInfo(Type type)
        {
            return type.GetTypeInfo().GetProperties().SingleOrDefault(p => p.GetCustomAttribute<YamlAnyMembersAttribute>() != null);
        }

        private class AnyMembersPropertyDescriptor : IPropertyDescriptor
        {
            private readonly PropertyInfo _anyMembersPropertyInfo;

            public AnyMembersPropertyDescriptor(PropertyInfo anyMembersPropertyInfo, string name)
            {
                _anyMembersPropertyInfo = anyMembersPropertyInfo;
                Name = name;
            }

            public string Name { get; set; }

            public bool CanWrite => true;

            public Type Type => typeof(object);

            public Type TypeOverride { get; set; }

            public int Order { get; set; }

            public ScalarStyle ScalarStyle { get; set; }

            public T GetCustomAttribute<T>()
                where T : Attribute
            {
                return null;
            }

            public IObjectDescriptor Read(object target)
            {
                var anyMembers = _anyMembersPropertyInfo.GetValue(target) as IDictionary;
                var value = anyMembers?[Name];

                // unclear what to do with this
                var actualType = TypeOverride ?? value?.GetType() ?? Type;
                return new ObjectDescriptor(value, actualType, Type, ScalarStyle);
            }

            public void Write(object target, object value)
            {
                var anyMembers = _anyMembersPropertyInfo.GetValue(target) as IDictionary;
                anyMembers[Name] = value;
            }
        }
    }
}
