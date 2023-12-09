using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Newtonsoft.Json.JsonConvert;
using static System.Text.Encoding;
using static System.TimeSpan;
using static UnityEngine.Networking.UnityWebRequestTexture;

namespace CybergrindMusicExplorer.Util
{
    public static class HttpUtils
    {
        private const int RetryAttempts = 3;
        private static readonly HttpClient Client = new HttpClient();

        static HttpUtils()
        {
            Client.Timeout = FromSeconds(5);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd("com.google.android.youtube/17.36.4 (Linux; U; Android 12; GB) gzip");
        }
        
        public static IEnumerator GetRequest<TRsp>(string url, Action<TRsp> callback)
        {
            return Request(ReadResponse<TRsp>(() => Client.GetAsync(url)), callback);
        }

        public static IEnumerator PostRequest<TReq, TRsp>(string url, TReq request, Action<TRsp> callback)
        {
            return Request(ReadResponse<TRsp>(
                () => Client.PostAsync(url, new StringContent(SerializeObject(request), UTF8))), callback);
        }

        private static IEnumerator Request<TRsp>(Task<TRsp> task, Action<TRsp> callback)
        {
            yield return new WaitUntil(() => task.IsCompleted || task.IsCanceled || task.IsFaulted);

            if (!task.IsCompleted || task.Exception != null)
            {
                if (task.Exception != null)
                    Debug.LogWarning($"HTTP error has occurred: {task.Exception.Message}");
                yield break;
            }

            try
            {
                callback.Invoke(task.Result);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"HTTP error has occurred: {e.Message}");
            }
        }

        private static async Task<TRsp> ReadResponse<TRsp>(Func<Task<HttpResponseMessage>> task)
        {
            TRsp response = default;
            for (var i = 0; i < RetryAttempts; i++)
            {
                string rawResponse = null;
                try
                {
                    var result = await task.Invoke();
                    rawResponse = await result.Content.ReadAsStringAsync();
                    if (rawResponse == null)
                        continue;
                } catch (Exception)
                {
                    continue;
                }

                response = DeserializeObject<TRsp>(rawResponse);
                    if (response == null)
                        continue;
                    
                    break;
            }

            return response;
        }

        public static IEnumerator DownloadFile(WebClient webClient, string url, string dst, Action<bool> success)
        {
            var downloading = true;
            using (webClient)
            {
                try
                {
                    webClient.Proxy = null;
                    webClient.DownloadFileCompleted += (sender, e) =>
                    {
                        if (e.Error != null || e.Cancelled)
                            success.Invoke(false);
                        else
                            success.Invoke(true);
                        downloading = false;
                    };

                    webClient.DownloadFileAsync(new Uri(url), dst);
                }
                catch (Exception)
                {
                    success.Invoke(false);
                    downloading = false;
                    yield break;
                }

                yield return new WaitUntil(() => !downloading);
            }
        }

        public static IEnumerator LoadTexture(string url, Action<Texture> callback) 
        {
            using var request = GetTexture(url);
            request.SendWebRequest();

            yield return new WaitUntil(() => request.isDone || request.error != null);
            if (request.error != null)
            {
                Debug.LogError($"An error has occured while downloading image {url}: {request.error}");
                yield break;
            }

            callback.Invoke(DownloadHandlerTexture.GetContent(request));
        }
    }
}