using Domovoy.Core;
using Domovoy.Core.Enums;
using Domovoy.Core.Interfaces;
using Domovoy.Core.Interfaces.IRepository;
using Domovoy.Core.Models;
using Domovoy.Core.Services;
using Domovoy.Infrastructure;
using Domovoy.Infrastructure.Web;
using Infrastructure;
using Infrastructure.Network;
using Infrastructure.Web;
using Microsoft.Extensions.DependencyInjection;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;
using RelaySwitch = Domovoy.Devices.RelaySwitch;

namespace Domovoy.Firmware
{
	public class Program
	{
		private static IObservableRepository _repository;
		private static INotificationService _notificationService;
		private static IDeviceService _deviceService;
		private static RelaySwitch _livingRoomLight;
		private static RelaySwitch _bedroomLight;

		private static IServiceProvider _serviceProvider;
		private static WebServer _webServer;

		public static void Main()
		{
			Console.WriteLine("\n╔══════════════════════════════╗");
			Console.WriteLine("║     ДОМОВОЙ v0.2.2          ║");
			Console.WriteLine("║   Многослойная архитектура  ║");
			Console.WriteLine("╚══════════════════════════════╝\n");

			try
			{
				InitializeSystem();
				RunMainLoop();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n!!! КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
				EmergencyShutdown();
			}
		}

		private static void InitializeSystem()
		{
			Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ СИСТЕМЫ ===");

			string ssid = "";
			string password = "";

			if (!WifiHelper.ConnectToWifi(ssid, password)) 
			{
				Console.WriteLine("Только локальное управление");
			}

			var devices = new Hashtable();

			// 1. Сервисы инфраструктуры

			_repository = new InMemoryDeviceRepository();
			_notificationService = new ConsoleNotificationService();			

			Console.WriteLine("✓ Сервисы инициализированы");

			// 2. Создаем устройства
			_livingRoomLight = new RelaySwitch(
				"light_living_room",
				"Основной свет гостиной",
				"Гостиная",
				21);

			_bedroomLight = new RelaySwitch(
				"light_bedroom",
				"Свет спальни",
				"Спальня",
				22);

			devices[_livingRoomLight.Id] = _livingRoomLight;
			devices[_bedroomLight.Id] = _bedroomLight;

			_deviceService = new DeviceService(_repository, _notificationService, devices);

			_serviceProvider=new ServiceCollection()
				.AddSingleton(typeof(IDeviceRepository), _repository)
				.AddSingleton(typeof(INotificationService), _notificationService)
				.AddSingleton(typeof(IDeviceService), _deviceService)
				.AddSingleton(typeof(RelaySwitch), _livingRoomLight)
				.AddTransient(typeof(ApiController))
				.BuildServiceProvider();

			// 3. Регистрируем устройства в репозитории
			RegisterDevice(_livingRoomLight);
			RegisterDevice(_bedroomLight);

			// 4. Инициализируем устройства
			InitializeDevice(_livingRoomLight);
			InitializeDevice(_bedroomLight);

			Console.WriteLine("\n=== СИСТЕМА ГОТОВА ===");
			PrintSystemStatus();

			StartWebServer();
		}

		private static void StartWebServer()
		{
			try
			{
				//создаю веб-сервер и указываю, где посмотреть роуты, конкретно здесь - ApiController
				_webServer = 
					new WebServerDi(80, HttpProtocol.Http, new Type[] {typeof(ApiController)}, _serviceProvider);

				//_webServer.CommandReceived += ServerCommandReceived;

				_webServer.Start();
				Console.WriteLine($"✓ Веб-сервер запущен на порту 80");

				Thread.Sleep(Timeout.Infinite);
			}

			catch (Exception ex)
			{
				Console.WriteLine($"✗ Ошибка запуска веб-сервера: {ex.Message}");
			}
		}

		private static void ServerCommandReceived(object source, WebServerEventArgs e)
		{
			//сделал простой вариант с ивентом и ручными роутами, позже поменяю на DI

			string url = e.Context.Request.RawUrl;
			//Console.WriteLine(url);
			string[] parts = url.Split('/');

			if (url.Length == 4)
				WebServer.OutputAsStream(e.Context.Response, "{\"error\":\"Invalid URL format\"}");

			string command = parts[2]; // /on/off/toggle/status
			string deviceId = parts[3]; // /api/{command}/{deviceId}

			bool result = false;
			string message = "";

			switch (command.ToLower())
			{
				case "on":
					result = _deviceService.TurnOnDevice(deviceId, "web_user");
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
					{
						message = JsonHelper.Serialize(info);
						break;
					}
					else
					{
						message = "Ошибка получения информации";
						break;
					}	

				default:
					WebServer.OutputAsStream(e.Context.Response, "{\"error\":\"Неизвестная команда\"}");
					break;
			}

			string responce = $"{{\"success\":{result.ToString().ToLower()},\"message\":\"{message}\"}}";
			WebServer.OutputAsStream(e.Context.Response, responce);
		}

		private static void RegisterDevice(IDevice device)
		{
			var deviceData = new DeviceData
			{
				Id = device.Id,
				Name = device.Name,
				Location = device.Location,
				Type = device.Type,
				Status = DeviceStatus.Offline,
				CreatedAt = DateTime.UtcNow,
				LastUpdated = DateTime.UtcNow
			};

			_repository.Add(deviceData);
			Console.WriteLine($"✓ Зарегистрировано: {device.Name}");
		}

		private static void InitializeDevice(IDevice device)
		{
			device.Initialize();

			if (device.Initialize())
			{
				// Обновляем статус в репозитории
				var data = _repository.GetById(device.Id);
				if (data != null)
				{
					//data.Status = DeviceStatus.Online;
					data.LastUpdated = DateTime.UtcNow;
					_repository.Update(data);
				}

				_notificationService.SendDeviceOnlineNotification(device.Id);
			}
			else
			{
				_notificationService.SendDeviceErrorNotification(device.Id, "Ошибка инициализации");
			}
		}

		private static void RunMainLoop()
		{
			Console.WriteLine("\n=== ЗАПУСК ОСНОВНОГО ЦИКЛА ===");
			Console.WriteLine("Система будет тестировать устройства каждые 5 секунд\n");

			int cycle = 0;

			while (true)
			{
				cycle++;
				Console.WriteLine($"\n───── ЦИКЛ #{cycle} ─────");

				// Тест через DeviceService (бизнес-логика)
				//TestWithDeviceService();

				// Тест напрямую через устройства
				//TestDirectDeviceControl();

				// Показываем статистику каждые 3 цикла
				if (cycle % 3 == 0)
				{
					ShowStatistics();
				}

				// Ждем 5 секунд
				Thread.Sleep(5000);
			}
		}

		private static void TestWithDeviceService()
		{
			Console.WriteLine("\n[Тест через DeviceService]");

			// Включаем свет в гостиной
			
			bool success = _deviceService.TurnOnDevice("light_living_room", "system");
			Console.WriteLine($"Включение гостиной: {(success ? "УСПЕХ" : "НЕУДАЧА")}");

			Thread.Sleep(1000);

			// Выключаем
			success = _deviceService.TurnOffDevice("light_living_room", "system");
			Console.WriteLine($"Выключение гостиной: {(success ? "УСПЕХ" : "НЕУДАЧА")}");

			// Информация об устройстве
			var info = _deviceService.GetDeviceInfo("light_living_room");
			if (info != null)
			{
				Console.WriteLine($"Инфо: {info["Name"]} - {info["Status"]}");
			}
		}

		private static void TestDirectDeviceControl()
		{
			Console.WriteLine("\n[Тест прямого управления]");

			// Переключаем свет в спальне
			_bedroomLight.Toggle();
			Thread.Sleep(800);
			_bedroomLight.Toggle();
		}

		private static void ShowStatistics()
		{
			Console.WriteLine("\n[Статистика системы]");

			var stats = _deviceService.GetStatistics();
			if (stats != null)
			{
				Console.WriteLine($"Устройств всего: {stats["Total"]}");
				Console.WriteLine($"  Онлайн: {stats["Online"]}");
				Console.WriteLine($"  Офлайн: {stats["Offline"]}");
				Console.WriteLine($"  Ошибки: {stats["Error"]}");
				Console.WriteLine($"Время: {stats["Timestamp"]}");
			}
		}

		private static void PrintSystemStatus()
		{
			var devices = _repository.GetAll();
			Console.WriteLine($"\nЗарегистрировано устройств: {devices.Count}");

			foreach (DeviceData device in devices)
			{
				string statusIcon = device.Status switch
				{
					DeviceStatus.Online => "🟢",
					DeviceStatus.Offline => "⚪",
					DeviceStatus.Error => "🔴",
					_ => "❓"
				};

				Console.WriteLine($"  {statusIcon} {device.Name} ({device.Location})");
			}
		}

		private static void EmergencyShutdown()
		{
			Console.WriteLine("\n!!! Аварийное выключение !!!");

			try
			{
				if (_livingRoomLight != null) _livingRoomLight.Shutdown();
				if (_bedroomLight != null) _bedroomLight.Shutdown();
				Console.WriteLine("Устройства выключены");
			}
			catch
			{
				Console.WriteLine("Не удалось безопасно выключить устройства");
			}

			// Бесконечный цикл с миганием светодиода (пин 2)
			var gpio = new GpioController();
			int ledPin = 21;

			try
			{
				gpio.OpenPin(ledPin, PinMode.Output);

				while (true)
				{
					gpio.Write(ledPin, PinValue.High);
					Thread.Sleep(100);
					gpio.Write(ledPin, PinValue.Low);
					Thread.Sleep(900);
				}
			}
			catch
			{
				// Если не удалось - просто спим
				Thread.Sleep(Timeout.Infinite);
			}
		}
	}
}