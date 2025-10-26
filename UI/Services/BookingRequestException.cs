using System;
using System.Net;

namespace UI.Services;

public sealed class BookingRequestException : Exception
{
    public BookingRequestException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
