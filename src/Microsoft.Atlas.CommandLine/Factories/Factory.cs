using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine.Factories
{
    public interface IFactory<TService>
    {
        Task<TService> Create<TArguments>(TArguments arguments);
    }

    public interface IFactoryInstance<TArguments>
    {
        Task Initialize(TArguments arguments);
    }

    public class Factory<TService> : IFactory<TService>
    {
        private readonly IServiceProvider _serviceProvider;

        public Factory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TService> Create<TArguments>(TArguments arguments)
        {
            var instance = _serviceProvider.GetService<TService>();
            await ((IFactoryInstance<TArguments>)instance).Initialize(arguments);
            return instance;
        }
    }
}
