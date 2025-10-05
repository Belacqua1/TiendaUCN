namespace TiendaUCN.src.Application.DTO.BaseResponse
{
    public class GenericResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public GenericResponse(string message, T? data = default, bool success = true)
        {
            Message = message;
            Data = data;
            Success = success;
        }
    }
}
