using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using Upchurch.Ingress.Domain;
using Upchurch.Ingress.Infrastructure;
using Upchurch.Ingress.Models;

namespace Upchurch.Ingress.Controllers
{
    [RoutePrefix("API")]
    public class ValuesController : ApiController
    {
        private readonly ICycleScoreUpdater _scoreUpdater;
        private readonly ISlackSender _slackSender;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scoreUpdater">should be a singleton</param>
        /// <param name="slackSender"></param>
        public ValuesController(ICycleScoreUpdater scoreUpdater, ISlackSender slackSender)
        {
            _scoreUpdater = scoreUpdater;
            _slackSender = slackSender;
        }

        private CycleScore GetScoreForCycle(int cycleId)
        {
            return _scoreUpdater.GetScoreForCycle(new CycleIdentifier(cycleId));
        }

        [Route("")]
        [HttpGet]
        public int CurrentCycle()
        {
            return CheckPoint.Current().Cycle.Id;
        }

        [Route("{cycleId:int}/OverallScore")]
        [HttpGet]
        public OverallScore OverallScore(int cycleId)
        {
            return GetScoreForCycle(cycleId).OverallScore();
        }
        /*
        [Route("{cycleId:int}/MissingCheckpoints")]
        [HttpGet]
        public MissingScore MissingCheckpoints(int cycleId)
        {
            return GetScoreForCycle(cycleId).CurrentMissingCps();
        }*/

        [Route("{cycleId:int}")]
        [HttpGet]
        public IEnumerable<CpStatus> Scores(int cycleId)
        {
            return GetScoreForCycle(cycleId).AllCPs();
        }

        [Route("{cycleId:int}/{checkpoint:int}")]
        [HttpGet]
        public UpdateScore GetScore(int cycleId, int checkpoint)
        {
            var cycle = GetScoreForCycle(cycleId);
            return cycle.ScoreForCheckpoint(checkpoint);
        }

        [Route("{cycleId:int}/{checkpoint:int}")]
        [HttpPost]
        public string SetScore(int cycleId, int checkpoint, UpdateScore newScore)
        {

            //Do you need to explicitly overwrite
            //is it the same score?
            //was it okay?
            var cycle = GetScoreForCycle(cycleId);
            var reason = cycle.IsUpdatable(newScore, checkpoint);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }
            if (!cycle.SetScore(checkpoint, newScore, _scoreUpdater))
            {
                return "SetScore Failed. Probably someone else updated it already.";
            }

            if (!cycle.HasMissingCPs())
            {
                PostToSlack(cycle);
                return "post to slack";
            }

            return "OK";
        }
        /// <summary>
        /// For plugins because they're stupid and we're defering as much logic here as possible
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [Route("fiddler")]
        [HttpPost]
        public string Fiddler(JObject json)
        {
            var score = RawScoreParser.Parse(json);
            var cycle = GetScoreForCycle(score.CycleId());
            List<KeyValuePair<int, CpScore>> updatedValues;
            var reason = cycle.IsUpdatable(score, out updatedValues);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }
            if (!cycle.SetScore(updatedValues, _scoreUpdater))
            {
                return "SetScore Failed. Someone else updated it OR all CPs are already updated";
            }

            if (!cycle.HasMissingCPs())
            {
                PostToSlack(cycle);
                return "post to slack";
            }

            return "OK";

        }

        /// <remarks>
        /// Why no add timestamp to the URL?
        /// For pasting the fiddler response
        /// </remarks>
        [Route("{cycleId:int}")]
        [HttpPost]
        public string SetScore(int cycleId, JsonIntel intelJson)
        {
            var score = RawScoreParser.Parse(intelJson.Json);
            var cycle = GetScoreForCycle(cycleId);
            var longtimeStamp = long.Parse(intelJson.TimeStamp);
            List<KeyValuePair<int, CpScore>> updatedValues;

            var reason = cycle.IsUpdatable(score, out updatedValues, longtimeStamp);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }
            if (!cycle.SetScore(updatedValues, _scoreUpdater, longtimeStamp))
            {
                return "SetScore Failed. Someone else updated it OR all CPs are already updated";
            }

            if (!cycle.HasMissingCPs())
            {
                PostToSlack(cycle);
                return "post to slack";
            }

            return "OK";

        }

        [Route("{cycleId:int}/{isSnooze:bool}")]
        [HttpGet] //nothing actually posted?
        public RedirectResult SetSnooze(int cycleId, bool isSnooze)
        {

            //Do you need to explicitly overwrite
            //is it the same score?
            //was it okay?
            var cycle = GetScoreForCycle(cycleId);

            var response = cycle.SetSnooze(isSnooze, _scoreUpdater);

            return Redirect($"https://{Request.RequestUri.Host.ToLower()}/?{response}");

        }

        [Route("{cycleId:int}/Summary")]
        [HttpGet]
        public IEnumerable<string> Summary(int cycleId)
        {
            return GetScoreForCycle(cycleId).Summary(false);
        }

        [Route("PostToSlack")]
        [HttpGet]
        public void PostToSlack()
        {
            var currentCycle = _scoreUpdater.GetScoreForCycle(CheckPoint.Current().Cycle);
            if (currentCycle.MissingCPs().Count() != 0)
            {
                PostToSlack(currentCycle);//only post of there are missing CPs
            }
        }

        private void PostToSlack(CycleScore currentCycle)
        {
            var cp = CheckPoint.Current();
            if (currentCycle.IsSnoozed)
            {

                if (cp.IsFirstMessageOfDay())
                {
                    var snoozeMessage = string.Format("Shhh. Cycle bot is sleeping.\nGoto http://{0}/api/{1}/false to un-snooze me.\nIf you really want to know the score summary goto http://{0}/#/{1}\nI'll wake up again at {2}.", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id, cp.NextUnsnoozeTime());
                    _slackSender.Send(snoozeMessage);
                }
                return;
            }
            if (currentCycle.Cycle.Id != cp.Cycle.Id)
            {
                return;
            }
            var missingCPs = currentCycle.MissingCPs().ToArray();

            if (missingCPs.Length == 0)
            {
                _slackSender.Send(currentCycle.ToString());
                return;
            }
            if (missingCPs.Length == 1)
            {

                _slackSender.Send(string.Format("Missing CP {2}. Goto https://{0}/#/{1}/{2} to update the score", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id, missingCPs[0].Cp));
                return;
            }

            var cpString = ConvertToDashAndCommaString(missingCPs);

            var manymissing = string.Format("Missing CPs {2}. Goto https://{0}/#/{1} to update the score", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id, cpString);
            _slackSender.Send(manymissing);



        }

        private static string ConvertToDashAndCommaString(IReadOnlyList<CpStatus> missingCPs)
        {
            var cpStrings = new string[missingCPs.Count];

            for (var i = 0; i < missingCPs.Count; i++)
            {
                if (i != 0 && i + 1 != missingCPs.Count)
                {
                    if (missingCPs[i - 1].Cp + 1 == missingCPs[i].Cp && missingCPs[i + 1].Cp - 1 == missingCPs[i].Cp)
                    {
                        cpStrings[i] = "-";
                        continue;
                    }
                }
                cpStrings[i] = missingCPs[i].Cp.ToString();
            }
            var cpString = cpStrings[0];

            for (var i = 1; i < cpStrings.Length; i++)
            {
                if (cpStrings[i] == "-" && cpStrings[i - 1] == "-")
                {
                    continue;
                }
                if (cpStrings[i] != "-" && cpStrings[i - 1] != "-")
                {
                    cpString += ",";
                }
                cpString += cpStrings[i];
            }
            return cpString;
        }
    }
}