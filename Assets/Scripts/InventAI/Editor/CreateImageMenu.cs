using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Inventai;
using System.Linq;
using System.Collections.Generic;

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

    [MenuItem("Assets/Create/Animation Sprite with InventAI")]
    public static void CreateInventaiAnimationSprite()
    {
        InventaiAnimationPromptWindow.Open();
    }

    [MenuItem("Assets/Edit to Animation Sprite with InventAI", true)]
    private static bool ValidateEditToAnimationSpriteWithInventAI()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Assets/Edit to Animation Sprite with InventAI")]
    private static void EditToAnimationSpriteWithInventAI()
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Texture2D asset.", "OK");
            return;
        }
        string imagePath = AssetDatabase.GetAssetPath(tex);
        EditToAnimationSpritePromptWindow.Open(imagePath);
    }

    private static async void EditAndSaveImage(string imagePath, string prompt)
    {
        EditorUtility.DisplayProgressBar("Editing image with InventAI", "Please wait...", 0.5f);
        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string baseUrl = InventaiSettings.BaseUrl;
            Texture2D texture = await InventaiImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
            // Resize to match original image size
            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            if (original != null && (texture.width != original.width || texture.height != original.height))
            {
                texture = ResizeTexture(texture, original.width, original.height);
            }
            InventaiImageGeneration.SaveTextureAsPng(texture, imagePath);
            AssetDatabase.ImportAsset(imagePath);
            SetTextureImporterToSprite(imagePath);
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
            // Resize to match original image size
            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            if (original != null && (texture.width != original.width || texture.height != original.height))
            {
                texture = ResizeTexture(texture, original.width, original.height);
            }
            string dir = Path.GetDirectoryName(imagePath);
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string newPath = Path.Combine(dir, name + "_inventai_variant.png");
            InventaiImageGeneration.SaveTextureAsPng(texture, newPath);
            AssetDatabase.ImportAsset(newPath);
            SetTextureImporterToSprite(newPath);
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

    /// <summary>
    /// Resizes a Texture2D to the specified width and height using bilinear filtering. Guarantees exact size.
    /// </summary>
    private static Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        rt.filterMode = source.filterMode;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(width, height, source.format, false);
        result.filterMode = source.filterMode;
        result.wrapMode = source.wrapMode;
        // Read the full rect, ensuring exact size
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
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
        guids = guids.Where(guid =>
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return !path.Contains("TextMesh Pro") && !path.Contains("Settings");
        }).ToArray();

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("InventAI", "No images found in Assets folder.", "OK");
            return;
        }
        string apiKey = InventaiSettings.ApiKey;
        string baseUrl = InventaiSettings.BaseUrl;
        string context = InventaiPromptUtils.GetSelectedPresetAsString();

        EditorUtility.DisplayProgressBar("InventAI", "Editing images with AI...", 0.0f);
        bool canceled = false;
        int maxDegreeOfParallelism = 4;
        var tasks = new List<Task<(string assetPath, byte[] imageData, int index)>>();

        // Start parallel AI edit tasks (only network and byte[] work)
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            int index = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    byte[] imageData = await InventaiImageGeneration.EditImageWithGptToBytesAsync(assetPath, context, apiKey, baseUrl);
                    return (assetPath, imageData, index);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Request error for {assetPath}: {e.Message}");
                    return (assetPath, null, index);
                }
            });
            tasks.Add(task);
            // Limit degree of parallelism
            if (tasks.Count >= maxDegreeOfParallelism)
            {
                var finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);
            }
        }
        // Wait for all remaining tasks
        var results = await Task.WhenAll(tasks);

        // Sort results by original index to preserve order
        var orderedResults = results.OrderBy(r => r.index).ToArray();

        // Sequentially save and import in the Editor, with progress and cancel
        for (int i = 0; i < orderedResults.Length; i++)
        {
            var (assetPath, imageData, _) = orderedResults[i];
            if (EditorUtility.DisplayCancelableProgressBar("InventAI", $"Saving and importing {Path.GetFileName(assetPath)} ({i + 1}/{orderedResults.Length})", (float)i / orderedResults.Length))
            {
                canceled = true;
                break;
            }
            if (imageData != null)
            {
                // Load original to get size
                Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                Texture2D editedTexture = new Texture2D(2, 2);
                editedTexture.LoadImage(imageData);
                if (original != null && (editedTexture.width != original.width || editedTexture.height != original.height))
                {
                    editedTexture = ResizeTexture(editedTexture, original.width, original.height);
                }
                InventaiImageGeneration.SaveTextureAsPng(editedTexture, assetPath);
                AssetDatabase.ImportAsset(assetPath);
                SetTextureImporterToSprite(assetPath);
            }
        }
        EditorUtility.ClearProgressBar();
        if (canceled)
            EditorUtility.DisplayDialog("InventAI", "Preset application canceled.", "OK");
        else
            EditorUtility.DisplayDialog("InventAI", $"Preset applied to {orderedResults.Length} images.", "OK");
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

    private class InventaiAnimationPromptWindow : EditorWindow
    {
        private string prompt = "";
        public static void Open()
        {
            var window = ScriptableObject.CreateInstance<InventaiAnimationPromptWindow>();
            window.titleContent = new GUIContent("InventAI Animation Prompt");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 100);
            window.ShowUtility();
        }
        void OnGUI()
        {
            GUILayout.Label("Enter a prompt for the animation sprite:", EditorStyles.wordWrappedLabel);
            prompt = EditorGUILayout.TextField("Prompt", prompt);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Animation Sprite"))
            {
                if (!string.IsNullOrEmpty(prompt))
                {
                    CustomCreateMenu.GenerateAndSaveAnimationSprite(prompt);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Prompt cannot be empty", "OK");
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
    /// Generates and saves an animation sprite from a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to generate the animation sprite from.</param>
    public static async void GenerateAndSaveAnimationSprite(string prompt)
    {
        EditorUtility.DisplayProgressBar("Generating animation sprite with InventAI", "Please wait...", 0.5f);
        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string modelId = InventaiSettings.ModelId;
            string baseUrl = InventaiSettings.BaseUrl;
            string context = InventaiPromptUtils.GetSelectedPresetAsString();
            string fullPrompt =
                "You are generating a high-quality 2D game asset sprite sheet for use in a professional game engine. " +
                "The image should depict a clearly defined subject, with a fully transparent background. " +
                "Do not include any text, watermarks, borders, or extraneous elements. The sprite sheet should be high resolution, clean, and ready for direct use in a 2D game. " +
                "If the user prompt does not specify a background, assume full transparency. " +
                "This is for an animation for a game, where the final image is divided into multiple sub-images, each serving as a continuous animation keyframe. " +
                "Design the sequence to depict the keyframes transition smoothly and continuously, and include a number of frames that will be used to create the animation. " +
                (!string.IsNullOrWhiteSpace(context) ? $"Request context (style, genre, etc): {context} " : "") +
                $"User prompt (what the user wants to see): {prompt}";

            Texture2D texture = await InventaiImageGeneration.GenerateTextureFromCustomPromptAsync(fullPrompt, apiKey, modelId, baseUrl);

            // Save as PNG in the selected folder
            string randomString = System.Guid.NewGuid().ToString().Substring(0, 8);
            string path = GetSelectedPathOrFallback() + "/InventAI_AnimationSprite_" + randomString + ".png";
            InventaiImageGeneration.SaveTextureAsPng(texture, path);
            AssetDatabase.ImportAsset(path);
            // Set import settings to Sprite (Multiple)
            SetTextureImporterToSpriteMultiple(path);
            EditorUtility.DisplayDialog("InventAI", "Animation sprite created at: " + path, "OK");
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

    /// <summary>
    /// Sets the import settings of a texture asset to Sprite (Multiple) and reimports it.
    /// </summary>
    /// <param name="path">The asset path of the texture.</param>
    private static void SetTextureImporterToSpriteMultiple(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();
        }
    }

    private class EditToAnimationSpritePromptWindow : EditorWindow
    {
        private string prompt = "";
        private string imagePath;
        public static void Open(string imagePath)
        {
            var window = ScriptableObject.CreateInstance<EditToAnimationSpritePromptWindow>();
            window.titleContent = new GUIContent("Edit to Animation Sprite");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 100);
            window.imagePath = imagePath;
            window.ShowUtility();
        }
        void OnGUI()
        {
            GUILayout.Label("Enter a prompt for the animation sprite:", EditorStyles.wordWrappedLabel);
            prompt = EditorGUILayout.TextField("Prompt", prompt);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit to Animation Sprite"))
            {
                if (!string.IsNullOrEmpty(prompt))
                {
                    CustomCreateMenu.EditImageToAnimationSpriteAndSave(imagePath, prompt);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Prompt cannot be empty", "OK");
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
    /// Modifies an existing image to an animation sprite using AI and saves the result as a sprite sheet.
    /// </summary>
    /// <param name="imagePath">The path to the existing image.</param>
    /// <param name="prompt">The prompt to guide the animation sprite generation.</param>
    public static async void EditImageToAnimationSpriteAndSave(string imagePath, string prompt)
    {
        EditorUtility.DisplayProgressBar("Editing to animation sprite with InventAI", "Please wait...", 0.5f);
        try
        {
            string apiKey = InventaiSettings.ApiKey;
            string modelId = InventaiSettings.ModelId;
            string baseUrl = InventaiSettings.BaseUrl;
            string context = InventaiPromptUtils.GetSelectedPresetAsString();
            string fullPrompt =
                "You are generating a high-quality 2D game asset sprite sheet for use in a professional game engine. " +
                "The image should depict a clearly defined subject, with a fully transparent background. " +
                "Do not include any text, watermarks, borders, or extraneous elements. The sprite sheet should be high resolution, clean, and ready for direct use in a 2D game. " +
                "If the user prompt does not specify a background, assume full transparency. " +
                "This is for an animation for a game, where the final image is divided into multiple sub-images, each serving as a continuous animation keyframe. " +
                "Design the sequence to depict the keyframes transition smoothly and continuously, and include a number of frames that will be used to create the animation. " +
                (!string.IsNullOrWhiteSpace(context) ? $"Request context (style, genre, etc): {context} " : "") +
                $"User prompt (what the user wants to see): {prompt} " +
                $" Use the provided image as the base for the animation sprite.";

            Texture2D texture = await InventaiImageGeneration.EditImageToAnimationSpriteAsync(imagePath, fullPrompt, apiKey, modelId, baseUrl);

            // Save as PNG in the same folder as the original image
            string dir = Path.GetDirectoryName(imagePath);
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string randomString = System.Guid.NewGuid().ToString().Substring(0, 8);
            string newPath = Path.Combine(dir, name + "_inventai_animsprite_" + randomString + ".png");
            InventaiImageGeneration.SaveTextureAsPng(texture, newPath);
            AssetDatabase.ImportAsset(newPath);
            // Set import settings to Sprite (Multiple)
            SetTextureImporterToSpriteMultiple(newPath);
            EditorUtility.DisplayDialog("InventAI", "Animation sprite created at: " + newPath, "OK");
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
}



