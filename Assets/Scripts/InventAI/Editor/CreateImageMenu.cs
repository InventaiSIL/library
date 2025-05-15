using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Inventai;
using System.Linq;

/// <summary>
/// Provides menu items and utility methods for creating, editing, and batch-generating images with InventAI in the Unity Editor.
/// </summary>
public class CustomCreateMenu : MonoBehaviour
{
    private static string pendingEditImagePath = null;
    private static bool isEditMode = false;
    private static bool isVariantMode = false;

    [MenuItem("Assets/Create/Asset with InventAI")]
    public static void CreateInventaiImage()
    {
        InventaiPromptWindow.Open();
    }

    [MenuItem("Assets/Edit with InventAI", true)]
    private static bool ValidateEditWithGptImage1()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Assets/Edit with InventAI")]
    private static void EditWithGptImage1()
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Texture2D asset.", "OK");
            return;
        }
        pendingEditImagePath = AssetDatabase.GetAssetPath(tex);
        isEditMode = true;
        InventaiPromptWindow.Open();
    }

    [MenuItem("Assets/Create variant with InventAI", true)]
    private static bool ValidateCreateVariantWithInventAI()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Assets/Create variant with InventAI")]
    private static void CreateVariantWithInventAI()
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Texture2D asset.", "OK");
            return;
        }
        pendingEditImagePath = AssetDatabase.GetAssetPath(tex);
        isEditMode = false;
        isVariantMode = true;
        InventaiPromptWindow.Open();
    }

    [MenuItem("Assets/Create/Batch generate with InventAI")]
    public static void BatchGenerateInventaiImages()
    {
        BatchPromptWindow.Open();
    }

    private static async void EditAndSaveImage(string imagePath, string prompt)
    {
        EditorUtility.DisplayProgressBar("Editing image with InventAI", "Please wait...", 0.5f);
        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string baseUrl = InventaiSettings.BaseUrl;
            Texture2D texture = await InventaiImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
            InventaiImageGeneration.SaveTextureAsPng(texture, imagePath);
            AssetDatabase.ImportAsset(imagePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Request error: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private class InventaiPromptWindow : EditorWindow
    {
        private string prompt = "";
        public static void Open()
        {
            var window = ScriptableObject.CreateInstance<InventaiPromptWindow>();
            window.titleContent = new GUIContent("InventAI Prompt");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 100);
            window.ShowUtility();
        }
        void OnGUI()
        {
            GUILayout.Label("Enter a prompt for the image:", EditorStyles.wordWrappedLabel);
            prompt = EditorGUILayout.TextField("Prompt", prompt);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(isEditMode ? "Edit" : (isVariantMode ? "Create Variant" : "Generate")))
            {
                if (!string.IsNullOrEmpty(prompt))
                {
                    if (isEditMode && !string.IsNullOrEmpty(pendingEditImagePath))
                    {
                        EditAndSaveImage(pendingEditImagePath, prompt);
                        pendingEditImagePath = null;
                        isEditMode = false;
                    }
                    else if (isVariantMode && !string.IsNullOrEmpty(pendingEditImagePath))
                    {
                        CreateVariantAndSaveImage(pendingEditImagePath, prompt);
                        pendingEditImagePath = null;
                        isVariantMode = false;
                    }
                    else
                    {
                        CustomCreateMenu.GenerateAndSaveImage(prompt);
                    }
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Prompt cannot be empty", "OK");
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                pendingEditImagePath = null;
                isEditMode = false;
                isVariantMode = false;
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    private static async void GenerateAndSaveImage(string prompt)
    {
        EditorUtility.DisplayProgressBar("Generating asset with InventAI", "Please wait...", 0.5f);

        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string modelId = InventaiSettings.ModelId;
            string baseUrl = InventaiSettings.BaseUrl;
            string context = InventaiPromptUtils.GetSelectedPresetAsString();
            Texture2D texture = await InventaiImageGeneration.GenerateTextureFromPromptAsync(prompt, apiKey, modelId, baseUrl, context);

            // Save as PNG in the selected folder
            string randomString = System.Guid.NewGuid().ToString().Substring(0, 8);
            string path = GetSelectedPathOrFallback() + "/InventAI_Image_" + randomString + ".png";
            InventaiImageGeneration.SaveTextureAsPng(texture, path);
            AssetDatabase.ImportAsset(path);
            // Set import settings to Sprite (Single)
            SetTextureImporterToSprite(path);
            EditorUtility.DisplayDialog("InventAI", "Image created at: " + path, "OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"Request error: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static async void CreateVariantAndSaveImage(string imagePath, string prompt)
    {
        EditorUtility.DisplayProgressBar("Creating variant with InventAI", "Please wait...", 0.5f);
        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string baseUrl = InventaiSettings.BaseUrl;
            Texture2D texture = await InventaiImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
            string dir = Path.GetDirectoryName(imagePath);
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string newPath = Path.Combine(dir, name + "_inventai_variant.png");
            InventaiImageGeneration.SaveTextureAsPng(texture, newPath);
            AssetDatabase.ImportAsset(newPath);
            EditorUtility.DisplayDialog("InventAI", "Variant image created at: " + newPath, "OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"Request error: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    /// <summary>
    /// Batch window for entering multiple prompts (one per line) for batch image generation.
    /// </summary>
    private class BatchPromptWindow : EditorWindow
    {
        private string promptsText = "";
        public static void Open()
        {
            var window = ScriptableObject.CreateInstance<BatchPromptWindow>();
            window.titleContent = new GUIContent("Batch InventAI Prompt");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 250);
            window.ShowUtility();
        }
        void OnGUI()
        {
            GUILayout.Label("Enter one prompt per line:", EditorStyles.wordWrappedLabel);
            promptsText = EditorGUILayout.TextArea(promptsText, GUILayout.Height(60));
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Batch"))
            {
                var prompts = promptsText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (prompts.Length > 0)
                {
                    CustomCreateMenu.BatchGenerateAndSaveImages(prompts);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please enter at least one prompt.", "OK");
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Generates and saves a batch of images, one for each prompt provided.
    /// </summary>
    /// <param name="prompts">Array of prompts, one per image.</param>
    public static async void BatchGenerateAndSaveImages(string[] prompts)
    {
        string apiKey = InventaiSettings.ApiKey;
        string modelId = InventaiSettings.ModelId;
        string baseUrl = InventaiSettings.BaseUrl;
        string context = InventaiPromptUtils.GetSelectedPresetAsString();
        string folder = GetSelectedPathOrFallback();
        bool canceled = false;
        for (int i = 0; i < prompts.Length; i++)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Batch generating with InventAI", $"Generating image {i + 1} of {prompts.Length}...", (float)i / prompts.Length))
            {
                canceled = true;
                break;
            }
            try
            {
                string prompt = prompts[i].Trim();
                if (string.IsNullOrEmpty(prompt)) continue;
                Texture2D texture = await InventaiImageGeneration.GenerateTextureFromPromptAsync(prompt, apiKey, modelId, baseUrl, context);
                string safePrompt = new string(prompt.Take(16).ToArray()).Replace(' ', '_');
                string randomString = System.Guid.NewGuid().ToString().Substring(0, 8);
                string path = folder + $"/InventAI_Batch_{i + 1}_{safePrompt}_{randomString}.png";
                InventaiImageGeneration.SaveTextureAsPng(texture, path);
                AssetDatabase.ImportAsset(path);
                // Set import settings to Sprite (Single)
                SetTextureImporterToSprite(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Request error: {e.Message}");
            }
        }
        EditorUtility.ClearProgressBar();
        if (canceled)
            EditorUtility.DisplayDialog("InventAI", "Batch generation canceled.", "OK");
        else
            EditorUtility.DisplayDialog("InventAI", $"Batch generation complete. {prompts.Length} images generated.", "OK");
    }

    /// <summary>
    /// Finds all Texture2D assets in the Assets folder and applies the current preset to each, regenerating them with AI.
    /// </summary>
    public static async void ApplyPresetToAllImages()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("InventAI", "No images found in Assets folder.", "OK");
            return;
        }
        string apiKey = InventaiSettings.ApiKey;
        string modelId = InventaiSettings.ModelId;
        string baseUrl = InventaiSettings.BaseUrl;
        string context = InventaiPromptUtils.GetSelectedPresetAsString();
        bool canceled = false;

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (EditorUtility.DisplayCancelableProgressBar("Applying preset to all images", $"Processing image {i + 1} of {guids.Length} called {fileName}...", (float)i / guids.Length))
            {
                canceled = true;
                break;
            }
            try
            {
                Texture2D texture = await InventaiImageGeneration.GenerateTextureFromPromptAsync(fileName, apiKey, modelId, baseUrl, context);
                InventaiImageGeneration.SaveTextureAsPng(texture, assetPath);
                AssetDatabase.ImportAsset(assetPath);
                // Set import settings to Sprite (Single)
                SetTextureImporterToSprite(assetPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing {assetPath}: {e.Message}");
            }
        }
        EditorUtility.ClearProgressBar();
        if (canceled)
            EditorUtility.DisplayDialog("InventAI", "Operation canceled.", "OK");
        else
            EditorUtility.DisplayDialog("InventAI", $"Preset applied to {guids.Length} images.", "OK");
    }

    /// <summary>
    /// Sets the import settings of a texture asset to Sprite (Single) and reimports it.
    /// </summary>
    /// <param name="path">The asset path of the texture.</param>
    private static void SetTextureImporterToSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}
