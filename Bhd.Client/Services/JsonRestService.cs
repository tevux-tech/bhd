using System.Net;
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

        public async Task<RestResponse<T>> GetAsync<T>(string url) {
            var httpResponse = await _httpClient.GetAsync(url);
            var restResponse = new RestResponse<T>();
            restResponse.StatusCode = httpResponse.StatusCode;
            restResponse.Body = await httpResponse.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);
            return restResponse;
        }

        public async Task<HttpStatusCode> PutAsync<T>(string url, T newValue) {
            var httpContent = JsonContent.Create(newValue, null, _jsonSerializerOptions);
            var httpResponse = await _httpClient.PutAsync(url, httpContent);
            return httpResponse.StatusCode;
        }

        public async Task<HttpStatusCode> PostAsync(string url) {
            var httpContent = new ByteArrayContent(new byte[0]);
            var httpResponse = await _httpClient.PostAsync(url, httpContent);
            return httpResponse.StatusCode;
        }

        public async Task<HttpStatusCode> PostAsync<T>(string url, T body) {
            var httpContent = JsonContent.Create(body, null, _jsonSerializerOptions);
            var httpResponse = await _httpClient.PostAsync(url, httpContent);
            return httpResponse.StatusCode;
        }

        public async Task<RestResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body) {
            var httpContent = JsonContent.Create(body, null, _jsonSerializerOptions);
            var httpResponse = await _httpClient.PostAsync(url, httpContent);

            var restResponse = new RestResponse<TResponse>();
            restResponse.StatusCode = httpResponse.StatusCode;
            restResponse.Body = await httpResponse.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions);

            return restResponse;
        }
    }
}