using System.Text;

namespace Inventai
{
    public static class InventaiPromptUtils
    {
        public static string GetSelectedPresetAsString()
        {
            var preset = InventaiSettingsProvider.GetSelectedPreset();
            if (preset == null) return string.Empty;
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(preset.artStyle))
                sb.Append($"Art Style: {(string.IsNullOrWhiteSpace(preset.customArtStyle) ? preset.artStyle : preset.customArtStyle)}; ");
            if (!string.IsNullOrWhiteSpace(preset.universe))
                sb.Append($"Universe: {(string.IsNullOrWhiteSpace(preset.customUniverse) ? preset.universe : preset.customUniverse)}; ");
            if (!string.IsNullOrWhiteSpace(preset.genre))
                sb.Append($"Genre: {(string.IsNullOrWhiteSpace(preset.customGenre) ? preset.genre : preset.customGenre)}; ");
            if (!string.IsNullOrWhiteSpace(preset.mood))
                sb.Append($"Mood: {(string.IsNullOrWhiteSpace(preset.customMood) ? preset.mood : preset.customMood)}; ");
            if (!string.IsNullOrWhiteSpace(preset.colorPalette))
                sb.Append($"Color Palette: {(string.IsNullOrWhiteSpace(preset.customColorPalette) ? preset.colorPalette : preset.customColorPalette)}; ");
            if (!string.IsNullOrWhiteSpace(preset.inspiration))
                sb.Append($"Inspiration: {(string.IsNullOrWhiteSpace(preset.customInspiration) ? preset.inspiration : preset.customInspiration)}; ");
            if (!string.IsNullOrWhiteSpace(preset.customPromptAddition))
                sb.Append($"{preset.customPromptAddition}; ");
            return sb.ToString().Trim();
        }
    }
}