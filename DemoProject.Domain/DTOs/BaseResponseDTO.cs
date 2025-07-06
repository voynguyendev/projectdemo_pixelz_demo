
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DemoProject.Domain.DTOs
{
    public class BaseResponseDTO<T>
    {
        public T Data { get; set; }

       
        public bool IsSuccess { get; set; } = true;
      
        public string Message { get; set; } = "";

        public int StatusCode   { get; set; }

    }
    public class BaseResponseDTO
    {
        
        public bool IsSuccess { get; set; } = true;
       
        public string Message { get; set; } = "";

        public int StatusCode { get; set; }

    }
}
