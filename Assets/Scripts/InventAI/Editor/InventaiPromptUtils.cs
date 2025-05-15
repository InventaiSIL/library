using System.Text;

/// <summary>
/// Utility class for building context strings from the selected InventAI art preset.
/// </summary>
namespace Inventai
{
    public static class InventaiPromptUtils
    {
        /// <summary>
        /// Builds a context string from the currently selected art preset, including default instructions for sprite generation.
        /// </summary>
        /// <returns>A string describing the preset and default sprite requirements.</returns>
        public static string GetSelectedPresetAsString()
        {
            var preset = InventaiSettingsProvider.GetSelectedPreset();
            var sb = new StringBuilder();
            if (preset != null)
            {
                if (!string.IsNullOrWhiteSpace(preset.artStyle) && preset.artStyle != "Custom")
                    sb.Append($"Art Style: {preset.artStyle}; ");
                else if (preset.artStyle == "Custom" && !string.IsNullOrWhiteSpace(preset.customArtStyle))
                    sb.Append($"Art Style: {preset.customArtStyle}; ");

                if (!string.IsNullOrWhiteSpace(preset.universe) && preset.universe != "Custom")
                    sb.Append($"Universe: {preset.universe}; ");
                else if (preset.universe == "Custom" && !string.IsNullOrWhiteSpace(preset.customUniverse))
                    sb.Append($"Universe: {preset.customUniverse}; ");

                if (!string.IsNullOrWhiteSpace(preset.genre) && preset.genre != "Custom")
                    sb.Append($"Genre: {preset.genre}; ");
                else if (preset.genre == "Custom" && !string.IsNullOrWhiteSpace(preset.customGenre))
                    sb.Append($"Genre: {preset.customGenre}; ");

                if (!string.IsNullOrWhiteSpace(preset.mood) && preset.mood != "Custom")
                    sb.Append($"Mood: {preset.mood}; ");
                else if (preset.mood == "Custom" && !string.IsNullOrWhiteSpace(preset.customMood))
                    sb.Append($"Mood: {preset.customMood}; ");

                if (!string.IsNullOrWhiteSpace(preset.colorPalette) && preset.colorPalette != "Custom")
                    sb.Append($"Color Palette: {preset.colorPalette}; ");
                else if (preset.colorPalette == "Custom" && !string.IsNullOrWhiteSpace(preset.customColorPalette))
                    sb.Append($"Color Palette: {preset.customColorPalette}; ");

                if (!string.IsNullOrWhiteSpace(preset.inspiration) && preset.inspiration != "Custom")
                    sb.Append($"Inspiration: {preset.inspiration}; ");
                else if (preset.inspiration == "Custom" && !string.IsNullOrWhiteSpace(preset.customInspiration))
                    sb.Append($"Inspiration: {preset.customInspiration}; ");

                if (!string.IsNullOrWhiteSpace(preset.customPromptAddition))
                    sb.Append($"{preset.customPromptAddition}; ");
            }
            return sb.ToString().Trim();
        }
    }
}
