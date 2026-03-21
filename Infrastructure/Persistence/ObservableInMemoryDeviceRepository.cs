using Domovoy.Core.Enums;
using Domovoy.Core.Models;
using Domovoy.Events;
using Domovoy.Interfaces;
using System;
using System.Collections;
using System.Text;

namespace Infrastructure.Persistence
{
	public class ObservableInMemoryDeviceRepository : IObservableRepository
	{
		private readonly Hashtable _devices = new Hashtable();

		public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

		public void Add(DeviceData device)
		{
			if (device == null) throw new ArgumentNullException(nameof(device));
			if (_devices.Contains(device.Id))
				throw new InvalidOperationException("Устройство с таким Id уже существует");

			_devices[device.Id] = device;
			//здесь тоже можно будет вызвать событие при добавлении
		}

		public void Delete(string deviceId)
		{
			_devices.Remove(deviceId);
		}

		public bool Exist(string deviceId)
		{
			return _devices.Contains(deviceId);
		}

		public IList GetAll()
		{
			var result = new ArrayList();
			foreach (DictionaryEntry entry in _devices)			
				result.Add(entry.Value);
			return result;
		}

		public DeviceData GetById(string deviceId)
		{
			return (DeviceData)_devices[deviceId];
		}

		public IList GetByLocation(string location)
		{
			var result = new ArrayList();
			foreach(DictionaryEntry entry in _devices)
			{
				var device = (DeviceData)entry.Value;
				if (device.Location == location)
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

		public int GetCount()
		{
			return _devices.Count;
		}

		public void Update(DeviceData device)
		{
			if (device == null) throw new ArgumentNullException(nameof(device));

			var existing = (DeviceData)_devices[device.Id];
			if (existing == null)
				throw new InvalidOperationException("Устройство не найдено");

			//генерация и сравнение событий
			if (existing.Name != device.Name)
				OnDeviceChanged(device.Id, nameof(DeviceData.Name), existing.Name, device.Name);
			if (existing.Location != device.Location)
				OnDeviceChanged(device.Id, nameof(DeviceData.Location), existing.Name, device.Name);
			if (existing.Status != device.Status)
				OnDeviceChanged(device.Id, nameof(DeviceData.Status), existing.Name, device.Name);

			//Копируем поля, Id и CreatedAt не меняются
			existing.Name = device.Name;
			existing.Location = device.Location;
			existing.Status = device.Status;
			existing.LastUpdated = device.LastUpdated;

			//не меняю объект в hashtable, тк обновил существующий
		}

		//метод для вызова ивента
		protected virtual void OnDeviceChanged(string deviceId, string propertyName, object oldValue, object newValue)
		{
			DeviceChanged?.Invoke(this, new DeviceChangedEventArgs(deviceId, propertyName, oldValue, newValue));
		}
	}
}
