using DomainService.Abstract;
using DomainService.Concrete;
using Microsoft.Practices.Unity;
using System;

namespace SmsService
{
    public class UnityConfig
    {
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IBotStorageRepo, BotStorageRepo>(new ContainerControlledLifetimeManager());
            container.RegisterType<IHorseRacingDbRepo, HorseRacingDbRepo>(new ContainerControlledLifetimeManager());
        }
    }
}
