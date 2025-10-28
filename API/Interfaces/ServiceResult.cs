namespace API.Interfaces;

public enum ServiceStatus
{
    Success,
    NotFound,
    Conflict,
    Unauthorized,
    Unexpected
}

public sealed class ServiceResult<T>
{
    public ServiceStatus Status { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];
}