namespace TiendaUCN.src.Application.DTO.BaseResponse
{
    /// <summary>
    /// Standard response wrapper for API responses.
    /// Contains a success flag, message, and optional data.
    /// </summary>
    /// <typeparam name="T">Type of the response data.</typeparam>
    public class GenericResponse<T>
    {
        // Indicates if the operation was successful
        public bool Success { get; set; }

        // Message describing the result
        public string Message { get; set; }

        // Optional data returned with the response
        public T? Data { get; set; }

        /// <summary>
        /// Creates a new generic response.
        /// </summary>
        /// <param name="message">Result message</param>
        /// <param name="data">Optional data</param>
        /// <param name="success">Indicates success (default true)</param>
        public GenericResponse(string message, T? data = default, bool success = true)
        {
            Message = message;
            Data = data;
            Success = success;
        }
    }
}
