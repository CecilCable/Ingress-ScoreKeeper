using RestSharp;

namespace Upchurch.Ingress.Infrastructure
{
    public interface ISlackSender
    {
        void Send(string text);
    }
}