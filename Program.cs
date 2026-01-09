#:package Microsoft.Extensions.AI@10.1.1
#:package Microsoft.Extensions.Logging.Console@10.0.1
#:package OllamaSharp@5.4.12

#:property PublishAot=false

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");

using var httpClient = new HttpClient()
{
    Timeout = TimeSpan.FromMinutes(10),
    BaseAddress = new Uri("http://localhost:11434")
};

var chatClient = new OllamaApiClient(httpClient, "llava:latest");

var chatHistory = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful assistant that can analyze images."),
};
Console.WriteLine("Enter the image path to analyze:");
var imagePath = Console.ReadLine();
if(string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
{
    logger.LogError("Invalid image path provided.");
    return;
}

var userMessage = new ChatMessage(ChatRole.User, "Describe the image");
var imageBytes = await File.ReadAllBytesAsync(imagePath);
var mediaType = "image/jpeg"; // Adjust based on your image type
userMessage.Contents.Add(new DataContent(imageBytes, mediaType));

chatHistory.Add(userMessage);
string response = "";
await foreach (ChatResponseUpdate item in chatClient.GetStreamingResponseAsync(userMessage))
{
    response += item.Text;
}

Console.WriteLine(response);
chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));