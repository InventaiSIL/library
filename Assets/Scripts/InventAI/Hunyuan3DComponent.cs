using UnityEngine;

/// <summary>
/// Unity component to interface with the Hunyuan3D-2 API
/// </summary>
public class Hunyuan3DComponent : MonoBehaviour
{
    [Header("📡 API Settings")]
    [Tooltip("URL to the local Hunyuan3D API endpoint.")]
    public string apiUrl = "http://localhost:5000/api/hunyuan3d";

    [Tooltip("Optional API key for authentication (if required).")]
    [TextArea(1, 2)]
    public string apiKey;

    [Header("📝 Generation Options")]
    [Tooltip("Optional text prompt to send to the API.")]
    [TextArea(2, 4)]
    public string prompt;

    //[Tooltip("Optional image to convert to base64 and send.")]
    //public Texture2D image;

    [Header("💾 Output Settings")]
    [Tooltip("Name of the file the API will generate, including extension (e.g. model.obj)")]
    public string resultFileName = "GeneratedModel.obj";

    [Tooltip("Folder inside Assets/ where the result file is saved.")]
    public string resultFolder = "Assets/GeneratedModels";
}