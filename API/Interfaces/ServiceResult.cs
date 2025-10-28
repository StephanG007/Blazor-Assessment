using System;
using System.Collections.Generic;
using System.Linq;

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
    private ServiceResult(ServiceStatus status, T? data, IReadOnlyList<string> errors)
    {
        Status = status;
        Data = data;
        Errors = errors;
    }

    public ServiceStatus Status { get; }

    public T? Data { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ServiceResult<T> Success(T? data = default) =>
        Create(ServiceStatus.Success, data, null);

    public static ServiceResult<T> NotFound(params string[] errors) =>
        Create(ServiceStatus.NotFound, default, errors);

    public static ServiceResult<T> Conflict(T? data = default, params string[] errors) =>
        Create(ServiceStatus.Conflict, data, errors);

    public static ServiceResult<T> Unauthorized(params string[] errors) =>
        Create(ServiceStatus.Unauthorized, default, errors);

    public static ServiceResult<T> Unexpected(params string[] errors) =>
        Create(ServiceStatus.Unexpected, default, errors);

    public static ServiceResult<T> From(ServiceStatus status, T? data = default, params string[] errors) =>
        Create(status, data, errors);

    private static ServiceResult<T> Create(ServiceStatus status, T? data, string[]? errors)
    {
        var normalizedErrors = errors is { Length: > 0 }
            ? errors.Where(static error => !string.IsNullOrWhiteSpace(error)).ToArray()
            : Array.Empty<string>();

        return new ServiceResult<T>(status, data, normalizedErrors);
    }
}
