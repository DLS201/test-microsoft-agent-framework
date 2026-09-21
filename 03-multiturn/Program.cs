using System;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var endpoint = "REPLACE_WITH_FOUNDRY_ENDPOINT";
var deploymentName = "DeepSeek-V4-Flash-0731";

AIAgent agent = new AIProjectClient(new Uri(endpoint), new AzureCliCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: "You are a friendly assistant. Keep your answers brief.",
        name: "ConversationAgent");

// Create a session to maintain conversation history
AgentSession session = await agent.CreateSessionAsync();

// First turn
Console.WriteLine(await agent.RunAsync("My name is Alice and I love hiking.", session));

// Second turn — the agent remembers the user's name and hobby
Console.WriteLine(await agent.RunAsync("What do you remember about me?", session));

// Same question without session
Console.WriteLine(await agent.RunAsync("What do you remember about me?"));

// ANSWERS
/*
Nice to meet you, Alice! Hiking is a fantastic way to explore nature. Do you have a favorite trail or a dream hike you’d love to do?
I remember that your name is Alice, and you love hiking. That's what you've shared with me so far! What else would you like me to know?
As an AI, I don’t have memory of past conversations unless you tell me something within this chat. If you’d like, you can remind me of your name, interests, or anything you’d like me to keep in mind—and I’ll use it for our current session! 😊
*/