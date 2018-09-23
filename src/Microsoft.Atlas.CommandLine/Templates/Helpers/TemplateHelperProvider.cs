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
                    possibleDelegate = method.CreateDelegate(typeof(T), this);
                }
                catch
                {
                    possibleDelegate = null;
                }

                if (possibleDelegate != null)
                {
                    yield return new KeyValuePair<string, T>(
                        method.GetCustomAttribute<DescriptionAttribute>().Description,
                        (T)(object)possibleDelegate);
                }
            }
        }
    }
}
