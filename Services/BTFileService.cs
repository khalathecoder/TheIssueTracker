﻿using TheIssueTracker.Models.Enums;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string _defaultImage = "/img/undraw_default.png"; 
        private readonly string _defaultBTUserImageSrc = "/img/undraw_user.png";
        private readonly string _defaultCompanyImageSrc = "/img/undraw_company.png";
        private readonly string _defaultProjectImageSrc = "/img/undraw_project.png";

        public string ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
        {
            if (fileData is null || string.IsNullOrEmpty(extension))
            {
                return defaultImage switch
                {
                    DefaultImage.BTUserImage => _defaultBTUserImageSrc,
                    DefaultImage.CompanyImage => _defaultCompanyImageSrc,
                    DefaultImage.ProjectImage => _defaultProjectImageSrc,
                    _ => _defaultImage,
                };
            }
            try
            {
                return string.Format($"data:{extension};base64,{Convert.ToBase64String(fileData)}");
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch
            {
                throw;
            }
        }
    }
}
