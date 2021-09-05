using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bhd.Client.Services {
    public class JsonRestService : IRestService {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonRestService(HttpClient httpClient) {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.PropertyNamingPolicy = null;
            _jsonSerializerOptions.IgnoreNullValues = true;

            _httpClient = httpClient;
        }

        public Task<T> GetAsync<T>(string url) {
            return _httpClient.GetFromJsonAsync<T>(url, _jsonSerializerOptions);
        }

        public Task PutAsync<T>(string url, T newValue) {
           return _httpClient.PutAsJsonAsync(url, newValue, _jsonSerializerOptions);
        }

        public Task Post(string url) {
            return _httpClient.PostAsync(url, new ByteArrayContent(new byte[0]));
        }

        public Task PostAsync<T>(string url, T body) {
            return _httpClient.PostAsJsonAsync(url, body, _jsonSerializerOptions);
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body) {
            var response =  await  _httpClient.PostAsJsonAsync(url, body, _jsonSerializerOptions);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}