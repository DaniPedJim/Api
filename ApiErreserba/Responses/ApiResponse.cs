namespace ApiErreserba.Responses
{
    public class ApiResponse<T>
    {
        public string Status { get; init; } = "success";
        public int Code { get; set; }
        public string Message { get; init; }
        public string Type { get; init; }
        public T? Data { get; init; }
        public List<T>? DataList { get; init; }

        public static ApiResponse<T> Ok(T data, int code, string message = "OK")
            => new() { Data = data, Type = "element", Code = code, Message = message };
        public static ApiResponse<T> Ok(List<T> datalist, int code, string message = "OK")
            => new() { DataList = datalist, Type = "list", Code = code, Message = message };

    }
}
