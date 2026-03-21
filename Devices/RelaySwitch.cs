using System;
using System.Device.Gpio;
using Domovoy.Core;
using Domovoy.Core.Enums;
using Domovoy.Core.Interfaces;
using Domovoy.Core.Models;
using Domovoy.Events;
using Domovoy.Interfaces;

namespace Domovoy.Devices
{
	public class RelaySwitch : ISwitchable
	{
		private readonly string _id;
		private readonly string _name;
		private readonly string _location;
		private readonly GpioController _gpio;
		private readonly int _pin;

		private bool _isInitialized = false;
		private bool _isOn = false;
		private DeviceStatus _status = DeviceStatus.Offline;

		//ссылка для подписки
		private readonly IObservableRepository _repository;

		public string Id => _id;
		public string Name => _name;
		public string Location => _location;
		public DeviceType Type => DeviceType.Switch;
		public DeviceStatus Status => _status;
		public bool IsOn => _isOn;

		public RelaySwitch(string id, string name, string location, int pin, IObservableRepository repository)
		{
			_id = id ?? throw new ArgumentNullException(nameof(id));
			_name = name ?? $"Реле {id}";
			_location = location ?? "Неизвестно";
			_pin = pin;
			_repository = repository;
			_gpio = new GpioController();
		}

		public bool Initialize()
		{
			try
			{
				if (_isInitialized) return true;

				Console.WriteLine($"Инициализация реле '{_name}' на пине {_pin}...");

				_gpio.OpenPin(_pin, PinMode.Output);
				//_gpio.Write(_pin, PinValue.High);
				TurnOffInternal();

				_isInitialized = true;
				_status = DeviceStatus.Online;

				//подписка
				_repository.DeviceChanged += OnDeviceChanged;

				Console.WriteLine($"✓ Реле '{_name}' готово");
				return true;
			}
			catch (Exception ex)
			{
				_status = DeviceStatus.Error;
				Console.WriteLine($"✗ Ошибка инициализации: {ex.Message}");
				return false;
			}
		}

		public bool Shutdown()
		{
			try
			{
				if (!_isInitialized) return true;

				Console.WriteLine($"Выключение реле '{_name}'...");

				//отписка
				_repository.DeviceChanged -= OnDeviceChanged;

				TurnOffInternal();
				_gpio.ClosePin(_pin);

				_isInitialized = false;
				_status = DeviceStatus.Offline;

				Console.WriteLine($"✓ Реле '{_name}' выключено");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"✗ Ошибка выключения: {ex.Message}");
				return false;
			}
		}

		private void OnDeviceChanged(object sender, DeviceChangedEventArgs e)
		{
			//если ивент не про текущее ус-во, игнор
			if (e.DeviceId != _id)
				return;

			if (e.PropertyName == nameof(DeviceData.Status))
			{
				DeviceStatus newStatus = (DeviceStatus)e.NewValue;
				if (newStatus == DeviceStatus.Online)
				{
					Console.WriteLine($"[СОБЫТИЕ] Реле '{_name}' получило команду включиться");
					TurnOnInternal();
				}
				else if (newStatus == DeviceStatus.Offline)
				{
					Console.WriteLine($"[СОБЫТИЕ] Реле '{_name}' получило команду выключиться");
					TurnOffInternal();
				}
			}
		}

		public bool TurnOn()
		{
			if (!_isInitialized || _status != DeviceStatus.Online)
			{
				Console.WriteLine($"Реле '{_name}' не готово к работе");
				return false;
			}

			try
			{
				TurnOnInternal();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"✗ Ошибка включения: {ex.Message}");
				return false;
			}
		}

		public bool TurnOff()
		{
			if (!_isInitialized)
			{
				Console.WriteLine($"Реле '{_name}' не инициализировано");
				return false;
			}

			try
			{
				TurnOffInternal();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"✗ Ошибка выключения: {ex.Message}");
				return false;
			}
		}

		public bool Toggle()
		{
			if (!_isInitialized) return false;
			return _isOn ? TurnOff() : TurnOn();
		}

		private void TurnOffInternal()
		{
			if (!_isOn)
			{
				//Console.WriteLine(_pin.ToString());
				_gpio.Write(_pin, PinValue.Low);
				_isOn = false;
				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] ✓ {_name}: ВЫКЛ");
			}
		}

		private void TurnOnInternal()
		{
			if (_isOn) return;
	
			_gpio.Write(_pin, PinValue.High);
			_isOn = true;
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] ✓ {_name}: ВКЛ");			
		}
	}
}