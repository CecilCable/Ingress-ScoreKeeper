// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Configuration;
using Upchurch.Ingress.Domain;
using Upchurch.Ingress.Infrastructure;

namespace Upchurch.Ingress.DependencyResolution {
    using StructureMap;
	
    public static class IoC {
        public static IContainer Initialize() {
            return new Container(c =>
            {
                var connectionString = ConfigurationManager.ConnectionStrings["AzureTables"];
                
                if (connectionString != null)
                {
                    c.ForSingletonOf<IScraperService>().Use<ScraperService>().Ctor<string>("connectionString").Is(connectionString.ConnectionString);
                    c.ForSingletonOf<ICycleScoreUpdater>().Use<AzureScoreFactory>().Ctor<string>("connectionString").Is(connectionString.ConnectionString);
                }
                else
                {
                    var pasteConnectionString =
                        "DefaultEndpointsProtocol=https;AccountName=PASTE";
                    c.ForSingletonOf<ICycleScoreUpdater>().Use<AzureScoreFactory>().Ctor<string>("connectionString").Is(pasteConnectionString);
                    c.ForSingletonOf<IScraperService>().Use<ScraperService>().Ctor<string>("connectionString").Is(pasteConnectionString);
                    c.For<ICycleScoreUpdater>().Use<InMemoryScoreFactory>().Singleton();
                }
                
                var slackApiUrl = ConfigurationManager.AppSettings["SlackApiUrl"];

                c.ForSingletonOf<ISlackSender>().Use<SendMessageToSlack>().Ctor<string>("slackApiUrl").Is(slackApiUrl);
                
            });
        }
    }
}