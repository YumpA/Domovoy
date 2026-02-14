using System;

namespace Domovoy.Core.Enums
{
	/// <summary>
	/// Состояние устройства
	/// </summary>
	public enum DeviceStatus
	{
		Offline = 0,
		Online,
		Error,
		Updating,
		Sleeping
	}
}
