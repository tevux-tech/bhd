using System.Threading.Tasks;

namespace Bhd.Client.Services {
    public interface IRestService {
        Task<T> GetAsync<T>(string url);
        Task PutAsync<T>(string url, T newValue);
        Task Post(string url);
        Task PostAsync<T>(string url, T body);
        Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body);
    }
}