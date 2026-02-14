// Program.cs - полный упрощенный проект
using Domovoy.Core.Enums;
using Domovoy.Core.Models;
using System;
using System.Device.Gpio;
using System.Threading;

namespace Domovoy.Core
{
	// Простые модели

	// Простой репозиторий
	public class DeviceRepository
	{
		private DeviceData[] _devices = new DeviceData[10];
		private int _count = 0;

		public void Add(DeviceData device)
		{
			if (_count < _devices.Length)
			{
				_devices[_count] = device;
				_count++;
			}
		}

		public DeviceData GetById(string id)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_devices[i].Id == id)
					return _devices[i];
			}
			return null;
		}
	}

	// Простой сервис уведомлений
	public class NotificationService
	{
		public void Send(string message)
		{
			Console.WriteLine("Уведомление: " + message);
		}
	}

	// Основной класс
	public class RelaySwitch
	{
		private GpioController _gpio;
		private int _pin;
		private DeviceData _data;

		public RelaySwitch(string id, string name, int pin)
		{
			_data = new DeviceData
			{
				Id = id,
				Name = name,
				Status = DeviceStatus.Offline
			};

			_pin = pin;
			_gpio = new GpioController();
		}

		public bool Initialize()
		{
			try
			{
				_gpio.OpenPin(_pin, PinMode.Output);
				_gpio.Write(_pin, PinValue.Low);
				_data.Status = DeviceStatus.Online;
				_data.LastUpdated = DateTime.UtcNow;
				return true;
			}
			catch
			{
				_data.Status = DeviceStatus.Error;
				return false;
			}
		}

		public void TurnOn()
		{
			_gpio.Write(_pin, PinValue.High);
			_data.LastUpdated = DateTime.UtcNow;
			Console.WriteLine(_data.Name + " ВКЛ");
		}

		public void TurnOff()
		{
			_gpio.Write(_pin, PinValue.Low);
			_data.LastUpdated = DateTime.UtcNow;
			Console.WriteLine(_data.Name + " ВЫКЛ");
		}
	}

	// Главная программа
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine("=== ДОМОВОЙ ===");
			Console.WriteLine("Простая версия");

			// Создаем реле
			var light = new RelaySwitch("light1", "Свет в гостиной", 21);

			if (light.Initialize())
			{
				Console.WriteLine("Устройство инициализировано");

				// Простой тест
				for (int i = 0; i < 3; i++)
				{
					light.TurnOn();
					Thread.Sleep(1000);
					light.TurnOff();
					Thread.Sleep(1000);
				}

				Console.WriteLine("Тест завершен");
			}
			else
			{
				Console.WriteLine("Ошибка инициализации");
			}

			// Бесконечный цикл
			while (true)
			{
				Thread.Sleep(5000);
				Console.WriteLine("Система работает: " + DateTime.UtcNow.ToString("HH:mm:ss"));
			}
		}
	}
}