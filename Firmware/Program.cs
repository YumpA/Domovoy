using System;
using System.Device.Gpio;
using System.Threading;
using Domovoy.Core;
using Domovoy.Core.Enums;
using Domovoy.Core.Interfaces;
using Domovoy.Core.Interfaces.IRepository;
using Domovoy.Core.Models;
using Domovoy.Core.Services;
using Domovoy.Infrastructure;
using Infrastructure;
using RelaySwitch = Domovoy.Devices.RelaySwitch;

namespace Domovoy.Firmware
{
	public class Program
	{
		private static IDeviceRepository _repository;
		private static INotificationService _notificationService;
		private static IDeviceService _deviceService;
		private static RelaySwitch _livingRoomLight;
		private static RelaySwitch _bedroomLight;

		public static void Main()
		{
			/*// ============ ДИАГНОСТИКА РЕЛЕ ============
			Console.WriteLine("=== ДИАГНОСТИКА РЕЛЕ ===");

			// Укажите правильный номер пина, к которому подключено реле
			int testPin = 21;   // ПОМЕНЯЙТЕ, если используете другой пин

			using (var gpio = new GpioController())
			{
				try
				{
					gpio.OpenPin(testPin, PinMode.Output);
					Console.WriteLine($"Пин {testPin} открыт.");

					// Тест 1: HIGH -> ждём -> LOW
					Console.WriteLine("Устанавливаем HIGH (попытка 1)...");
					gpio.Write(testPin, PinValue.High);
					Thread.Sleep(2000);
					Console.WriteLine("Устанавливаем LOW...");
					gpio.Write(testPin, PinValue.Low);
					Thread.Sleep(1000);

					// Тест 2: LOW -> ждём -> HIGH (если реле работает инверсно)
					Console.WriteLine("Устанавливаем LOW (попытка 2)...");
					gpio.Write(testPin, PinValue.Low);
					Thread.Sleep(2000);
					Console.WriteLine("Устанавливаем HIGH...");
					gpio.Write(testPin, PinValue.High);
					Thread.Sleep(1000);

					// Тест 3: быстрое мигание
					Console.WriteLine("Мигаем 3 раза...");
					for (int i = 0; i < 3; i++)
					{
						gpio.Write(testPin, PinValue.High);
						Thread.Sleep(200);
						gpio.Write(testPin, PinValue.Low);
						Thread.Sleep(200);
					}

					gpio.ClosePin(testPin);
					Console.WriteLine("Диагностика завершена.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при работе с пином {testPin}: {ex.Message}");
				}
			}
			// ============ КОНЕЦ ДИАГНОСТИКИ ============*/

			Console.WriteLine("\n╔══════════════════════════════╗");
			Console.WriteLine("║     ДОМОВОЙ v0.2.0          ║");
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

			// 1. Сервисы инфраструктуры
			_repository = new InMemoryDeviceRepository();
			_notificationService = new ConsoleNotificationService();
			_deviceService = new DeviceService(_repository, _notificationService);

			Console.WriteLine("✓ Сервисы инициализированы");

			// 2. Создаем устройства
			_livingRoomLight = new RelaySwitch(
				"light_living_room",
				"Основной свет гостиной",
				"Гостиная",
				22);

			_bedroomLight = new RelaySwitch(
				"light_bedroom",
				"Свет спальни",
				"Спальня",
				21);

			// 3. Регистрируем устройства в репозитории
			RegisterDevice(_livingRoomLight);
			RegisterDevice(_bedroomLight);

			// 4. Инициализируем устройства
			InitializeDevice(_livingRoomLight);
			InitializeDevice(_bedroomLight);

			Console.WriteLine("\n=== СИСТЕМА ГОТОВА ===");
			PrintSystemStatus();
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
					data.Status = DeviceStatus.Online;
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
				TestWithDeviceService();

				// Тест напрямую через устройства
				TestDirectDeviceControl();

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