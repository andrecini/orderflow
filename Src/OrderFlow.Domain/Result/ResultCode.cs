namespace OrderFlow.Domain.Result;

public enum ResultCode
{
    Success,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    ValidationError,
    BusinessError,
    InternalError
}
