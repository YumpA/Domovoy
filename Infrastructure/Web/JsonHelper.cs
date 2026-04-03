using Domovoy;
using System;
using System.Collections;
using System.Text;

namespace Infrastructure.Web
{
	public class JsonHelper
	{
		public static string Serialize(object obj) 
		{
			if (obj == null) return "null";

			if (obj is string str) return $"\"{EscapeString(str)}\"";

			if (obj is int || obj is long || obj is float || obj is double) return obj.ToString();

			if (obj is bool b) return b ? "true" : "false";

			if (obj is DateTime dt) return $"\"{dt:yyyy-MM-dd HH:mm:ss}\"";

			if (obj is Hashtable ht)
			{
				StringBuilder sb = new StringBuilder("{");
				bool first = true;
				foreach(DictionaryEntry entry in ht)
				{
					if (!first) sb.Append(",");
					sb.Append($"\"{entry.Key}\":{Serialize(entry.Value)}");
					first = false;
				}
				sb.Append("}");
				return sb.ToString();
			}

			if (obj is IEnumerable enumerable)
			{
				StringBuilder sb = new StringBuilder("[");
				bool first = true;
				foreach (var item in enumerable)
				{
					if (!first) sb.Append(",");
					sb.Append(Serialize(item));
					first = false;
				}
				sb.Append("]");
				return sb.ToString();
			}

			return $"\"{obj}\"";
		}

		private static string EscapeString(string s)
		{			
			if (string.IsNullOrEmpty(s)) return s;
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];

				if (c == '\\')
				{
					sb.Append("\\\\"); // экранируем обратный слеш
				}
				else if (c == '"')
				{
					sb.Append("\\\""); // экранируем кавычку
				}
				else
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public static AppConfig DeserializeAppConfig(string json)
		{
			if (string.IsNullOrEmpty(json))
				throw new ArgumentException("JSON is empty");

			var config = new AppConfig();

			// Убираю пробелы и переводы строк для упрощения
			json = json.Trim();

			// Ищу объект верхнего уровня { ... }
			int start = json.IndexOf('{');
			int end = json.LastIndexOf('}');
			if (start == -1 || end == -1) throw new FormatException("Invalid JSON: no outer braces");
			string content = json.Substring(start + 1, end - start - 1);

			// Разбиваю на пары "ключ": значение, но учитывая вложенные объекты и массивы
			// Упрощаю: ищу корневые ключи "Wifi", "Mqtt", "Devices"

			config.Wifi = ParseWifi(ExtractObject(content, "Wifi"));
			config.Mqtt = ParseMqtt(ExtractObject(content, "Mqtt"));
			config.Devices = ParseDevices(ExtractArray(content, "Devices"));

			return config;
		}

		private static string ExtractObject(string jsonContent, string key)
		{
			// Ищу "key": { ... }
			string pattern = "\"" + key + "\"\\s*:\\s*\\{";
			int start = FindPattern(jsonContent, pattern);
			if (start == -1) return null;

			// Ищу закрывающую скобку, учитывая вложенность
			int braceCount = 0;
			int i = start + pattern.Length - 1;
			for (; i < jsonContent.Length; i++)
			{
				char c = jsonContent[i];
				if (c == '{') braceCount++;
				else if (c == '}') braceCount--;
				if (braceCount == 0) break;
			}
			return jsonContent.Substring(start, i - start + 1);
		}

		private static string ExtractArray(string jsonContent, string key)
		{
			string pattern = "\"" + key + "\"\\s*:\\s*\\[";
			int start = FindPattern(jsonContent, pattern);
			if (start == -1) return null;

			int bracketCount = 0;
			int i = start + pattern.Length - 1;
			for (; i < jsonContent.Length; i++)
			{
				char c = jsonContent[i];
				if (c == '[') bracketCount++;
				else if (c == ']') bracketCount--;
				if (bracketCount == 0) break;
			}
			return jsonContent.Substring(start, i - start + 1);
		}

		private static int FindPattern(string text, string pattern)
		{
			// Простой поиск подстроки
			for (int i = 0; i <= text.Length - pattern.Length; i++)
			{
				bool match = true;
				for (int j = 0; j < pattern.Length; j++)
				{
					if (text[i + j] != pattern[j])
					{
						match = false;
						break;
					}
				}
				if (match) return i;
			}
			return -1;
		}

		private static WifiConfig ParseWifi(string wifiJson)
		{
			if (string.IsNullOrEmpty(wifiJson)) return new WifiConfig();

			string content = wifiJson.Trim();
			content = content.Substring(content.IndexOf('{') + 1, content.LastIndexOf('}') - content.IndexOf('{') - 1);

			var wifi = new WifiConfig();
			wifi.Ssid = ExtractStringValue(content, "Ssid");
			wifi.Password = ExtractStringValue(content, "Password");
			return wifi;
		}

		private static MqttConfig ParseMqtt(string mqttJson)
		{
			if (string.IsNullOrEmpty(mqttJson)) return new MqttConfig();

			string content = mqttJson.Trim();
			content = content.Substring(content.IndexOf('{') + 1, content.LastIndexOf('}') - content.IndexOf('{') - 1);

			var mqtt = new MqttConfig();
			mqtt.Broker = ExtractStringValue(content, "Broker");
			mqtt.Port = ExtractIntValue(content, "Port");
			mqtt.ClientId = ExtractStringValue(content, "ClientId");
			mqtt.UseTls = ExtractBoolValue(content, "UseTls");
			return mqtt;
		}

		private static ArrayList ParseDevices(string devicesJson)
		{
			var list = new ArrayList();
			if (string.IsNullOrEmpty(devicesJson)) return list;

			// Убираю внешние скобки массива
			string content = devicesJson.Trim();
			content = content.Substring(content.IndexOf('[') + 1, content.LastIndexOf(']') - content.IndexOf('[') - 1);

			// Разбиваю на отдельные объекты (между объектами разделитель "},")
			int start = 0;
			while (start < content.Length)
			{
				int objStart = content.IndexOf('{', start);
				if (objStart == -1) break;

				int braceCount = 0;
				int objEnd = objStart;
				for (int i = objStart; i < content.Length; i++)
				{
					char c = content[i];
					if (c == '{') braceCount++;
					else if (c == '}') braceCount--;
					if (braceCount == 0) { objEnd = i; break; }
				}

				string deviceJson = content.Substring(objStart, objEnd - objStart + 1);
				var device = ParseDevice(deviceJson);
				if (device != null) list.Add(device);

				start = objEnd + 1;
			}
			return list;
		}

		private static DeviceConfig ParseDevice(string deviceJson)
		{
			string content = deviceJson.Trim();
			content = content.Substring(content.IndexOf('{') + 1, content.LastIndexOf('}') - content.IndexOf('{') - 1);

			var dev = new DeviceConfig();
			dev.Id = ExtractStringValue(content, "Id");
			dev.Name = ExtractStringValue(content, "Name");
			dev.Location = ExtractStringValue(content, "Location");
			dev.Type = ExtractStringValue(content, "Type");
			dev.Pin = ExtractIntValue(content, "Pin");
			return dev;
		}

		private static string ExtractStringValue(string jsonContent, string key)
		{
			string pattern = "\"" + key + "\"\\s*:\\s*\"";
			int start = FindPattern(jsonContent, pattern);
			if (start == -1) return null;

			start += pattern.Length;
			int end = jsonContent.IndexOf('"', start);
			if (end == -1) return null;
			return jsonContent.Substring(start, end - start);
		}

		private static int ExtractIntValue(string jsonContent, string key)
		{
			string pattern = "\"" + key + "\"\\s*:\\s*";
			int start = FindPattern(jsonContent, pattern);
			if (start == -1) return 0;

			start += pattern.Length;
			int end = start;
			while (end < jsonContent.Length && (jsonContent[end] == '-'))
				end++;
			string numStr = jsonContent.Substring(start, end - start);
			if (int.TryParse(numStr, out int val)) return val;
			return 0;
		}

		private static bool ExtractBoolValue(string jsonContent, string key)
		{
			string pattern = "\"" + key + "\"\\s*:\\s*";
			int start = FindPattern(jsonContent, pattern);
			if (start == -1) return false;

			start += pattern.Length;
			if (jsonContent.IndexOf("true", start) == start) return true;
			if (jsonContent.IndexOf("false", start) == start) return false;
			return false;
		}

		public static Hashtable ParseJson(string json)
		{
			if (string.IsNullOrEmpty(json)) return null;
			json = json.Trim();
			if (!json.StartsWith("{") || !json.EndsWith("}")) return null;

			var result = new Hashtable();
			string content = json.Substring(1, json.Length - 2);
			// Разбиваю на пары ключ:значение, учитывая вложенные объекты и строки
			// Разбиваею по запятым, но не внутри кавычек и скобок
			var pairs = SplitJsonPairs(content);
			foreach (var pair in pairs)
			{
				int colonIndex = pair.IndexOf(':');
				if (colonIndex == -1) continue;
				string key = pair.Substring(0, colonIndex).Trim().Trim('"');
				string valueStr = pair.Substring(colonIndex + 1).Trim();
				object value = ParseJsonValue(valueStr);
				result[key] = value;
			}
			return result;
		}

		private static string[] SplitJsonPairs(string content)
		{
			var list = new ArrayList();
			int depth = 0;
			bool inString = false;
			int start = 0;
			for (int i = 0; i < content.Length; i++)
			{
				char c = content[i];
				if (c == '"' && (i == 0 || content[i - 1] != '\\')) inString = !inString;
				if (!inString)
				{
					if (c == '{' || c == '[') depth++;
					else if (c == '}' || c == ']') depth--;
					else if (c == ',' && depth == 0)
					{
						list.Add(content.Substring(start, i - start));
						start = i + 1;
					}
				}
			}
			if (start < content.Length) list.Add(content.Substring(start));
			string[] result = new string[list.Count];
			for (int i = 0; i < list.Count; i++) result[i] = (string)list[i];
			return result;
		}

		private static object ParseJsonValue(string valueStr)
		{
			valueStr = valueStr.Trim();
			if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
				return valueStr.Substring(1, valueStr.Length - 2);
			if (valueStr == "true") return true;
			if (valueStr == "false") return false;
			if (valueStr == "null") return null;
			if (valueStr.StartsWith("{"))
				return ParseJson(valueStr);
			if (valueStr.StartsWith("["))
			{
				// Массив: парсим как ArrayList
				var arr = new ArrayList();
				string arrayContent = valueStr.Substring(1, valueStr.Length - 2);
				var items = SplitJsonPairs(arrayContent); // переиспользуем логику разделения
				foreach (var item in items)
					arr.Add(ParseJsonValue(item));
				return arr;
			}
			// Число
			if (int.TryParse(valueStr, out int intVal)) return intVal;
			if (long.TryParse(valueStr, out long longVal)) return longVal;
			if (double.TryParse(valueStr, out double dblVal)) return dblVal;
			return valueStr;
		}
	}
}
