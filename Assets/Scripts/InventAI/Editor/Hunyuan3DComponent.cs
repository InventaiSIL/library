using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

[CustomEditor(typeof(Hunyuan3DComponent))]
public class Hunyuan3DComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Hunyuan3DComponent component = (Hunyuan3DComponent)target;

        if (GUILayout.Button("✨ Generate 3D Model"))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequestAndSaveModel(component));
        }
    }

    private IEnumerator SendRequestAndSaveModel(Hunyuan3DComponent component)
    {
        // Build request payload only with non-null values
        var requestJson = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(component.prompt))
            requestJson["text"] = component.prompt;

        /*if (component.image != null)
        {
            byte[] pngBytes = component.image.EncodeToPNG();
            string base64 = System.Convert.ToBase64String(pngBytes);
            requestJson["image"] = base64;
        }*/

        if (requestJson.Count == 0)
        {
            Debug.LogError("❌ Cannot send request: Provide either a prompt or an image.");
            yield break;
        }

        string jsonPayload = JsonConvert.SerializeObject(requestJson, Formatting.Indented);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(component.apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"📡 Sending request:\n{jsonPayload}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ API request failed: " + request.error);
            yield break;
        }

        // Get binary .glb content
        byte[] modelBytes = request.downloadHandler.data;

        // Save model to Assets/GeneratedModels/
        string saveFolder = Path.Combine(Application.dataPath, component.resultFolder.Replace("Assets/", "").TrimStart('/', '\\'));

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string fileName = component.resultFileName;
        string savePath = Path.Combine(saveFolder, fileName);
        File.WriteAllBytes(savePath, modelBytes);

        Debug.Log($"✅ Model saved to: {savePath}");

        // Refresh AssetDatabase
        AssetDatabase.Refresh();
        string assetPath = Path.Combine("Assets", component.resultFolder.TrimStart('/', '\\'), fileName).Replace("\\", "/");


        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (model != null)
        {
            PrefabUtility.InstantiatePrefab(model);
            Debug.Log("🎉 Model added to the scene!");
        }
        else
        {
            Debug.LogWarning("⚠ Model saved but could not be loaded. Check file format support in Unity.");
        }
    }
}