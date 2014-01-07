using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ChunkedStreamServer
{
    public class PushController : ApiController
    {
        int run = 0;


        [HttpGet]
        [Route("PollData")]
        public HttpResponseMessage PollData(string sessionId)
        {
            log("Polling requst recevied from " + sessionId, 4);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            //response.Headers.TransferEncodingChunked = true;
            response.Content = new PushStreamContent((streamContent, content, context) => writePushData(streamContent, content, context), "application/json");
            return response;
        }

        [HttpGet]
        [Route("PollData")]
        public HttpResponseMessage PollData()
        {
            log("Polling requst recevied", 4);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            //response.Headers.TransferEncodingChunked = true;
            response.Content = new PushStreamContent((streamContent, content, context) => writePushData(streamContent, content, context), "application/json");
            return response;
        }

        private async void writePushData(Stream streamContent, HttpContent content, TransportContext context)
        {
            StreamWriter sw = new StreamWriter(streamContent) { AutoFlush = true };
            while (true)
            {
                run++;
                await Task.Delay(1000);
                var str = "{ \"data\" : \"" + run + "\"}";
                var buffer = UTF8Encoding.UTF8.GetBytes(str);
                log("sending " + str, 4);
                try
                {
                    await sw.WriteLineAsync(str);
                }
                catch (IOException i)
                {
                    log("Writing to stream failed: " + i.Message, 2);
                    break;
                }
                catch (ObjectDisposedException o)
                {
                    log("Unable to write to stream, object disposed: " + o.Message, 2);
                    break;
                }
            }
        }

        private void log(string message, int severity)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
        }

    }
}
