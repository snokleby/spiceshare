using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SpiceShare.Helpers
{
    public class CognetiveServices
    {
       public static string UnwantedContent = "Opps, we have detected that this image might contain unvanted content. Try another image.";
        public static string RateLimit = "We're glad you like this service, but we have to limit how often you can upload a picture. Please wait a minute or two before trying again.";


        public static async Task<bool> IsSafeContent(string imgUrl)
        {
            try
            {
                var baseUrl = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Adult&visualFeatures=Adult";
                var response = await DoRequest(baseUrl, imgUrl);

                var res = await response.Content.ReadAsAsync<CognetiveAdultResult>();
                return !res.Adult.IsAdultContent && !res.Adult.IsRacyContent;
            } catch(Exception) { }
            return true;
        }

        internal static async Task<string> GetDescription(string orignalUrl)
        {
            try
            {
                var baseUrl = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Description&visualFeatures=Description";
                var response = await DoRequest(baseUrl, orignalUrl);

                var res = await response.Content.ReadAsAsync<CognetiveDescriptionResult>();
                return res.Description.Captions.FirstOrDefault()?.Text;
            }
            catch (Exception) {
                return "";
            }
        }

        private async static Task<HttpResponseMessage> DoRequest(string baseUrl, string imgUrl)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseUrl),
                Method = HttpMethod.Post,
                Content = new StringContent($"{{ \"url\" : \"{imgUrl}\" }}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("Ocp-Apim-Subscription-Key"));
            return await client.SendAsync(request);
        }
    }

    public class CognetiveAdultResult
    {
        public Adult Adult { get; set; }
    }

    public class CognetiveDescriptionResult
    {
        public Description Description { get; set; }
    }

    public class Description
    {
        public List<Caption> Captions { get; set; }
    }

    public class Caption
    {
        public string Text { get; set; }
    }


    public class Adult
    {
       
        public bool IsAdultContent { get; set; }

        public bool IsRacyContent { get; set; }

        public double AdultScore { get; set; }
       
        public double RacyScore { get; set; }
    }
}
