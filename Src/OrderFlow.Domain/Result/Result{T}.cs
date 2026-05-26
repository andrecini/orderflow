namespace OrderFlow.Domain.Result;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, ResultCode code, string? message, int? statusCode, T? value)
        : base(isSuccess, code, message, statusCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) =>
        new(true, ResultCode.Success, null, null, value);

    public static new Result<T> Failure(ResultCode code, string message, int? statusCode = null) =>
        new(false, code, message, statusCode, default);
}
