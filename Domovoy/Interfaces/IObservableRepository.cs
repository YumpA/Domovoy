using Domovoy.Core.Interfaces.IRepository;
using Domovoy.Events;
using System;
using System.Text;

namespace Domovoy.Interfaces
{
	public interface IObservableRepository : IDeviceRepository
	{
		//ивент срабатывает при любом изменении ус-ва
		event EventHandler<DeviceChangedEventArgs> DeviceChanged;
		//позже добавить более специфичные события
	}
}
