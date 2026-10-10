using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.UI;
using System.Globalization;

namespace FireBlade.NotificationEditor.Helpers
{

    public sealed class ColorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Color);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            string s = (string)reader.Value;

            if (s == null || !s.StartsWith("#", StringComparison.Ordinal))
                throw new JsonSerializationException("Invalid color.");

            try
            {
                if (s.Length == 9)
                {
                    byte a = byte.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                    byte r = byte.Parse(s.Substring(3, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(s.Substring(5, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(s.Substring(7, 2), NumberStyles.HexNumber);

                    return Color.FromArgb(a, r, g, b);
                }

                if (s.Length == 7)
                {
                    byte r = byte.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(s.Substring(3, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(s.Substring(5, 2), NumberStyles.HexNumber);

                    return Color.FromArgb(255, r, g, b);
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Invalid color.", ex);
            }

            throw new JsonSerializationException("Invalid color.");
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            Color color = (Color)value;
            writer.WriteValue(
                $"#{color.A:x2}{color.R:x2}{color.G:x2}{color.B:x2}");
        }
    }
}
