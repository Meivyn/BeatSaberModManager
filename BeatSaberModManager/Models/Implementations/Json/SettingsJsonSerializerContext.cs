using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Settings;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(AppSettings))]
    [JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
    internal sealed partial class SettingsJsonSerializerContext : JsonSerializerContext;
}
