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
	}
}
