using Domovoy.Core.Enums;
using Domovoy.Core.Interfaces;
using Domovoy.Core.Interfaces.IRepository;
using Domovoy.Core.Models;
using System;
using System.Collections;

namespace Domovoy.Core.Services
{
	public class DeviceService : IDeviceService
	{
		private readonly IDeviceRepository _repository;
		private readonly INotificationService _notificationService;

		public DeviceService(IDeviceRepository repository, INotificationService notificationService)
		{
			_repository = repository;
			_notificationService = notificationService;
		}

		public bool TurnOnDevice(string deviceId, string initiatedBy)
		{
			try
			{
				var device = _repository.GetById(deviceId);
				if (device == null)
				{
					Console.WriteLine($"Устройство {deviceId} не найдено");
					return false;
				}

				if (device.Status == DeviceStatus.Error)
				{
					Console.WriteLine($"Устройство {deviceId} в состоянии ошибки");
					return false;
				}

				if (device.Status == DeviceStatus.Online)
				{
					Console.WriteLine($"Устройство {deviceId} уже включено");
					return true;
				}

				// Обновляем статус
				var oldStatus = device.Status;
				device.Status = DeviceStatus.Online;
				device.LastUpdated = DateTime.UtcNow;
				_repository.Update(device);

				// Уведомление
				_notificationService.SendDeviceStatusNotification(
					deviceId, oldStatus, DeviceStatus.Online, $"Включено пользователем {initiatedBy}");

				Console.WriteLine($"Устройство {device.Name} включено");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка включения устройства: {ex.Message}");
				return false;
			}
		}

		public bool TurnOffDevice(string deviceId, string initiatedBy)
		{
			try
			{
				var device = _repository.GetById(deviceId);
				if (device == null)
				{
					Console.WriteLine($"Устройство {deviceId} не найдено");
					return false;
				}

				if (device.Status == DeviceStatus.Error)
				{
					Console.WriteLine($"Устройство {deviceId} в состоянии ошибки");
					return false;
				}

				if (device.Status == DeviceStatus.Offline)
				{
					Console.WriteLine($"Устройство {deviceId} уже выключено");
					return true;
				}

				var oldStatus = device.Status;
				device.Status = DeviceStatus.Offline;
				device.LastUpdated = DateTime.UtcNow;
				_repository.Update(device);

				_notificationService.SendDeviceStatusNotification(
					deviceId, oldStatus, DeviceStatus.Offline, $"Выключено пользователем {initiatedBy}");

				Console.WriteLine($"Устройство {device.Name} выключено");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка выключения устройства: {ex.Message}");
				return false;
			}
		}

		public bool ToggleDevice(string deviceId, string initiatedBy)
		{
			var device = _repository.GetById(deviceId);
			if (device == null) return false;

			return device.Status == DeviceStatus.Online
				? TurnOffDevice(deviceId, initiatedBy)
				: TurnOnDevice(deviceId, initiatedBy);
		}

		public int TurnOffAllInLocation(string location, string initiatedBy)
		{
			int count = 0;
			var devices = _repository.GetByLocation(location);

			Console.WriteLine($"Выключение всех устройств в {location}...");

			foreach (DeviceData device in devices)
			{
				if (TurnOffDevice(device.Id, initiatedBy))
					count++;
			}

			Console.WriteLine($"Выключено {count} устройств в {location}");
			return count;
		}

		public Hashtable GetDeviceInfo(string deviceId)
		{
			var device = _repository.GetById(deviceId);
			if (device == null) return null;

			var info = new Hashtable();
			info["Id"] = device.Id;
			info["Name"] = device.Name;
			info["Location"] = device.Location;
			info["Type"] = device.Type.ToString();
			info["Status"] = device.Status.ToString();
			info["LastUpdated"] = device.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");

			return info;
		}

		public Hashtable GetStatistics()
		{
			var allDevices = _repository.GetAll();
			int total = allDevices.Count;
			int online = 0, offline = 0, error = 0;

			foreach (DeviceData device in allDevices)
			{
				if (device.Status == DeviceStatus.Online) online++;
				else if (device.Status == DeviceStatus.Offline) offline++;
				else if (device.Status == DeviceStatus.Error) error++;
			}

			var stats = new Hashtable();
			stats["Total"] = total;
			stats["Online"] = online;
			stats["Offline"] = offline;
			stats["Error"] = error;
			stats["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

			return stats;
		}

		public bool RenameDevice(string deviceId, string newName, string initiatedBy)
		{
			try
			{
				var device = _repository.GetById(deviceId);
				if (device == null) return false;

				var oldName = device.Name;
				device.Name = newName;
				device.LastUpdated = DateTime.UtcNow;
				_repository.Update(device);

				Console.WriteLine($"Устройство переименовано: {oldName} -> {newName}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка переименования: {ex.Message}");
				return false;
			}
		}

		public bool UpdateDeviceLocation(string deviceId, string newLocation, string initiatedBy)
		{
			try
			{
				var device = _repository.GetById(deviceId);
				if (device == null) return false;

				var oldLocation = device.Location;
				device.Location = newLocation;
				device.LastUpdated = DateTime.UtcNow;
				_repository.Update(device);

				Console.WriteLine($"Локация устройства изменена: {oldLocation} -> {newLocation}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка изменения локации: {ex.Message}");
				return false;
			}
		}
	}
}