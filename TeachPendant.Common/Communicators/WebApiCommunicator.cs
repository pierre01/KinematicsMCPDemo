using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Biosero.TeachPendant.Common.Communicators
{
    internal class WebApiCommunicator
    {
        private string _url;

        private HttpClient _client;

        internal WebApiCommunicator(string url)
        {
            _client = new HttpClient();
            if (!url.EndsWith("/")) { url += "/"; }
            _url = url;
            _client.BaseAddress = new Uri(_url);

        }

        internal void Connect()
        {
            // TODO: get token or something to ensure connection parmeters are good
        }

        internal async Task<T> GetAsync<T>(string path)
        {
            path = path.TrimStart('/');
            var response = await _client.GetAsync(path).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"path: {path} response: {(int)response.StatusCode} {response.ReasonPhrase}");
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var objectResponse = JsonConvert.DeserializeObject<T>(jsonResponse);
                return objectResponse;
            }
        }

        internal async Task<string> GetJsonAsync(string path)
        {
            path = path.TrimStart('/');
            var response = await _client.GetAsync(path).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"path: {path} response: {(int)response.StatusCode} {response.ReasonPhrase}");
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return jsonResponse;
            }
        }

        internal async Task<string> PostJsonAsync(string path, string body)
        {
            path = path.TrimStart('/');

            var json = body;
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(path, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}");
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return jsonResponse;
            }
        }

        internal async Task<string> PutJsonAsync(string path, string body)
        {
            path = path.TrimStart('/');

            var json = body;
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(path, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}");
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return jsonResponse;
            }
        }

        internal T Get<T>(string path)
        {
            var task = GetAsync<T>(path);
            task.Wait();
            return task.Result;
        }

        internal string GetJson(string path)
        {
            var task = GetJsonAsync(path);
            task.Wait();
            return task.Result;
        }

        internal string PostJson(string path, string body)
        {
            var task = PostJsonAsync(path, body);
            task.Wait();
            return task.Result;
        }

        internal string PutJson(string path, string body)
        {
            var task = PutJsonAsync(path, body);
            task.Wait();
            return task.Result;
        }
        
        internal double GetDoubleFromJson(string endpoint)
        {
            var json = GetJson(endpoint);
            return double.Parse(json);
        }

        internal JObject GetJObject(string url)
        {
            return JObject.Parse(GetJson(url));
        }

        internal T GetJsonAsObject<T>(string url)
        {
            return GetJObject(url).ToObject<T>();
        }

        internal string PostWithObjectBody(string url, object body)
        {
            return PostJson(url, JsonConvert.SerializeObject(body));
        }

        internal JObject PostAndParseWithObjectBody(string url, object body)
        {
            return JObject.Parse(PostWithObjectBody(url, body));
        }

        internal T PostAndParseObjectBody<T>(string url, object body)
        {
            return SetJObjectType<T>(PostAndParseWithObjectBody(url, body));
        }

        private static T SetJObjectType<T>(JObject jObject)
        {
            return jObject == null ? default : jObject.ToObject<T>() ?? default;
        }
    }
}
