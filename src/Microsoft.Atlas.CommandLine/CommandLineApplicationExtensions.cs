// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine
{
    public static class CommandLineApplicationExtensions
    {
        public static string Color(this string text, ConsoleColor color)
        {
            return $"\x1b{{{(int)color}}}{text}\x1b{{-1}}";
        }

        public static string Required(this CommandArgument argument)
        {
            if (argument == null)
            {
                throw new Exception("Required CommandArgument is not declared on CommandLineApplication");
            }

            if (string.IsNullOrEmpty(argument.Value))
            {
                throw new ApplicationException($"Argument {argument.Name} is required.");
            }

            return argument.Value;
        }

        public static string Required(this CommandOption option)
        {
            if (option == null)
            {
                throw new Exception("Required CommandOption is not declared on CommandLineApplication");
            }

            if (!option.HasValue())
            {
                throw new ApplicationException($"--{option.LongName} is required.");
            }

            return option.Value();
        }

        public static string Optional(this CommandOption option, string defaultValue)
        {
            if (option == null)
            {
                throw new Exception("Required CommandOption is not declared on CommandLineApplication");
            }

            return option.HasValue() ? option.Value() : defaultValue;
        }

        public static IEnumerable<string> OptionalMany(this CommandOption option, params string[] defaultValue)
        {
            if (option == null)
            {
                throw new Exception("Required CommandOption is not declared on CommandLineApplication");
            }

            if (option.HasValue())
            {
                return option.Values;
            }

            return defaultValue;
        }

        public static void OnExecute<TCommand>(this CommandLineApplication app, Func<TCommand, int> onExecute)
        {
            app.OnExecute(() =>
            {
                var command = GetServiceProvider(app).GetRequiredService<TCommand>();
                var typeInfo = command.GetType().GetTypeInfo();
                foreach (var property in typeInfo.GetProperties().Where(prop => prop.PropertyType == typeof(CommandOption)))
                {
                    var commandOption = GetAllOptions(app).FirstOrDefault(option => Match(property, option));
                    if (commandOption != null)
                    {
                        property.SetValue(command, commandOption);
                    }
                }
                foreach (var property in typeInfo.GetProperties().Where(prop => prop.PropertyType == typeof(CommandArgument)))
                {
                    var commandArgument = GetAllArguments(app).FirstOrDefault(argument => Match(property, argument));
                    if (commandArgument != null)
                    {
                        property.SetValue(command, commandArgument);
                    }
                }
                return onExecute(command);
            });
        }

        public static void OnExecute<TCommand>(this CommandLineApplication app)
        {
            var helpOption = app.GetOptions().Single(x => x.LongName == "help");

            var method = typeof(TCommand).GetTypeInfo().GetMethod($"Execute{PascalCase(app.Name)}");

            app.OnExecute<TCommand>(cmd =>
            {
                if (helpOption.HasValue())
                {
                    app.ShowHelp();
                    return 0;
                }
                else
                {
                    var task = (Task<int>)method.Invoke(cmd, new object[0]);
                    return task.GetAwaiter().GetResult();
                }
            });
        }

        private static bool Match(PropertyInfo property, CommandOption option)
        {
            var propertyName = string.Join(string.Empty, option.LongName.Split('-').Select(PascalCase));

            return string.Equals(property.Name, propertyName, StringComparison.Ordinal);
        }

        private static bool Match(PropertyInfo property, CommandArgument argument)
        {
            var propertyName = PascalCase(argument.Name);

            return string.Equals(property.Name, propertyName, StringComparison.Ordinal);
        }

        private static string PascalCase(string segment)
        {
            return new string(
                segment.ToUpperInvariant().Take(1).Concat(segment.ToLowerInvariant().Skip(1)).ToArray());
        }

        private static IEnumerable<CommandOption> GetAllOptions(CommandLineApplication app)
        {
            for (var scan = app; scan != null; scan = scan.Parent)
            {
                foreach (var option in scan.Options)
                {
                    yield return option;
                }
            }
        }

        private static IEnumerable<CommandArgument> GetAllArguments(CommandLineApplication app)
        {
            for (var scan = app; scan != null; scan = scan.Parent)
            {
                foreach (var argument in scan.Arguments)
                {
                    yield return argument;
                }
            }
        }

        private static IServiceProvider GetServiceProvider(CommandLineApplication app)
        {
            for (var scan = app; scan != null; scan = scan.Parent)
            {
                if (scan is CommandLineApplicationServices)
                {
                    return (scan as CommandLineApplicationServices).Services;
                }
            }

            throw new ApplicationException($"Unable to locate {nameof(CommandLineApplicationServices)} ");
        }
    }
}
