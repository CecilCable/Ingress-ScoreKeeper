using RestSharp;

namespace Upchurch.Ingress.Infrastructure
{
    public interface ISlackSender
    {
        IRestResponse Send(string text);
    }
}