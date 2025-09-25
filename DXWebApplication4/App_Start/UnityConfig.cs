using DXWebApplication4.Repository;
using DXWebApplication4.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace DXWebApplication4
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
     
            container.RegisterType<IProductRepository, ProductRepository>();

        
            container.RegisterType<IProductService, ProductService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}