using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DaemonSide
{
    class Http
    {
        HttpClient client = new HttpClient();
        public async Task<string> GetAsync(string api)
        {
            string result = await client.GetStringAsync(api);
            return result;
        }
        public async Task<string> GetAsyncID(string api, int id)
        {
            if (client.BaseAddress == null) client.BaseAddress = new Uri("https://localhost:44358");
            string result = await client.GetStringAsync(api + id);
            return result;
        }
        public async Task<string> GetAsyncIDMulti(string api, string ids)
        {
            if (client.BaseAddress == null) client.BaseAddress = new Uri("https://localhost:44358");
            string result = await client.GetStringAsync(api + ids);
            return result;
        }
        public async Task<string> PostAsync(string api, Object obj)
        {
            if (client.BaseAddress == null) client.BaseAddress = new Uri("https://localhost:44358");
            HttpContent content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            HttpResponseMessage msg = await client.PostAsync(api, content);
            string result = await msg.Content.ReadAsStringAsync();
            return result;
        }
        public async Task<string> PutAsync(string api, int id, Object obj)
        {
            if (client.BaseAddress == null) client.BaseAddress = new Uri("https://localhost:44358");
            HttpContent content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            HttpResponseMessage msg = await client.PutAsync(api + id, content);
            string result = await msg.Content.ReadAsStringAsync();
            return result;
        }
    }
}