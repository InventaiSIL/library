using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
using Inventai.ImageAgents;

/// <summary>
/// Provides static methods for generating and editing images using AI, and saving textures as PNG files.
/// </summary>
public static class InventaiImageGeneration
{
    /// <summary>
    /// Generates a Texture2D from a user prompt and context using the AI image generation API.
    /// </summary>
    /// <param name="prompt">The user prompt describing the desired image.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="modelId">The model ID to use for generation.</param>
    /// <param name="baseUrl">The base URL of the image generation API.</param>
    /// <param name="context">Additional context (e.g., preset details) to guide the generation.</param>
    /// <returns>A Texture2D generated from the prompt and context.</returns>
    public static async Task<Texture2D> GenerateTextureFromPromptAsync(string prompt, string apiKey, string modelId, string baseUrl, string context)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("API Key is missing in Inventai settings.");

        // Build the full prompt with clear separation and improved instructions
        string fullPrompt =
            "You are generating a high-quality 2D game asset sprite for use in a professional game engine. " +
            "The image should depict a single, clearly defined subject, centered in the frame, with a fully transparent background. " +
            "Do not include any text, watermarks, borders, or extraneous elements. The sprite should be high resolution, clean, and ready for direct use in a 2D game. " +
            "If the user prompt does not specify a background, assume full transparency. " +
            (!string.IsNullOrWhiteSpace(context) ? $"Request context (style, genre, etc): {context} " : "") +
            $"User prompt (what the user wants to see): {prompt}";

        // Call the AI image generation API
        var result = await ImageGeneration.GenerateImageAsync(fullPrompt, apiKey, modelId, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] imageData = Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    /// <summary>
    /// Saves a Texture2D as a PNG file at the specified path.
    /// </summary>
    /// <param name="texture">The texture to save.</param>
    /// <param name="path">The file path to save the PNG.</param>
    public static void SaveTextureAsPng(Texture2D texture, string path)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
    }

    /// <summary>
    /// Edits an existing image using AI based on a prompt and saves the result as a Texture2D.
    /// </summary>
    /// <param name="imagePath">The path to the image to edit.</param>
    /// <param name="prompt">The prompt describing the desired edit.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="baseUrl">The base URL of the image editing API.</param>
    /// <returns>The edited Texture2D.</returns>
    public static async Task<Texture2D> EditImageWithGptAsync(string imagePath, string prompt, string apiKey, string baseUrl)
    {
        var result = await ImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] editedImageData = System.Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(editedImageData);
        return texture;
    }

    /// <summary>
    /// Generates a Texture2D from a fully custom prompt using the AI image generation API.
    /// </summary>
    /// <param name="fullPrompt">The full prompt to send to the API.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="modelId">The model ID to use for generation.</param>
    /// <param name="baseUrl">The base URL of the image generation API.</param>
    /// <returns>A Texture2D generated from the custom prompt.</returns>
    public static async Task<Texture2D> GenerateTextureFromCustomPromptAsync(string fullPrompt, string apiKey, string modelId, string baseUrl)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("API Key is missing in Inventai settings.");

        var result = await ImageGeneration.GenerateImageAsync(fullPrompt, apiKey, modelId, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] imageData = Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    /// <summary>
    /// Generates image data (PNG bytes) from a user prompt and context using the AI image generation API.
    /// </summary>
    /// <param name="prompt">The user prompt describing the desired image.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="modelId">The model ID to use for generation.</param>
    /// <param name="baseUrl">The base URL of the image generation API.</param>
    /// <param name="context">Additional context (e.g., preset details) to guide the generation.</param>
    /// <returns>PNG image data as a byte array.</returns>
    public static async Task<byte[]> GenerateImageDataFromPromptAsync(string prompt, string apiKey, string modelId, string baseUrl, string context)
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
        return Convert.FromBase64String(imageBase64);
    }

    /// <summary>
    /// Edits an existing image using AI based on a prompt and returns the result as a PNG byte array.
    /// </summary>
    /// <param name="imagePath">The path to the image to edit.</param>
    /// <param name="prompt">The prompt describing the desired edit.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="baseUrl">The base URL of the image editing API.</param>
    /// <returns>The edited image as a PNG byte array.</returns>
    public static async Task<byte[]> EditImageWithGptToBytesAsync(string imagePath, string prompt, string apiKey, string baseUrl)
    {
        var result = await ImageGeneration.EditImageWithGptAsync(imagePath, prompt, apiKey, baseUrl);
        string imageBase64 = result.ImageBase64;
        byte[] editedImageData = System.Convert.FromBase64String(imageBase64);
        return editedImageData;
    }
}
