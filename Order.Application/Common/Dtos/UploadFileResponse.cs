using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Common.Dtos;
public sealed record UploadFileResponse(
    string Url,
    string PublicId);