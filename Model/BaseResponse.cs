namespace ApiOnLamda.Model
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse(T data = default, List<string> errors = null)
        {
            Data = data;
            Console.WriteLine("data:", Data);
            Errors = errors ?? new List<string>();
        }
    }
}
