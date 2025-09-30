using TIENDAUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.DTO.BaseResponse
{
    public class GenericResponse<T>(string message, T? data = default)
    {
        public string Message { get; set; } = message;
        public T? Data { get; set; } = data;
    }
}
