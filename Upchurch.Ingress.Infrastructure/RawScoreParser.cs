using Newtonsoft.Json;
using Upchurch.Ingress.Domain.Intel;

namespace Upchurch.Ingress.Infrastructure
{
    public class RawScoreParser
    {
        public Result Parse(string json)
        {
            var result= JsonConvert.DeserializeObject<Score>(json);
            return result.result;
        }

        public string Serialize(string[][] scoreHistory)
        {
            var rawScore = new Score {result = new Result {scoreHistory = scoreHistory } };
            var json = JsonConvert.SerializeObject(rawScore);
            return json;
        }
    }

}
