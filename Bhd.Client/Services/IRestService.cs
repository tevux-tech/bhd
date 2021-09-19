using System.Net;
using System.Threading.Tasks;

namespace Bhd.Client.Services {
    public interface IRestService {
        Task<RestResponse<T>> GetAsync<T>(string url);
        Task<HttpStatusCode> PutAsync<T>(string url, T newValue);
        Task<HttpStatusCode> PostAsync(string url);
        Task<HttpStatusCode> PostAsync<T>(string url, T body);
        Task<RestResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body);
    }

    public class RestResponse<T> {
        public HttpStatusCode StatusCode { get; set; }
        public T Body { get; set; }
    }
}