// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Atlas.CommandLine.Accounts;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Blueprints.Providers;
using Microsoft.Atlas.CommandLine.Commands;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.JsonClient;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.Atlas.CommandLine.Templates;
using Microsoft.Atlas.CommandLine.Templates.Helpers;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Atlas.CommandLine
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var services = CreateServiceProvider();

            var secretTracker = services.GetRequiredService<ISecretTracker>();

            Console.SetOut(secretTracker.FilterTextWriter(new ColorConsoleWriter(ColorConsole.GetOutput())));
            Console.SetError(secretTracker.FilterTextWriter(new ColorConsoleWriter(ColorConsole.GetError())));

            var attributes = typeof(Program).GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
            var assemblyVersionAttribute = attributes.SingleOrDefault() as AssemblyInformationalVersionAttribute;

            var console = services.GetRequiredService<IConsole>();

            var consoleTitle = $"Atlas version {assemblyVersionAttribute?.InformationalVersion}";
            console.WriteLine(consoleTitle.Color(ConsoleColor.Cyan));

            var app = services.GetRequiredService<CommandLineApplicationServices>();

            ConfigureApplicationCommands(app);

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                console.Error.WriteLine(ex);
                return 1;
            }
            finally
            {
                console.Out.Flush();
                console.Error.Flush();
            }
        }

        public static IServiceProvider CreateServiceProvider()
        {
            return AddServices(new ServiceCollection()).BuildServiceProvider();
        }

        public static IServiceCollection AddServices(IServiceCollection services)
        {
            return services
                .AddSingleton<IYamlSerializers, YamlSerializers>()
                .AddSingleton<IJmesPathQuery, JmesPathQuery>()
                .AddSingleton<ISecretTracker, SecretTracker>()

                .AddSingleton<IJsonHttpClientFactory, JsonHttpClientFactory>()
                .AddSingleton<IHttpClientFactory, HttpClientFactory>()
                .AddSingleton<IHttpClientHandlerFactory, HttpClientHandlerFactory>()

                .AddSingleton<ISettingsDirectory, SettingsDirectory>()
                .AddSingleton<ISettingsManager, SettingsManager>()

                .AddTransient<IPatternMatcherFactory, PatternMatcherFactory>()

                .AddTransient<IBlueprintManager, BlueprintManager>()
                .AddTransient<IBlueprintPackageProvider, HttpsFilesBlueprintPackageProvider>()
                .AddTransient<IBlueprintPackageProvider, DirectoryBlueprintPackageProvider>()
                .AddTransient<IBlueprintPackageProvider, ArchiveBlueprintPackageProvider>()

                .AddSingleton<ITemplateEngineFactory, TemplateEngineFactory>()
                .AddSingleton<TemplateEngineServices>()
                .AddTransient<ITemplateHelperProvider, SshHelpers>()
                .AddTransient<ITemplateHelperProvider, BinaryHelpers>()

                .AddSingleton<CommandLineApplicationServices>()
                .AddTransient<IConsole, ConsoleService>()

                .AddTransient<WorkflowCommands>()
                .AddTransient<AccountCommands>()

                .AddLogging(builder =>
                {
                    builder
                        .AddConsole(opt => opt.IncludeScopes = true)
                        .AddFilter(level => true)
                        .SetMinimumLevel(LogLevel.Trace);
                })
                .Configure<LoggerFilterOptions>(options =>
                {
                    options.MinLevel = LogLevel.Trace;
                })
                ;
        }

        public static void ConfigureApplicationCommands(CommandLineApplicationServices app)
        {
            app.HelpOption("-h|--help").Inherited = true;

            app.Command("help", help =>
            {
                help.Description = "Show help information";
                var command = help.Argument("command", "Show help for particular arguments", true);

                help.OnExecute(() =>
                {
                    CommandLineApplication helpTarget = app;
                    foreach (var helpCommand in command.Values)
                    {
                        helpTarget = helpTarget.Commands.Single(c => c.Name == helpCommand);
                    }
                    helpTarget.ShowHelp();
                    return 0;
                });
            });

            app.Command("account", account =>
            {
                account.Description = "Manages authentication credentials";
                account.Command("clear", clear =>
                {
                    clear.Description = "Remove any authentication credentials which have been stored";
                    clear.OnExecute<AccountCommands>();
                });

                account.Command("show", show =>
                {
                    show.Description = " Displays the credentials have been stored";
                    show.Option("-n|--name", "Names to show", CommandOptionType.MultipleValue);
                    show.OnExecute<AccountCommands>();
                });

                account.Command("add", add =>
                {
                    add.Description = "Stores authentication credentials to be used by non-interactive deploy";
                    add.Option("-n|--name", "Unique name for entry being added", CommandOptionType.SingleValue);
                    add.Option("--resource", "Resource guid or uri being authorized", CommandOptionType.SingleValue);
                    add.Option("--authority", "OAuth token authority url", CommandOptionType.SingleValue);
                    add.Option("--tenant", "Azure Active Directory tenant id", CommandOptionType.SingleValue);

                    add.Option("--username", "User name for basic authentication", CommandOptionType.SingleValue);
                    add.Option("--password", "Password for basic authentication ", CommandOptionType.SingleValue);

                    add.Option("--pat", "Personal Access Token for VSTS authentication", CommandOptionType.SingleValue);

                    add.Option("--appid", "Application ID used for service principal authentication", CommandOptionType.SingleValue);
                    add.Option("--secret", "Application Secret used for service principal authentication", CommandOptionType.SingleValue);

                    add.Option("--token", "Existing bearer token", CommandOptionType.SingleValue);

                    add.OnExecute<AccountCommands>();
                });

                account.OnExecute(() =>
                {
                    account.ShowHelp();
                    return 1;
                });
            });

            app.Command("generate", generate =>
            {
                generate.Description = "Processes a workflow without executing any operations";
                generate.Argument("blueprint", "Path or url to atlas blueprint");

                generate.Option("-f|--values", "Input file containing parameter values", CommandOptionType.MultipleValue, inherited: true);
                generate.Option("-p|--set", "Set or override parameter values", CommandOptionType.MultipleValue, inherited: true);
                generate.Option("-o|--output-directory", "Output folder for generated files", CommandOptionType.SingleValue, inherited: true);

                generate.OnExecute<WorkflowCommands>();
            });

            app.Command("deploy", deploy =>
            {
                deploy.Description = "Processes a workflow and executes the operations";
                deploy.Argument("blueprint", "Path or url to atlas blueprint");
                deploy.Argument("target", "Name of workflow yaml file inside template", multipleValues: true);

                deploy.Option("-f|--values", "Input file containing parameter values", CommandOptionType.MultipleValue, inherited: true);
                deploy.Option("-p|--set", "Set or override parameter values", CommandOptionType.MultipleValue, inherited: true);
                deploy.Option("-o|--output-directory", "Output folder for generated files", CommandOptionType.SingleValue, inherited: true);

                deploy.Option("--dry-run", "Skip non-GET REST api calls", CommandOptionType.NoValue, inherited: true);
                deploy.Option("--non-interactive", "Disables all interactive prompting. Some examples of interactive prompting include requests to complete authentication steps.", CommandOptionType.NoValue, inherited: true);

                deploy.OnExecute<WorkflowCommands>();
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });
        }
    }
}
