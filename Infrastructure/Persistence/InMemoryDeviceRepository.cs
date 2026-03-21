using Domovoy.Core.Enums;
using Domovoy.Core.Models;
using Domovoy.Core.Interfaces.IRepository;
using System;
using System.Collections;

namespace Infrastructure.Persistence
{
	public class InMemoryDeviceRepository : IDeviceRepository
	{
		private readonly Hashtable _devices = new Hashtable();

		public DeviceData GetById(string deviceId)
		{
			/*foreach (DeviceData device in _devices)
			{
				if (device.Id == deviceId)
					return device;
			}
			return null;*/

			return (DeviceData)_devices[deviceId];
		}

		public IList GetAll()
		{
			var result = new ArrayList();

			/*foreach (DeviceData device in _devices)
			{
				result.Add(device);
			}*/

			foreach (DictionaryEntry entry in _devices)
			{
				result.Add(entry.Value);
			}

			return result;
		}

		public void Add(DeviceData device)
		{
			if (device==null)
				throw new ArgumentNullException(nameof(device));

			if (_devices.Contains(device.Id))
				throw new InvalidOperationException($"Устройство уже существует");

			_devices[device.Id] = device;
		}

		public void Update(DeviceData device)
		{
			if (device == null)
				throw new ArgumentNullException(nameof(device));

			/*for (int i = 0; i < _devices.Count; i++)
			{
				var existingDevice = (DeviceData)_devices[i];
				if (existingDevice.Id==device.Id)
				{
					_devices[i] = device;
					return;
				}
			}*/

			_devices[device.Id] = device;

			if(!_devices.Contains(device.Id))
				throw new InvalidOperationException($"Устройство не найдено");
		}

		public void Delete(string deviceId)
		{
			_devices.Remove(deviceId);

			/*for (int i = 0; i < _devices.Count; i++)
			{
				var device = (DeviceData)_devices[i];
				if (device.Id == deviceId)
				{
					_devices.RemoveAt(i);
					return;
				}
			}*/
		}

		public IList GetByLocation(string location)
		{
			var result = new ArrayList();
			foreach (DictionaryEntry entry in _devices)
			{
				var device = (DeviceData)entry.Value;
				if(device.Location==location)
					result.Add(device);
			}
			return result;
		}

		public IList GetByType(DeviceType type)
		{
			var result = new ArrayList();
			foreach (DictionaryEntry entry in _devices)
			{
				var device = (DeviceData)entry.Value;
				if (device.Type == type)
					result.Add(device);
			}

			return result;
		}

		public IList GetByStatus(DeviceStatus status)
		{
			var result = new ArrayList();

			foreach (DictionaryEntry entry in _devices)
			{
				var device = (DeviceData)entry.Value;
				if (device.Status == status)
					result.Add(device);
			}

			return result;
		}

		public bool Exist(string deviceId)
		{
			return _devices.Contains(deviceId);
		}		

		public int GetCount()
		{
			return _devices.Count;
		}
	}
}
