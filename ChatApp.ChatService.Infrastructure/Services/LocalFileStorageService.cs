using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.ChatService.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ChatApp.ChatService.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _env;

        public LocalFileStorageService(IHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string subFolder)
        {
            var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var targetFolder = Path.Combine(wwwroot, subFolder);
            Directory.CreateDirectory(targetFolder);

            var filePath = Path.Combine(targetFolder, fileName);
            await using var output = File.Create(filePath);
            await fileStream.CopyToAsync(output);

            return $"{subFolder}/{fileName}".Replace("\\", "/");
        }

        public Task DeleteAsync(string fileName, string subFolder)
        {
            var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var file = Path.Combine(wwwroot, subFolder, fileName);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            return Task.CompletedTask;
        }
    }
}
