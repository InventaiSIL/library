using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

static class InventaiSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateInventaiSettingsProvider()
    {
        var modelOptions = new[] { "dall-e-3", "dall-e-2", "gpt-image-1" };

        // Create a new SettingsProvider for the Inventai settings
        var provider = new SettingsProvider("Project/InventAI Settings", SettingsScope.Project)
        {
            // Create the SettingsProvider GUI content
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

                // Draw the Context field
                var context = EditorPrefs.GetString("Open AI Context", "");
                EditorGUI.BeginChangeCheck();
                context = EditorGUILayout.TextField("Context", context);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString("Open AI Context", context);
                }

                // Add a test connection button
                EditorGUILayout.Space();
                if (GUILayout.Button("Test Connection"))
                {
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        EditorUtility.DisplayDialog("Error", "API Key cannot be empty", "OK");
                    }
                    else
                    {
                        // Here you would implement a connection test
                        EditorUtility.DisplayDialog("Info", "Connection test not implemented yet.", "OK");
                    }
                }
            },

            // Populate the search keywords
            keywords = new HashSet<string>(new[] { "InventAI", "AI", "API", "Key", "Model" })
        };

        return provider;
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