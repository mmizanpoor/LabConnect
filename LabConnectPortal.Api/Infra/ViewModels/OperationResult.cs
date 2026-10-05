namespace LabConnectPortal.Infra.ViewModels
{
    public class OperationResult
    {
        public bool Status { get; set; }

        public bool Success
        {
            get => Status;
            set => Status = value;
        }

        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static OperationResult Success()
            => new OperationResult { Status = true };

        public static OperationResult Failure(string message)
            => new OperationResult
            {
                Status = false,
                Message = message
            };
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> Success(T data)
            => new OperationResult<T>
            {
                Status = true,
                Data = data
            };
    }
}
