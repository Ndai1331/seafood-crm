using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data;
public class ResponseHttpBase<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; } = default(T);
        
        [JsonProperty("total")]
        public int Total { get; set; } = 0;
        
        [JsonProperty("status")]
        public bool Status { get; set; } = true;
        
        [JsonProperty("message")]
        public string? Message  { get; set; } = null;
    }

    public class ResponseHttpBaseBool : ResponseHttpBase<bool> { }
    public class ResponseHttpBaseInt : ResponseHttpBase<int> { }
    public class ResponseHttpBaseString : ResponseHttpBase<string> { }
    public class ResponseHttpBaseObject : ResponseHttpBase<object> { }
    public class ResponseHttpBaseList<T> : ResponseHttpBase<List<T>> { }


