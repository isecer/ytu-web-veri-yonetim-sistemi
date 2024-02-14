using WebApp.Utilities.MessageBox;

namespace WebApp.Utilities.Results
{
    public class DataResult<T> : Result, IDataResult<T>
    {
        public DataResult(T data, bool success, MmMessage message) : base(success, message)
        {
            Data = data;
            Message = message;
        }

        public DataResult(T data, bool success) : base(success)
        {
            Data = data;
        }
        public MmMessage Message { get; }
        public T Data { get; }
    }
}
