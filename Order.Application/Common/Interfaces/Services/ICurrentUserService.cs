using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Common.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
    }
}
