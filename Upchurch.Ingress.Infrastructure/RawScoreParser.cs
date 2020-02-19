using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Upchurch.Ingress.Domain.Intel;

namespace Upchurch.Ingress.Infrastructure
{
    public static class RawScoreParser
    {
        public static Result Parse(string json)
        {
            var result= JsonConvert.DeserializeObject<Score>(json);
            return result.result;
        }

        public static Result Parse(JObject json)
        {
            
            var result = json.SelectToken("result").ToObject<Result>();
            return result;
        }

        public static string Serialize(string[][] scoreHistory)
        {
            var rawScore = new Score {result = new Result {scoreHistory = scoreHistory } };
            var json = JsonConvert.SerializeObject(rawScore);
            return json;
        }
    }

}
