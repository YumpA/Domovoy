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
			string deviceId = GetDeviceId(e);

			bool result = _deviceService.TurnOnDevice(deviceId, "web_user");

			string responce = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"Device turned on\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, responce);
		}

		[Route("api/off/{id}")]
		[Method("GET")]
		public void TurnOff(WebServerEventArgs e)
		{
			string deviceId = GetDeviceId(e);
			Console.WriteLine(deviceId);

			bool result = _deviceService.TurnOffDevice(deviceId, "web_user");

			string responce = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"Device turned off\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, responce);
		}

		[Route("api/toggle/{id}")]
		[Method("GET")]
		public void ToggleDevice(WebServerEventArgs e)
		{
			string deviceId = GetDeviceId(e);

			bool result = _deviceService.ToggleDevice(deviceId, "web_user");

			string responce = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"Device toggled\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, responce);
		}

		[Route("api/blink/{id}")]
		[Method("GET")]
		public void Blink(WebServerEventArgs e)
		{
			string deviceId = GetDeviceId(e);
			string query = e.Context.Request.RawUrl;
			Console.WriteLine(deviceId);
			int count = 3, delay = 200;

			if (!string.IsNullOrEmpty(query))
			{
				var parts = query.TrimStart('?').Split('&');
				foreach (var part in parts)
				{
					var kv = part.Split('=');
					if (kv[0] == "count") int.TryParse(kv[1], out count);
					if (kv[0] == "delay") int.TryParse(kv[1], out delay);
				}
			}
			bool result = _deviceService.BlinkDevice(deviceId, count, delay, "web_user");
			string response = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"Blink started\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, response);
		}

		[Route("api/timer/{id}")]
		public void Timer(WebServerEventArgs e)
		{
			string deviceId = GetDeviceId(e);
			string query = e.Context.Request.RawUrl;
			int seconds = 10;
			bool turnOn = true;
			if (!string.IsNullOrEmpty(query))
			{
				var parts = query.TrimStart('?').Split('&');
				foreach (var part in parts)
				{
					var kv = part.Split('=');
					if (kv[0] == "seconds") int.TryParse(kv[1], out seconds);
					if (kv[0] == "action") turnOn = kv[1] == "on";
				}
			}
			bool result=_deviceService.SetTimer(deviceId, seconds, turnOn, "web_user");
			string response= $"{{\"success\":{result.ToString().ToLower()}," +
				$"\"message\":\"Timer set for {seconds} seconds\"}}";
			SendResponse(e.Context.Response, result ? 200 : 500, response);
		}

		[Route("api/status/{id}")]
		[Method("GET")]
		public void GetDeviceInfo(WebServerEventArgs e)
		{
			string deviceId = GetDeviceId(e);

			var info = _deviceService.GetDeviceInfo(deviceId);
			if (info != null)
			{
				string json = JsonHelper.Serialize(info);
				SendResponse(e.Context.Response, 200, json);
			}

			else
			{
				SendResponse(e.Context.Response, 404, "{\"error\":\"Device not found\"}");
			}
		}

		private string GetDeviceId(WebServerEventArgs e)
		{
			string url = e.Context.Request.RawUrl;
			char[] separators = { '/', '?' };
			//Console.WriteLine(url);
			string[] parts = url.Split(separators);
			string id = parts[3];

			if (url.Length == 4)
				WebServer.OutputAsStream(e.Context.Response, "{\"error\":\"Invalid URL format\"}");

			return id;
		}

		//вспомогательный метод для отправки ответа
		private void SendResponse(HttpListenerResponse response, int statusCode, string content)
		{
			try
			{
				response.StatusCode = statusCode;
				response.ContentType = "application/json";

				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
				response.ContentLength64 = buffer.Length;
				response.OutputStream.Write(buffer, 0, buffer.Length);
				response.OutputStream.Flush();
			}

			finally
			{
				response.Close();
			}
			//return null; //WebServer требует возврата строки, но мы уже отправили ответ
		}
	}
}
