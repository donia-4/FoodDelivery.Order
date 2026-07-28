using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Common.Dtos
{
    public sealed record FileUpload(
        Stream Content,
        string FileName,
        string ContentType);
}
