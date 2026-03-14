using Domovoy.Core.Services;
using Infrastructure.Web;
using nanoFramework.WebServer;
using System;
using System.Net;

namespace Domovoy.Infrastructure.Web
{
	public class ApiController
	{
		public IDeviceService _deviceService;

		public ApiController(IDeviceService deviceService)
		{
			_deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
		}

		[Route("api/on/{id}")]
		[Method("GET")]
		public void TurnOn(WebServerEventArgs e) 
		{
			/*if (_deviceService == null)
			{
				SendResponse(e.Context.Response, 500, "{\"error\":\"Device service not initialized\"}");
				return;
			}*/

			string deviceId = e.GetRouteParameter("id");
			Console.WriteLine(deviceId);

			bool result = _deviceService.TurnOnDevice(deviceId, "web_user");

			string responce = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"Device turned on\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, responce);
			WebServer.OutputAsStream(e.Context.Response, responce);
		}


		//Обработчик для включения/выключения ус-в
		public string HandleDeviceCommand(HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;

			//получаем Id из URL
			string url = request.RawUrl;
			string[] parts = url.Split('/');

			if (parts.Length == 4) 
				return SendResponse(response, 400, "{\"error\":\"Invalid URL format\"}");			

			string deviceId = parts[3]; // /api/device/{deviceId}/command
			string command = parts[4]; // /on/off/toggle/status

			bool result = false;
			string message = "";

			switch (command.ToLower()) 
			{
				case "on":
					result=_deviceService.TurnOnDevice(deviceId, "web_user");
					message = result ? "Ус-во включено" : "Ошибка включения";
					break;

				case "off":
					result = _deviceService.TurnOffDevice(deviceId, "web_user");
					message = result ? "Ус-во выключено" : "Ошибка выключения";
					break;

				case "toggle":
					result = _deviceService.ToggleDevice(deviceId, "web_user");
					message = result ? "Ус-во переключено" : "Ошибка переключения";
					break;
				case "status":
					var info = _deviceService.GetDeviceInfo(deviceId);
					if (info != null)
						return SendResponse(response, 200, JsonHelper.Serialize(info));
					else
						return SendResponse(response, 404, "{\"error\":\"Ус-во не найдено\"}");
				default:
					return SendResponse(response, 400, "{\"error\":\"Неизвестная команда\"}");
			}

			return SendResponse(response, result ? 200 : 500,
				$"{{\"success\":{result.ToString().ToLower()},\"message\":\"{message}\"}}");
		}

		//Обработчик для статистики
		public string HandleStats(HttpListenerContext context)
		{
			var stats = _deviceService.GetStatistics();

			return SendResponse(context.Response, 200, JsonHelper.Serialize(stats));
		}

		//обработчик списка ус-в
		public string HandleDeviceList(HttpListenerContext context)
		{
			//добавить метод в IDeviceService для получения всех ус-в, пока заглушка
			return SendResponse(context.Response, 200, "{\"devices\":[]}");
		}

		//вспомогательный метод для отправки ответа
		private string SendResponse(HttpListenerResponse response, int statusCode, string content)
		{
			response.StatusCode = statusCode;
			response.ContentType = "application/json";

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
			response.ContentLength64 = buffer.Length;
			response.OutputStream.Write(buffer, 0, buffer.Length);
			response.OutputStream.Flush();

			return null; //WebServer требует возврата строки, но мы уже отправили ответ
		}
	}
}
