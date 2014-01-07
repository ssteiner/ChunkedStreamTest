using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChunkedStreamClient
{
    public class ServerConnector
    {

        HttpClient httpClient;
        string baseUrl;
        private JsonSerializerSettings serializerSettings;
        private JsonMediaTypeFormatter jsonFormatter;

        //stuff for chunked stream
        private CancellationTokenSource cancelSource;
        private StreamReader chunkedStreamReader;

        public string UserId { get; private set; }

        public ServerConnector(string baseUrl)
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            this.baseUrl = baseUrl;

            serializerSettings = new JsonSerializerSettings();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            serializerSettings.Converters.Add(new StringEnumConverter());
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // use camel case so we can use proper .net notation

            jsonFormatter = new JsonMediaTypeFormatter { SerializerSettings = serializerSettings };

            cancelSource = new CancellationTokenSource();

        }


        public async void StartPolling()
        {
            HttpResponseMessage response = null;
            try
            {
                HttpClientHandler pollHandler = new HttpClientHandler { UseCookies = true, AllowAutoRedirect = false };
                HttpClient pollClient = new HttpClient(pollHandler);
                pollClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                Uri reqUri = new Uri(baseUrl + "PollData");
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, reqUri);
                //req.Headers.ConnectionClose = true;
                response = await pollClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {

                    System.IO.Stream chunkedStream2 = await response.Content.ReadAsStreamAsync();
                    startReadingFromChunkedStream(chunkedStream2);

                    //System.IO.Stream chunkedStream = await pollClient.GetStreamAsync(reqUri); // generates a second GET request
                    //startReadingFromChunkedStream(chunkedStream);
                }
            }
            catch (Exception e)
            {
                log("Exception in StartPolling: " + e.Message, 2);
            }
        }

        private void startReadingFromChunkedStream(System.IO.Stream chunkedStream)
        {
            Task readerTask = Task.Factory.StartNew(() => readFromChunkedStream(chunkedStream), cancelSource.Token);
            readerTask.ContinueWith(t =>
            {
                foreach (var e in t.Exception.Flatten().InnerExceptions)
                    log("Exception reading from chunked stream: " + e.Message, 2);
            }, TaskContinuationOptions.OnlyOnFaulted);
            readerTask.ContinueWith(t =>
            {
                log("Chunked stream reader task cancelled", 3);
            }, TaskContinuationOptions.OnlyOnCanceled);
        }

        private void readFromChunkedStream(System.IO.Stream chunkedStream)
        {
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(chunkedStream))
                {
                    chunkedStreamReader = sr;
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (cancelSource.IsCancellationRequested)
                        {
                            log("stop polling requested, aborting polling task", 4);
                            break;
                        }
                        log("Data received: " + line, 4);
                    }
                }
            }
            catch (Exception e)
            {
                log("stream reader faulted: " + e.Message, 2);
            }
            finally
            {
                if (chunkedStream != null)
                {
                    try
                    {
                        chunkedStream.Close();
                    }
                    catch (Exception) { }
                }
            }
        }


        private void log(string message, int severity)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
        }
    }
}
