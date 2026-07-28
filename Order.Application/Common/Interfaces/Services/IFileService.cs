using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order.Application.Common.Dtos;


namespace Order.Application.Common.Interfaces.Services
{
    public interface IFileService
    {
        Task<UploadFileResponse> UploadAsync(
            FileUpload file,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            string publicId,
            CancellationToken cancellationToken = default);
    }
}
