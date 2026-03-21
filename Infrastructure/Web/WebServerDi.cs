using Microsoft.Extensions.DependencyInjection;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Net;
using System.Text;

namespace Infrastructure.Web
{
	public class WebServerDi : WebServer
	{
		private readonly IServiceProvider _serviceProvider;

		public WebServerDi(int port, HttpProtocol protocol,
			Type[] controllers, IServiceProvider serviceProvider) : base(port, protocol, controllers)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>
		/// Переопределение метода вызова марщрута - создаю контроллер через DI
		/// </summary>
		protected override void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
		{
			//вместо Activator.CreateInstance использую DI контейнер
			var controller =
				ActivatorUtilities.CreateInstance(_serviceProvider, route.Callback.DeclaringType);

			//вызываю метод с параметрами
			route.Callback.Invoke(controller, new object[] { new WebServerEventArgs(context) });
		}
	}
}
