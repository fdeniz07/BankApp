using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>
/// Seeds tiny fake PDF/JPEG files under App_Data for pending applications (demo RBAC lab).
/// </summary>
public static class CreditApplicationDocumentSeed
{
    private const int MaxApplications = 24;

    // Minimal valid-ish JPEG (1x1) and PDF bytes for demo download.
    private static readonly byte[] TinyJpeg =
    [
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
        0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
        0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
        0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
        0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
        0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
        0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
        0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
        0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x08, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00,
        0x7F, 0xFF, 0xD9
    ];

    private static readonly byte[] TinyPdf =
        "%PDF-1.1\n1 0 obj<<>>endobj\n2 0 obj<</Length 44>>stream\nBT /F1 12 Tf 100 700 Td (BankApp demo) Tj ET\nendstream\nendobj\ntrailer<</Root 1 0 R>>\n%%EOF\n"u8.ToArray();

    public static async Task SeedAsync(BankingDbContext context, string contentRootPath)
    {
        string root = Path.GetFullPath(Path.Combine(contentRootPath, "App_Data", "credit-documents"));
        Directory.CreateDirectory(root);

        ApplicationUser? uploader = await context.Users
            .Where(u => u.CustomerId == null && u.IsActive)
            .OrderBy(u => u.Email)
            .FirstOrDefaultAsync();
        if (uploader is null)
            return;

        List<CreditApplication> pending = await context.CreditApplications
            .Where(a => !a.IsDeleted && a.Status == CreditApplicationStatus.Pending)
            .OrderBy(a => a.ApplicationDate)
            .Take(MaxApplications)
            .ToListAsync();

        bool changed = false;
        foreach (CreditApplication application in pending)
        {
            bool hasDocs = await context.CreditApplicationDocuments
                .AnyAsync(d => d.CreditApplicationId == application.Id && !d.IsDeleted);
            if (hasDocs)
                continue;

            changed |= await AddDocumentAsync(
                context,
                root,
                application.Id,
                uploader.Id,
                CreditDocumentType.IdentityDocument,
                "identity-demo.jpg",
                "image/jpeg",
                TinyJpeg);

            changed |= await AddDocumentAsync(
                context,
                root,
                application.Id,
                uploader.Id,
                CreditDocumentType.IncomeProof,
                "income-demo.pdf",
                "application/pdf",
                TinyPdf);
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static async Task<bool> AddDocumentAsync(
        BankingDbContext context,
        string root,
        Guid applicationId,
        Guid uploaderId,
        CreditDocumentType type,
        string fileName,
        string contentType,
        byte[] bytes)
    {
        Guid documentId = Guid.NewGuid();
        string relativeKey = $"{applicationId:N}/{documentId:N}{(contentType.Contains("pdf") ? ".pdf" : ".jpg")}";
        string absolutePath = Path.Combine(root, relativeKey.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllBytesAsync(absolutePath, bytes);

        await context.CreditApplicationDocuments.AddAsync(new CreditApplicationDocument
        {
            Id = documentId,
            CreditApplicationId = applicationId,
            DocumentType = type,
            OriginalFileName = fileName,
            ContentType = contentType,
            SizeBytes = bytes.Length,
            StoragePath = relativeKey.Replace('\\', '/'),
            UploadedByUserId = uploaderId,
            UploadedAt = DateTimeOffset.UtcNow
        });

        return true;
    }
}
