namespace LearnBase.API.DTOs.Shared;

/// <summary>
/// Standard API response wrapper for consistent responses
/// </summary>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    /// <summary>Success response with data</summary>
    public static ApiResponseDto<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>Error response with message</summary>
    public static ApiResponseDto<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}