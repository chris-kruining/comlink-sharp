using System.Text.Json;

namespace Comlink.Core.Json
{
    public static class Options
    {
        public static JsonSerializerOptions Default
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                options.Converters.Add(new ObjectConverter());

                return options;
            }
        }
    }
}