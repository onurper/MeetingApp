using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;

namespace MeetingApp.Service.Services;

public class FileService(IWebHostEnvironment env)
{
    public async Task<string> SaveAndCompressFileAsync(Stream fileStream, string fileName)
    {
        string uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "document");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string zipFilePath = Path.Combine(uploadsFolder, $"{Path.GetFileNameWithoutExtension(fileName)}.zip");

        using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            var zipEntry = archive.CreateEntry(fileName);

            using (var zipEntryStream = zipEntry.Open())
            {
                await fileStream.CopyToAsync(zipEntryStream);
            }
        }

        return zipFilePath;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        string uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "document");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileOutput);
        }

        return filePath;
    }
}