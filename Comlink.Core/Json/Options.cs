using System.Text.Json;
using System.Text.Json.Serialization;

namespace Comlink.Core.Json
{
    public static class Options
    {
        public static JsonSerializerOptions Default
        {
            get
            {
                JsonSerializerOptions options = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new JsonStringEnumConverter(new JsonUpperCaseNamingPolicy()),
                    },
                };

                return options;
            }
        }
    }
}