using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a single art preset for InventAI, including style, universe, genre, mood, color palette, inspiration, and custom fields.
/// </summary>
[System.Serializable]
public class InventaiArtPreset
{
    public string name;
    public string artStyle;
    public string universe;
    public string genre;
    public string mood;
    public string colorPalette;
    public string inspiration;
    public string customPromptAddition;
    // For custom values
    public string customArtStyle;
    public string customUniverse;
    public string customGenre;
    public string customMood;
    public string customColorPalette;
    public string customInspiration;
}

/// <summary>
/// Wrapper for serializing a list of InventaiArtPreset objects.
/// </summary>
[System.Serializable]
public class InventaiArtPresetListWrapper
{
    public List<InventaiArtPreset> presets = new List<InventaiArtPreset>();
    public InventaiArtPresetListWrapper() { }
}

/// <summary>
/// Provides the Unity Project Settings UI for InventAI, including API key, model, base URL, and art presets.
/// </summary>
static class InventaiSettingsProvider
{
    private static List<InventaiArtPreset> _presets;
    private static int _selectedPresetIndex = 0;

    private static string PresetsKey => "InventAI_ArtPresets";
    private static string SelectedPresetKey => "InventAI_SelectedPreset";

    // Preconfigured options
    private static readonly string[] ArtStyles = { "Pixel Art", "Watercolor", "Oil Painting", "Ink Sketch", "Low Poly", "Photorealistic", "Anime", "Comic Book", "Custom" };
    private static readonly string[] Universes = { "Cyberpunk", "Medieval Fantasy", "Space Opera", "Steampunk", "Underwater World", "Post-Apocalyptic", "Fairy Tale", "Ancient Egypt", "Custom" };
    private static readonly string[] Genres = { "Horror", "Adventure", "Puzzle", "Platformer", "RPG", "Shooter", "Custom" };
    private static readonly string[] Moods = { "Whimsical", "Dark", "Epic", "Peaceful", "Mysterious", "Surreal", "Custom" };
    private static readonly string[] ColorPalettes = { "Pastel", "Neon", "Monochrome", "Earth Tones", "Retro 80s", "Black & White", "Custom" };
    private static readonly string[] Inspirations = { "Studio Ghibli", "Blade Runner", "Pixar", "Van Gogh", "Custom" };

    private static void LoadPresets()
    {
        string json = EditorPrefs.GetString(PresetsKey, "");
        if (string.IsNullOrEmpty(json))
        {
            _presets = new List<InventaiArtPreset>();
            _selectedPresetIndex = 0;
            return;
        }
        try
        {
            var wrapper = JsonUtility.FromJson<InventaiArtPresetListWrapper>(json);
            _presets = wrapper?.presets ?? new List<InventaiArtPreset>();
        }
        catch
        {
            _presets = new List<InventaiArtPreset>();
        }
        _selectedPresetIndex = EditorPrefs.GetInt(SelectedPresetKey, 0);
        if (_selectedPresetIndex >= _presets.Count) _selectedPresetIndex = 0;
    }

    private static void SavePresets()
    {
        var wrapper = new InventaiArtPresetListWrapper { presets = _presets };
        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString(PresetsKey, json);
        EditorPrefs.SetInt(SelectedPresetKey, _selectedPresetIndex);
    }

    [SettingsProvider]
    public static SettingsProvider CreateInventaiSettingsProvider()
    {
        var modelOptions = new[] { "dall-e-3", "dall-e-2", "gpt-image-1" };

        LoadPresets();

        var provider = new SettingsProvider("Project/InventAI Settings", SettingsScope.Project)
        {
            guiHandler = (searchContext) =>
            {
                // Load the saved settings
                var apiKey = EditorPrefs.GetString("Open AI API Key", "");
                var modelId = EditorPrefs.GetString("Open AI Model ID", "dall-e-3");
                var baseUrl = EditorPrefs.GetString("Open AI Base URL", "https://api.openai.com/v1/images/generations");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Image generation", EditorStyles.boldLabel);

                // Draw the API Key field
                EditorGUI.BeginChangeCheck();
                apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString("Open AI API Key", apiKey);
                }

                // Draw the Model ID field as a dropdown
                int currentModelIndex = System.Array.IndexOf(modelOptions, modelId);
                if (currentModelIndex < 0) currentModelIndex = 0;
                EditorGUI.BeginChangeCheck();
                int selectedModelIndex = EditorGUILayout.Popup("Model", currentModelIndex, modelOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    modelId = modelOptions[selectedModelIndex];
                    EditorPrefs.SetString("Open AI Model ID", modelId);
                }

                // Draw the Base URL field
                EditorGUI.BeginChangeCheck();
                baseUrl = EditorGUILayout.TextField("API Base URL", baseUrl);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString("Open AI Base URL", baseUrl);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Art Presets", EditorStyles.boldLabel);

                // Preset selection
                if (_presets.Count > 0)
                {
                    string[] presetNames = _presets.Select(p => p.name).ToArray();
                    _selectedPresetIndex = EditorGUILayout.Popup("Selected Preset", _selectedPresetIndex, presetNames);
                }
                else
                {
                    EditorGUILayout.LabelField("No presets defined.");
                }

                // Preset management
                if (GUILayout.Button("Add Preset"))
                {
                    _presets.Add(new InventaiArtPreset { name = "New Preset" });
                    _selectedPresetIndex = _presets.Count - 1;
                }

                if (_presets.Count > 0 && _selectedPresetIndex < _presets.Count)
                {
                    var preset = _presets[_selectedPresetIndex];
                    preset.name = EditorGUILayout.TextField("Name", preset.name);

                    // Art Style
                    int artStyleIdx = System.Array.IndexOf(ArtStyles, preset.artStyle);
                    if (artStyleIdx < 0) artStyleIdx = 0;
                    artStyleIdx = EditorGUILayout.Popup("Art Style", artStyleIdx, ArtStyles);
                    preset.artStyle = ArtStyles[artStyleIdx];
                    if (preset.artStyle == "Custom")
                        preset.customArtStyle = EditorGUILayout.TextField("Custom Art Style", preset.customArtStyle);
                    else
                        preset.customArtStyle = "";

                    // Universe
                    int universeIdx = System.Array.IndexOf(Universes, preset.universe);
                    if (universeIdx < 0) universeIdx = 0;
                    universeIdx = EditorGUILayout.Popup("Universe", universeIdx, Universes);
                    preset.universe = Universes[universeIdx];
                    if (preset.universe == "Custom")
                        preset.customUniverse = EditorGUILayout.TextField("Custom Universe", preset.customUniverse);
                    else
                        preset.customUniverse = "";

                    // Genre
                    int genreIdx = System.Array.IndexOf(Genres, preset.genre);
                    if (genreIdx < 0) genreIdx = 0;
                    genreIdx = EditorGUILayout.Popup("Genre", genreIdx, Genres);
                    preset.genre = Genres[genreIdx];
                    if (preset.genre == "Custom")
                        preset.customGenre = EditorGUILayout.TextField("Custom Genre", preset.customGenre);
                    else
                        preset.customGenre = "";

                    // Mood
                    int moodIdx = System.Array.IndexOf(Moods, preset.mood);
                    if (moodIdx < 0) moodIdx = 0;
                    moodIdx = EditorGUILayout.Popup("Mood", moodIdx, Moods);
                    preset.mood = Moods[moodIdx];
                    if (preset.mood == "Custom")
                        preset.customMood = EditorGUILayout.TextField("Custom Mood", preset.customMood);
                    else
                        preset.customMood = "";

                    // Color Palette
                    int colorPaletteIdx = System.Array.IndexOf(ColorPalettes, preset.colorPalette);
                    if (colorPaletteIdx < 0) colorPaletteIdx = 0;
                    colorPaletteIdx = EditorGUILayout.Popup("Color Palette", colorPaletteIdx, ColorPalettes);
                    preset.colorPalette = ColorPalettes[colorPaletteIdx];
                    if (preset.colorPalette == "Custom")
                        preset.customColorPalette = EditorGUILayout.TextField("Custom Color Palette", preset.customColorPalette);
                    else
                        preset.customColorPalette = "";

                    // Inspiration
                    int inspirationIdx = System.Array.IndexOf(Inspirations, preset.inspiration);
                    if (inspirationIdx < 0) inspirationIdx = 0;
                    inspirationIdx = EditorGUILayout.Popup("Inspiration", inspirationIdx, Inspirations);
                    preset.inspiration = Inspirations[inspirationIdx];
                    if (preset.inspiration == "Custom")
                        preset.customInspiration = EditorGUILayout.TextField("Custom Inspiration", preset.customInspiration);
                    else
                        preset.customInspiration = "";

                    preset.customPromptAddition = EditorGUILayout.TextField("Additional Prompt", preset.customPromptAddition);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Delete Preset"))
                    {
                        _presets.RemoveAt(_selectedPresetIndex);
                        if (_selectedPresetIndex >= _presets.Count) _selectedPresetIndex = _presets.Count - 1;
                    }
                    if (GUILayout.Button("Save Preset"))
                    {
                        SavePresets();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Add Apply preset button
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply preset to all images in Assets"))
                {
                    CustomCreateMenu.ApplyPresetToAllImages();
                }
            },

            // Populate the search keywords
            keywords = new HashSet<string>(new[] { "InventAI", "AI", "API", "Key", "Model", "Preset" })
        };

        return provider;
    }

    // Add this method to allow access to the selected preset from other scripts
    public static InventaiArtPreset GetSelectedPreset()
    {
        LoadPresets();
        if (_presets != null && _presets.Count > 0 && _selectedPresetIndex >= 0 && _selectedPresetIndex < _presets.Count)
            return _presets[_selectedPresetIndex];
        return null;
    }

    public static string GetSelectedPresetAsString()
    {
        var preset = GetSelectedPreset();
        if (preset == null) return "";
        return $"Art Style: {preset.artStyle}, Universe: {preset.universe}, Genre: {preset.genre}, Mood: {preset.mood}, Color Palette: {preset.colorPalette}, Inspiration: {preset.inspiration}";
    }
}

// Static class for accessing Inventai settings throughout the editor
public static class InventaiSettings
{
    public static string ApiKey => EditorPrefs.GetString("Open AI API Key", "");
    public static string ModelId => EditorPrefs.GetString("Open AI Model ID", "dall-e-3");
    public static string BaseUrl => EditorPrefs.GetString("Open AI Base URL", "https://api.openai.com/v1/images/generations");
    public static string Context => EditorPrefs.GetString("Open AI Context", "");
}
