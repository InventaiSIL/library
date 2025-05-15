using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Inventai;

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

    [MenuItem("Assets/Edit with InventAI...", true)]
    private static bool ValidateEditWithGptImage1()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Assets/Edit with InventAI...")]
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

    [MenuItem("Assets/Create variant with InventAI...", true)]
    private static bool ValidateCreateVariantWithInventAI()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Assets/Create Variant with InventAI...")]
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
}
