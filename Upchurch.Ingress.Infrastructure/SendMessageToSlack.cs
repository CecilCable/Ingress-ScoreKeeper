using System.Net;
using System.Web;
using RestSharp;

namespace Upchurch.Ingress.Infrastructure
{
    public class SendMessageToSlack : ISlackSender
    {
        private readonly string _slackApiUrl;

        public SendMessageToSlack(string slackApiUrl)
        {
            _slackApiUrl = slackApiUrl;
        }

        public void Send(string text)
        {
            var client = new RestClient(_slackApiUrl);
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(new payload {text = text});
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException((int) response.StatusCode, "Error Sending To Slack. " + response.ErrorMessage);
            }
        }

        private class payload
        {
            public string text { get; set; }
        }
    }
}