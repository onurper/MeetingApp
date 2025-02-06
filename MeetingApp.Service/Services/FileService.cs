using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;

namespace MeetingApp.Service.Services;

public class FileService(IWebHostEnvironment env)
{
    public async Task<string> SaveAndCompressFileAsync(Stream fileStream, string fileName)
    {
        var uploadsFolder = GetPath();

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string zipFileName = $"{Guid.NewGuid()}.zip";
        string zipFilePath = Path.Combine(uploadsFolder, zipFileName);

        using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            var zipEntry = archive.CreateEntry(fileName);

            using (var zipEntryStream = zipEntry.Open())
            {
                await fileStream.CopyToAsync(zipEntryStream);
            }
        }

        return Path.Combine("document", zipFileName);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        var uploadsFolder = GetPath();

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        string filePath = Path.Combine(uploadsFolder, newFileName);

        using (var fileOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileOutput);
        }

        return Path.Combine("document", newFileName);
    }

    private string GetPath()
    {
        string wwwRootFolder = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string documentFolder = Path.Combine(wwwRootFolder, "document");

        if (!Directory.Exists(documentFolder))
        {
            Directory.CreateDirectory(documentFolder);
        }

        return documentFolder;
    }
}