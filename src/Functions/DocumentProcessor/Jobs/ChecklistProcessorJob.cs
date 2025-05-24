using DocumentProcessor.Models;
using DocumentProcessor.Settings;
using Hangfire;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using MongoDB.Driver;
using QuestPDF.Fluent;
using Services.Core.Events.ChecklistsEvents;

namespace DocumentProcessor.Jobs;

public class ChecklistProcessorJob
{
    private readonly IMongoCollection<Checklist> _checklistCollection;
    private readonly IMinioClient _minioClient;
    private readonly DocumentProcessorServiceConfiguration _configuration;
    private readonly ILogger<ChecklistProcessorJob> _logger;

    public ChecklistProcessorJob(
        IMongoDatabase mongoDatabase,
        IMinioClient minioClient,
        DocumentProcessorServiceConfiguration configuration,
        ILogger<ChecklistProcessorJob> logger)
    {
        _checklistCollection = mongoDatabase.GetCollection<Checklist>("checklists");
        _minioClient = minioClient;
        _configuration = configuration;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ProcessChecklistAsync(ChecklistSubmitted message)
    {
        try
        {
            _logger.LogInformation("Processing checklist {ChecklistId} for account {AccountId}", 
                message.ChecklistId, message.AccountId);

            // Fetch checklist from MongoDB (previously Cosmos DB)
            var filter = Builders<Checklist>.Filter.Eq(c => c.id, message.ChecklistId);
            var checklist = await _checklistCollection.Find(filter).FirstOrDefaultAsync();
            
            if (checklist == null)
            {
                _logger.LogError("Checklist {ChecklistId} not found", message.ChecklistId);
                throw new InvalidOperationException($"Checklist {message.ChecklistId} not found");
            }

            // Generate PDF using existing logic
            var document = new ChecklistDocument(checklist);
            var generatedPdf = document.GeneratePdf();

            // Upload to MinIO (previously Azure Blob Storage)
            await UploadToMinIO(message.ChecklistId, generatedPdf);

            _logger.LogInformation("Successfully processed checklist {ChecklistId}", message.ChecklistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process checklist {ChecklistId}", message.ChecklistId);
            throw;
        }
    }

    private async Task UploadToMinIO(Guid checklistId, byte[] pdfData)
    {
        const string bucketName = "checklist-pdfs";
        
        // Ensure bucket exists
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(bucketName));
        
        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs()
                .WithBucket(bucketName));
        }

        // Generate unique file name
        string fileName = $"checklist-{checklistId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        
        // Upload PDF
        using (var stream = new MemoryStream(pdfData))
        {
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/pdf"));
        }

        _logger.LogInformation("Uploaded PDF {FileName} to MinIO bucket {BucketName}", 
            fileName, bucketName);
    }
}