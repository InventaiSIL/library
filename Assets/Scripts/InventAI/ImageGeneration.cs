using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
using Inventai.ImageAgents;

public static class InventaiImageGeneration
{
    public static async Task<Texture2D> GenerateTextureFromPromptAsync(string prompt, string apiKey, string modelId, string baseUrl, string context)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("API Key is missing in Inventai settings.");

        string fullPrompt = string.IsNullOrWhiteSpace(context) ? prompt : prompt + "(Context: " + context + ")";
        var result = await ImageGeneration.GenerateImageAsync(fullPrompt, apiKey, modelId, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] imageData = Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    public static void SaveTextureAsPng(Texture2D texture, string path)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
    }

    public static async Task<Texture2D> EditImageWithGptAsync(string imagePath, string prompt, string apiKey, string baseUrl)
    {
        var result = await ImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] editedImageData = System.Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(editedImageData);
        return texture;
    }
}
