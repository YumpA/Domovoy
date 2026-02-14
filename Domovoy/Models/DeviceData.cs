using Domovoy.Core.Enums;
using System;

namespace Domovoy.Core.Models
{
	/// <summary>
	/// Модель устройства с данными без поведения
	/// </summary>
	public class DeviceData
	{
		//публичные свойства
		public string Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public DeviceType Type { get; set; }
		public DeviceStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime LastUpdated { get; set; }

		//конструктор по умолчанию для сериализации
		public DeviceData() { }

		public DeviceData(string id, string name, DeviceType type, string location)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Name = name ?? throw new ArgumentNullException(nameof(name));			
			Type = type;
			Location = location ?? "Неизвестно";
			Status = DeviceStatus.Offline;
			CreatedAt = DateTime.UtcNow;
			LastUpdated = DateTime.UtcNow;
		}
	}
}
