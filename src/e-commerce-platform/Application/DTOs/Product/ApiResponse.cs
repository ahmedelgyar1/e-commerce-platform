namespace e_commerce_platform.Application.DTOs.Product;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
}
