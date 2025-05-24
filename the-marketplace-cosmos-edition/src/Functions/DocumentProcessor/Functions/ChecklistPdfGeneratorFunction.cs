using DocumentProcessor.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using Azure.Storage.Blobs;
using DocumentProcessor.Models;
using QuestPDF.Fluent;
using Services.Core.Events.ChecklistsEvents;
using Services.Core.ServiceBus;

namespace DocumentProcessor.Functions;

public class ChecklistPdfGeneratorFunction(
    CosmosClient cosmosClient,
    DocumentProcessorServiceConfiguration configuration,
    BlobServiceClient blobServiceClient)
{
    [Function(nameof(ChecklistPdfGeneratorFunction))]
    public async Task Run(
        [ServiceBusTrigger(
            ServiceBusConstants.Topics.Checklist.Submitted,
            ServiceBusConstants.Topics.Checklist.Subscriptions.DocumentProcessorSubmitted,
            Connection = "ApplicationConfiguration:AzureServiceBusSettings:ConnectionString")] string messageJson,
        FunctionContext context)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<ChecklistSubmitted>>(messageJson);
            var message = envelope.Message;

            var container = cosmosClient.GetContainer(
                configuration.AzureCosmosDbSettings.DatabaseName,
                configuration.AzureCosmosDbSettings.ContainerName);
            
            var checklist = await container.ReadItemAsync<Checklist>(
                id: message.ChecklistId.ToString(),
                partitionKey: new PartitionKey(message.AccountId.ToString())
            );
            
            var document = new ChecklistDocument(checklist);
            var generatedPdf = document.GeneratePdf();

            // Get blob container
            var containerClient = blobServiceClient.GetBlobContainerClient("checklist-pdfs");
            await containerClient.CreateIfNotExistsAsync();
            
            // Generate unique blob name
            string blobName = $"checklist-{message.ChecklistId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            
            // Upload to blob storage
            var blobClient = containerClient.GetBlobClient(blobName);
            using (var ms = new MemoryStream(generatedPdf))
            {
                await blobClient.UploadAsync(ms, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}


