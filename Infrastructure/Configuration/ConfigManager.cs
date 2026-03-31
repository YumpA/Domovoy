using Domovoy;
using System;
using System.Text;
using System.IO;
using nanoFramework.Json;
using System.Collections;

namespace Infrastructure.Configuration
{
	public class ConfigManager
	{
		private const string ConfigfilePath = "I:\\config.json";

		public static AppConfig Load()
		{
			try
			{
				if (!File.Exists(ConfigfilePath))
				{
					Console.WriteLine("Config file not found, creating default.");
					var defaultConfig = CreateDefaultConfig();
					Save(defaultConfig);
					return defaultConfig;
				}

				string json = File.ReadAllText(ConfigfilePath);
				//Console.WriteLine(json);

				//проблема с десерализацией, нужно исправить
				var config = (AppConfig)JsonConvert.DeserializeObject(json, typeof(AppConfig));
				return CreateDefaultConfig();
			}

			catch (Exception ex)
			{
				Console.WriteLine($"Failed to load config: {ex.Message}");
				return CreateDefaultConfig();
			}
		}

		private static void Save(AppConfig config)
		{
			try
			{
				string json = JsonSerializer.SerializeObject(config);
				File.WriteAllText(ConfigfilePath, json);
				Console.WriteLine("Config saved.");
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Failed to save config: {ex.Message}");
			}
		}

		private static AppConfig CreateDefaultConfig()
		{
			var devices = new ArrayList
			{
				new DeviceConfig
				{
					Id = "light_living_room",
					Name = "Основной свет гостиной",
					Location = "Гостиная",
					Type = "Switch",
					Pin = 21
				},
				 new DeviceConfig
				{
					Id = "light_bedroom",
					Name = "Свет спальни",
					Location = "Спальня",
					Type = "Switch",
					Pin = 22
				}
			};

			return new AppConfig
			{
				Wifi = new WifiConfig { Ssid = "Intersvyaz_AB8E", Password = "34520291" },
				Mqtt = new MqttConfig(),
				Devices = devices
			};
		}
	}
}
