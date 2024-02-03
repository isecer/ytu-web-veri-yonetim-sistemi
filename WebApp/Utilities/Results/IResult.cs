using System.Collections.Generic;
using WebApp.Utilities.MessageBox;

namespace WebApp.Utilities.Results
{
    public interface IResult
    {
        bool Success { get; }
        MmMessage Message { get; }
    }
}
