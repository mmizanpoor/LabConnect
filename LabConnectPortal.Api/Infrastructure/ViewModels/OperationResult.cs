namespace LabConnectPortal.Api.Infrastructure.ViewModels;

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

    public static OperationResult SuccessResult()
        => new() { Status = true };

    public static OperationResult Failure(string message, List<string>? errors = null)
        => new()
        {
            Status = false,
            Message = message,
            Errors = errors
        };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static new OperationResult<T> Success(T data)
        => new()
        {
            Status = true,
            Data = data
        };

    public new static OperationResult<T> Failure(string message, List<string>? errors = null)
        => new()
        {
            Status = false,
            Message = message,
            Errors = errors
        };
}
