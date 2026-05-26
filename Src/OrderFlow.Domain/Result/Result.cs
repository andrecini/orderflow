namespace OrderFlow.Domain.Result;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ResultCode Code { get; }
    public string? Message { get; }
    public int? StatusCode { get; }

    protected Result(bool isSuccess, ResultCode code, string? message, int? statusCode)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }

    public static Result Success() =>
        new(true, ResultCode.Success, null, null);

    public static Result Failure(ResultCode code, string message, int? statusCode = null) =>
        new(false, code, message, statusCode);
}
