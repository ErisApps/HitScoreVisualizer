using System;
using Newtonsoft.Json;
using UnityEngine;

namespace HitScoreVisualizer.Helpers.Json
{
	internal class Vector3Converter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Vector3?) || objectType == typeof(Vector3);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var t = serializer.Deserialize(reader);
			if (t == null)
			{
				return objectType == typeof(Vector3) ? default(Vector3) : null!;
			}

			return JsonConvert.DeserializeObject<Vector3>(t.ToString());
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is Vector3 v)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("x");
				writer.WriteValue(v.x);
				writer.WritePropertyName("y");
				writer.WriteValue(v.y);
				writer.WritePropertyName("z");
				writer.WriteValue(v.z);
				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNull();
			}
		}
	}
}