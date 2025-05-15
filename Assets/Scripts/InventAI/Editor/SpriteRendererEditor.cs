using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // Make sure to add this namespace
using Inventai;
using System;

// Specify that this editor is for the SpriteRenderer component
[CustomEditor(typeof(SpriteRenderer))]
public class SpriteRendererCustomEditor : Editor
{
    private string prompt;
    private static readonly HttpClient client = new HttpClient();

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
            SpriteRenderer spriteRenderer = (SpriteRenderer)target;
            Vector2 objectSize = spriteRenderer.bounds.size;
            float targetWorldSize = Mathf.Max(objectSize.x, objectSize.y);
            float textureSize = Mathf.Max(texture.width, texture.height);
            float pixelsPerUnit = textureSize / targetWorldSize;
            spriteRenderer.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
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
