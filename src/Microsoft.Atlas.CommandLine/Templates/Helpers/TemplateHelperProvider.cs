// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Templates.Helpers
{
    public abstract class TemplateHelperProvider : ITemplateHelperProvider
    {
        public IEnumerable<KeyValuePair<string, HandlebarsHelper>> GetHelpers()
        {
            return GetHelpers<HandlebarsHelper>();
        }

        public IEnumerable<KeyValuePair<string, HandlebarsBlockHelper>> GetBlockHelpers()
        {
            return GetHelpers<HandlebarsBlockHelper>();
        }

        private IEnumerable<KeyValuePair<string, T>> GetHelpers<T>()
        {
            var thisType = GetType();
            foreach (var method in thisType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.GetCustomAttribute<DescriptionAttribute>() != null))
            {
                Delegate possibleDelegate;
                try
                {
#if NETCOREAPP2_0
                    possibleDelegate = method.CreateDelegate(typeof(T), this);
#else
                    possibleDelegate = Delegate.CreateDelegate(typeof(T), method, this);
#endif
                }
                catch
                {
                    possibleDelegate = null;
                }

                if (possibleDelegate != null)
                {
#if NETCOREAPP2_0
                    yield return new KeyValuePair<string, T>(
                        method.GetCustomAttribute<DescriptionAttribute>().Description,
                        (T)(object)possibleDelegate);
#else
                    yield return new KeyValuePair<string, T>(
                        ((DescriptionAttribute)Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute))).Description,
                        (T)(object)possibleDelegate);
#endif
                }
            }
        }
    }
}
