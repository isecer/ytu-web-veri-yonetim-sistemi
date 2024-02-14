using WebApp.Utilities.MessageBox;

namespace WebApp.Utilities.Results
{
    public class Result : IResult
    {
        public Result(bool success, MmMessage messages) : this(success)
        {
            Success = success;
            Message = messages;
        }

        protected Result(bool success)
        {
            Success = success;
        }
        public bool Success { get; }
        public MmMessage Message { get; }
    }
}
