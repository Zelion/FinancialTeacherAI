using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var kernel = CreateKernelWithChatCompletion();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var settings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    ChatSystemPrompt = @"You are a financial teacher's assisstant. You are tasked with giving score to an exam solved by a student (user).
                        Provide the exam questions to the student and wait for him to answer.
                        Don't provide the answers to the student. Evaluate the student's answer and provide a score based on the correctness of the answer.
                        Once you are done with all the questions, take each score from the questions and give the average score of the overall exam from 1 to 10.",
};

var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage(@"Exam Questions: 
                            1. What is Finance?
                            2. What is Working Capital Management?");

string input;
do
{
    input = Console.ReadLine();
    if (input.ToLower().Equals("exit"))
    {
        break;
    }

    chatHistory.AddUserMessage(input);

    var response = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel
    );

    Console.WriteLine(response);
}
while (true);


static Kernel CreateKernelWithChatCompletion()
{
    var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build();

    var apiKey = configuration["AzureOpenAIChatCompletion:ApiKey"];
    var endpoint = configuration["AzureOpenAIChatCompletion:Endpoint"];
    var deployment = configuration["AzureOpenAIChatCompletion:DeploymentName"];

    var builder = Kernel.CreateBuilder();

    builder.Services.AddAzureOpenAIChatCompletion(
        deployment!,
        endpoint!,
        apiKey!,
        "gpt-4o");
    builder.Services.AddAzureOpenAITextEmbeddingGeneration(
        deploymentName: "text-embedding-ada-002",
        endpoint,
        apiKey
    );

    builder.Services.AddSingleton<IConfiguration>(configuration);
    builder.Services.AddTransient(sp =>
    {
        // Create a collection of plugins that the kernel will use
        KernelPluginCollection pluginCollection = new();
        return new Kernel(sp, pluginCollection);
    });
    builder.Services.AddTransient<IEmbeddingService, EmbeddingService>();
    builder.Services.AddTransient<IPineconeService, PineconeService>();
    builder.Services.AddSingleton<IChatCompletionService>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        return new AzureOpenAIChatCompletionService(deployment!, endpoint!, apiKey!);
    });

    builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

    var kernel = builder.Build();
    kernel.ImportPluginFromType<PineconeService>();
    kernel.ImportPluginFromType<PromptService>();

    return kernel;
}