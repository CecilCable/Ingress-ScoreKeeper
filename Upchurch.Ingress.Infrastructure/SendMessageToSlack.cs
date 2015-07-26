using RestSharp;

namespace Upchurch.Ingress.Infrastructure
{
    public class SendMessageToSlack:ISlackSender
    {
        private readonly string _slackApiUrl;

        public SendMessageToSlack(string slackApiUrl)
        {
            _slackApiUrl = slackApiUrl;
        }

        public IRestResponse Send(string text)
        {
            var client = new RestClient(_slackApiUrl);
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(new payload {text = text});
            return client.Execute(request);
        }

        private class payload
        {
            public string text { get; set; }
        }
    }
}