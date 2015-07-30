using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
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
            var reason = cycle.IsUpdatable(newScore, checkpoint, CheckPoint.Current());
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
            
            IRestResponse response;
            if (currentCycle.HasMissingCPs())
            {
                var missingCPs = currentCycle.MissingCPs();
                //http://localhost:31790/#/6/1
                var missingMessages = missingCPs.Select(cp => string.Format("Missing CP {2}. Goto http://{0}/#/{1}/{2} to update the score", Request.RequestUri.Host.ToLower(), currentCycle.Cycle.Id,cp.Cp)).ToList();
                response = _slackSender.Send(string.Join("\n", missingMessages));
            }
            else
            {
                response = _slackSender.Send(currentCycle.ToString());
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException((int) response.StatusCode, "Error Sending To Slack. " + response.ErrorMessage);
            }
        }

        /*
        [Route("{cp:int}/{elightenedScore:int}/{resistanceScore:int}")]
        [HttpGet]
        // GET api/values/5
        public string SetScore(int cp, int elightenedScore, int resistanceScore)
        {
            var checkPoint = CheckPoint.Current();
            var cpScore = new CpScore(cp, resistanceScore, elightenedScore);
            var setScore = new SetScoreCommand(checkPoint, cpScore);

            _cycleScore = _cycleScore.SetScore(setScore);
            _factory.UpdateScore(_cycleScore);
            if (!_cycleScore.CurrentMissingCps().Any())
            {
                SendMessageToSlack.Send(_cycleScore.ToString());
            }
            return _cycleScore.ToString();
        }
         * */
        /*
        [Route("summary")]
        [HttpGet]
        public string[] Summary()
        {
            return _currenntCycleScore.Summary.ToArray();
        }
        */
        /*
        /// <summary>
        ///     For slack slashcommands. Not wired in
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        private string Post(payload post)
        {
            //post.text should be [1-35] enlscore resscore 
            //or
            //post.text should be enlscore resscore 
            
            try
            {
                var command = SetScoreCommand.Parse(post.text);
                if (command == null)
                {
                    if (string.IsNullOrEmpty(post.text))
                    {
                        return _cycleScore.ToString();
                    }
                    /*
                    if (post.text.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var list = string.Join("\n", scoreEntity.CycleScore.List());
                        SendMessageToSlack.Send(list);
                        return list;
                    }
                     * * ///
                    if (post.text.Equals("summary", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var summary = _cycleScore.ToString();
                        SendMessageToSlack.Send(summary);
                        return summary;
                    }
                    return string.Format("Could not parse {0}", post.text);
                }
                if (_cycleScore.IsForCurrentCycle(command))
                {
                    _cycleScore = _cycleScore.SetScore(command);
                    _factory.UpdateScore(_cycleScore);
                }
                else
                {
                    //should we insert logic?
                    return "No scores availabe for current cycle.";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            

            return _cycleScore.ToString();
        }*/
    }
}