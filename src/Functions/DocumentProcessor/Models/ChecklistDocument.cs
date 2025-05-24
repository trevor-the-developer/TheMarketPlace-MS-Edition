using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DocumentProcessor.Models;

public class ChecklistDocument(Checklist model) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
            
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text("Checklist Created")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().Text(text =>
                {
                    text.Span("Checklist ID: ").SemiBold();
                    text.Span($"{model.id}");
                });
                
                column.Item().Text(text =>
                {
                    text.Span("Account ID: ").SemiBold();
                    text.Span($"{model.AccountId}");
                });
                
                column.Item().Text(text =>
                {
                    text.Span("Created date: ").SemiBold();
                    text.Span($"{model.CreatedAt:d}");
                });
                
                column.Item().Text(text =>
                {
                    text.Span("Submitted date: ").SemiBold();
                    text.Span($"{model.SubmittedAt:d}");
                });
            });

            row.ConstantItem(100).Height(50).Placeholder();
        });
    }

    void ComposeContent(IContainer container)
    {
        container
            .PaddingVertical(40)
            .Height(250)
            .Background(Colors.Grey.Lighten3)
            .AlignCenter()
            .AlignMiddle()
            .Text("Content").FontSize(16);
    }
}