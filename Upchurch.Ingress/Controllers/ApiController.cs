using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using RestSharp;
using Upchurch.Ingress.Domain;
using Upchurch.Ingress.Infrastructure;

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

        [Route("{cycleId:int}")]
        [HttpPost]
        public string SetScore(int cycleId, string intelJson, string timeStamp)
        {
            var parser = new RawScoreParser();
            var score = parser.Parse(intelJson);
            var cycle = GetScoreForCycle(cycleId);
            var longtimeStamp = long.Parse(timeStamp);
            var reason = cycle.IsUpdatable(score, longtimeStamp);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }
            if (!cycle.SetScore(score.Generate(), longtimeStamp, _scoreUpdater))
            {
                return "SetScore Failed. Probably someone else updated it already.";
            }

            if (!cycle.HasMissingCPs())
            {
                PostToSlack(cycle);
                return "post to slack";
            }

            return "OK";
            ////Do you need to explicitly overwrite
            ////is it the same score?
            ////was it okay?
            //var cycle = GetScoreForCycle(cycleId);
            //var reason = cycle.IsUpdatable(newScore, checkpoint);
            //if (!string.IsNullOrEmpty(reason))
            //{
            //    return reason;
            //}
            //if (!cycle.SetScore(checkpoint, newScore, _scoreUpdater))
            //{
            //    return "SetScore Failed. Probably someone else updated it already.";
            //}

            //if (!cycle.HasMissingCPs())
            //{
            //    PostToSlack(cycle);
            //    return "post to slack";
            //}

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

            return Redirect(string.Format("http://{0}/?{1}", Request.RequestUri.Host.ToLower(), response));

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

            PostToSlack(currentCycle);
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
                var missingMessages = string.Format("Missing CP {2}. Goto http://{0}/#/{1}/{2} to update the score", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id, missingCPs[0].Cp);
                _slackSender.Send(missingMessages);
                return;
            }

            var cpString = ConvertToDashAndCommaString(missingCPs);

            var manymissing = string.Format("Missing CPs {2}. Goto http://{0}/#/{1} to update the score", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id, cpString);
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