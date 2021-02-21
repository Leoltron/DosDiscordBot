using Dos.Game;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dos.ReplayService
{
    public static class JsonConfigurationExtensions
    {
        public static IMvcBuilder AddConfiguredNewtonsoftJson(this IMvcBuilder builder) =>
            builder.AddNewtonsoftJson(opts =>
                    {
                        opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        opts.SerializerSettings.Converters.Add(new StringEnumConverter());
                        opts.SerializerSettings.Converters.Insert(0, new NullableCardJsonConverter());
                    });
    }
}
