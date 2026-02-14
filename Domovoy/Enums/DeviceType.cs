using System;

namespace Domovoy.Core.Enums
{
	/// <summary>
	/// Типы устройств, которыми управляет Домовой
	/// </summary>
	public enum DeviceType
	{
		Unknown = 0,
		Switch,         // Выключатель
		Dimmer,         // Диммер
		Sensor,         // Датчик
		Thermostat,     // Термостат
		Lock,           // Замок
		Camera,         // Камера
		Speaker,        // Колонка
		Display,        // Дисплей
		Composite       // Композитное устройство
	}
}
