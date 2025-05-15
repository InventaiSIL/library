using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using Inventai;

[CustomEditor(typeof(Image))]
public class ImageInventaiEditor : Editor
{
    private string prompt;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("InventAI", EditorStyles.boldLabel);

        prompt = EditorGUILayout.TextField("Prompt", prompt);
        if (GUILayout.Button("Generate asset with InventAI"))
        {
            if (string.IsNullOrEmpty(prompt))
            {
                EditorUtility.DisplayDialog("Error", "Prompt cannot be empty", "OK");
            }
            else
            {
                GenerateAndApplyAsset(prompt);
            }
        }
    }

    private async void GenerateAndApplyAsset(string prompt)
    {
        EditorUtility.DisplayProgressBar("Generating asset with InventAI", "Please wait...", 0.5f);

        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string modelId = InventaiSettings.ModelId;
            string baseUrl = InventaiSettings.BaseUrl;
            string context = InventaiPromptUtils.GetSelectedPresetAsString();
            Texture2D texture = await InventaiImageGeneration.GenerateTextureFromPromptAsync(prompt, apiKey, modelId, baseUrl, context);
            // For UI Images, use a default pixelsPerUnit of 100
            float pixelsPerUnit = 100f;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
            Image image = (Image)target;
            image.sprite = sprite;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}