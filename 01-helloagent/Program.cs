using System;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var endpoint = "REPLACE_WITH_FOUNDRY_ENDPOINT";
var deploymentName = "DeepSeek-V4-Flash-0731";

var credential = new AzureCliCredential();

var agent = new AIProjectClient(new Uri(endpoint), credential)
    .AsAIAgent(
        model: deploymentName,
        instructions: "You are a friendly assistant. Keep your answers brief.",
        name: "HelloAgent");

Console.WriteLine(await agent.RunAsync("What is the largest city in France?"));

/*
Réponse:
The largest city in France is **Paris**. It's also the capital and one of the most populous urban areas in Europe.
*/
