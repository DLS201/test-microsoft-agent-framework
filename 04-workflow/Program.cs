// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace WorkflowFoundryAgentSample;

public static class Program
{
    private static async Task Main()
    {
        // Set up the Azure AI Project client
        var endpoint = "REPLACE_WITH_FOUNDRY_ENDPOINT";
        var deploymentName = "DeepSeek-V4-Flash-0731";
        var aiProjectClient = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());

        // Create agents
        AIAgent frenchAgent = await CreateTranslationAgentAsync("French", aiProjectClient, deploymentName);
        AIAgent spanishAgent = await CreateTranslationAgentAsync("Spanish", aiProjectClient, deploymentName);
        AIAgent englishAgent = await CreateTranslationAgentAsync("English", aiProjectClient, deploymentName);

        try
        {
            // Build the workflow by adding executors and connecting them
            var workflow = new WorkflowBuilder(frenchAgent)
                .AddEdge(frenchAgent, spanishAgent)
                .AddEdge(spanishAgent, englishAgent)
                .Build();

            // Execute the workflow
            await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new ChatMessage(ChatRole.User, "Hello World!"));

            // Must send the turn token to trigger the agents.
            // The agents are wrapped as executors. When they receive messages,
            // they will cache the messages and only start processing when they receive a TurnToken.
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
            
            await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            {
                if (evt is AgentResponseUpdateEvent executorComplete)
                {
                    Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
                }
                else if (evt is WorkflowErrorEvent workflowError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(workflowError.Exception?.ToString() ?? "Unknown workflow error occurred.");
                    Console.ResetColor();
                }
                else if (evt is ExecutorFailedEvent executorFailed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Executor '{executorFailed.ExecutorId}' failed with {(executorFailed.Data == null ? "unknown error" : $"exception {executorFailed.Data}")}.");
                    Console.ResetColor();
                }
            }
        }
        finally
        {
            Console.ReadLine();
            // Cleanup the agents created for the sample.
            await aiProjectClient.AgentAdministrationClient.DeleteAgentAsync(frenchAgent.Name);
            await aiProjectClient.AgentAdministrationClient.DeleteAgentAsync(spanishAgent.Name);
            await aiProjectClient.AgentAdministrationClient.DeleteAgentAsync(englishAgent.Name);
        }
    }

    /// <summary>
    /// Creates a translation agent for the specified target language.
    /// </summary>
    /// <param name="targetLanguage">The target language for translation</param>
    /// <param name="aiProjectClient">The <see cref="AIProjectClient"/> to create the agent with.</param>
    /// <param name="model">The model to use for the agent</param>
    /// <returns>A FoundryAgent configured for the specified language</returns>
    private static async Task<FoundryAgent> CreateTranslationAgentAsync(
        string targetLanguage,
        AIProjectClient aiProjectClient,
        string model)
    {
        ProjectsAgentVersion agentVersion = await aiProjectClient.AgentAdministrationClient.CreateAgentVersionAsync(
            $"{targetLanguage}Translator",
            new ProjectsAgentVersionCreationOptions(
                new DeclarativeAgentDefinition(model: model)
                {
                    Instructions = $"You are a translation assistant that translates the provided text to {targetLanguage}.",
                }));
        return aiProjectClient.AsAIAgent(agentVersion);
    }
}

/*
4
FrenchTranslator_FrenchTranslator_1: 
FrenchTranslator_FrenchTranslator_1: Bon
FrenchTranslator_FrenchTranslator_1: jour le
FrenchTranslator_FrenchTranslator_1:  monde !
4
FrenchTranslator_FrenchTranslator_1: 
4
SpanishTranslator_SpanishTranslator_1: 
SpanishTranslator_SpanishTranslator_1: ¡
SpanishTranslator_SpanishTranslator_1: Hola Mundo
SpanishTranslator_SpanishTranslator_1: !

¡
SpanishTranslator_SpanishTranslator_1: H
SpanishTranslator_SpanishTranslator_1: ola mundo!
4
SpanishTranslator_SpanishTranslator_1: 
4
EnglishTranslator_EnglishTranslator_1: 
EnglishTranslator_EnglishTranslator_1: Hello
EnglishTranslator_EnglishTranslator_1:  World!


EnglishTranslator_EnglishTranslator_1: Hello world
EnglishTranslator_EnglishTranslator_1: !

Hello
EnglishTranslator_EnglishTranslator_1:  World!

Hello world!
4
EnglishTranslator_EnglishTranslator_1: 
*/