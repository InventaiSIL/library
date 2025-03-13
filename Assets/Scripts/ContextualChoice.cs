using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

// Inventai imports
using Inventai;
using Inventai.TextAgents;
using Inventai.Core.Discussion;
using Inventai.Discussion;

public class ContextualChoice : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_InputField numChoicesInputField;
    public TMP_Text responseText;
    public Button submitButton;

    void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitPrompt);
        }
        else
        {
            Debug.LogError("Submit Button is not assigned!");
        }

        // Clear the response text initially
        if (responseText != null)
        {
            responseText.text = "";
        }
    }

    void SubmitPrompt()
    {
        if (inputField != null && numChoicesInputField != null && responseText != null)
        {
            string prompt = inputField.text;
            int numChoices = int.Parse(numChoicesInputField.text);
            string response = ProcessPrompt(prompt, numChoices);
            responseText.text = response;
        }
        else
        {
            Debug.LogError("InputField or ResponseText is not assigned!");
        }
    }

    string ProcessPrompt(string prompt, int numChoices)
    {
        string? openaiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openaiApiKey))
        { 
            Console.WriteLine("OPENAI - OPENAI_API_KEY environment variable is not set");
            return "OPENAI - OPENAI_API_KEY environment variable is not set";
        }
        else
        {
            TextAgentOpenAI agent = new("gpt-3.5-turbo", openaiApiKey);

            DiscussionContextManager discussionContextManager = new(agent);

            Inventai.Core.Discussion.ContextualChoicesRequest request = new()
            {
                Prompt = prompt,
                Context = "You are a person who is trying to be good",
                NumChoices = numChoices
            };

            Inventai.Core.Discussion.ContextualChoicesResponse response = discussionContextManager.GenerateContextualChoices(request);

            return string.Join("\n", response.Choices);
        }
    }
}