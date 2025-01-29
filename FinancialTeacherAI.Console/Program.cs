using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

var apiKey = config["AzureOpenAIChatCompletion:ApiKey"];
var endpoint = config["AzureOpenAIChatCompletion:Endpoint"];
var deployment = config["AzureOpenAIChatCompletion:DeploymentName"];

var host = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
    // Register services
    services.AddTransient<IEmbeddingService, EmbeddingService>();
    services.AddTransient<IPineconeService, PineconeService>();
    services.AddSingleton<IChatCompletionService>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        
        return new AzureOpenAIChatCompletionService(deployment!, endpoint!, apiKey!);
    });

    services.AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: "text-embedding-ada-002",
                endpoint,
                apiKey
            );

    services.AddKeyedTransient("FinancialAIKernel", (sp, key) =>
    {
        // Create a collection of plugins that the kernel will use
        KernelPluginCollection pluginCollection = new();
        return new Kernel(sp, pluginCollection);
    });

    // Add enterprise components
    services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
})
.Build();

var settings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    ChatSystemPrompt = @"You are a financial teacher's assisstant. You are tasked with giving score to an exam solved by a student.
                        You will be provided with the questions for the exam.
                        First thing you will do is give the student the exam questions and ask the student to answer them as a bullet point separated by a line break (wait for his answer).
                        Once you recieve the answers from the student you will accomplish the next tasks for each question and answer:
                        1- Generate the embeddings from the student's answer and retrieve the relevant chunks using those generated embeddings.
                        2- Use the generated chunks to answer the exam question from your perspective and retrieve facts for that answer.
                        3- Use the generated chunks to retrieve facts from the student's answer.
                        Parallelize when possible.
                        Once the facts for both your answer as well as the student's answer are generated, compare them, and state why you think the student's answer is correct or not, then score it.
                        "
};

// Initialize the Semantic Kernel
var kernel = host.Services.GetKeyedService<Kernel>("FinancialAIKernel");
kernel.ImportPluginFromType<EmbeddingService>();
kernel.ImportPluginFromType<PineconeService>();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

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

    var message = new ChatMessageContent(AuthorRole.User, input);
    chatHistory.Add(message);

    var response = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel
    );

    Console.WriteLine(response);
}
while (true);