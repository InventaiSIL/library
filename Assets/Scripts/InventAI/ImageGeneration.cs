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

        string fullPrompt =
            "You are generating a high-quality 2D game asset sprite for use in a professional game engine. " +
            "The image should depict a single, clearly defined subject, centered in the frame, with a fully transparent background. " +
            "Do not include any text, watermarks, borders, or extraneous elements. The sprite should be high resolution, clean, and ready for direct use in a 2D game. " +
            "If the user prompt does not specify a background, assume full transparency. " +
            (!string.IsNullOrWhiteSpace(context) ? $"Request context (style, genre, etc): {context} " : "") +
            $"User prompt (what the user wants to see): {prompt}";

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
