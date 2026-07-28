using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Order.Application.Common.Dtos;
using Order.Application.Common.Interfaces.Services;
using Order.Infrastructure.Settings;

namespace Order.Infrastructure.Services
{
    public sealed class CloudinaryFileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<UploadFileResponse> UploadAsync(
            FileUpload file,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);

            if (file.Content == Stream.Null)
                throw new ArgumentException("File stream is empty.", nameof(file));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(
                    file.FileName,
                    file.Content),

                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                throw new Exception(result.Error.Message);

            return new UploadFileResponse(
                result.SecureUrl.ToString(),
                result.PublicId);
        }

        public async Task<bool> DeleteAsync(
            string publicId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return false;

            var result = await _cloudinary.DestroyAsync(
                new DeletionParams(publicId));

            return result.Result == "ok";
        }
    }
}
