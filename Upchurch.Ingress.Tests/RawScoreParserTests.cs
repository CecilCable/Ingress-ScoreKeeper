using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upchurch.Ingress.Infrastructure;

namespace Upchurch.Ingress.Tests
{
    
    [TestClass]
    public class RawScoreParserTests
    {
        [TestMethod]
        public void SerializeThis()
        {
            

            var history = new string[1][];
            history[0] = new [] {"21", "22", "23"};



           var json = RawScoreParser.Serialize(history);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void ParseThis2()
        {
            
            var output = RawScoreParser.Parse(@"{""result"":{""scoreHistory"":[[""21"",""22"",""23""]]}}");
            Assert.AreEqual(1, output.scoreHistory.Length);
        }

        [TestMethod]
        public void ParseThis()
        {
           var output = RawScoreParser.Parse(@"{
  ""result"":{
    ""gameScore"":[
      ""143703"",
      ""135249""
    ],
    ""topAgents"":[
      {""nick"":""R3D3vi1Ford"",
        ""team"":""ENLIGHTENED""
      },
      {""nick"":""Becktard55"",
        ""team"":""RESISTANCE""
      },
      {""nick"":""xxxPumpkinxxx"",
        ""team"":""ENLIGHTENED""
      }
    ],
    ""scoreHistory"":[
      [""21"",
        ""125649"",
        ""221328""],
      [""20"",
        ""122540"",
        ""139784""],
      [""19"",
        ""124469"",
        ""114180""],
      [""18"",
        ""112735"",
        ""100892""],
      [""17"",
        ""116668"",
        ""108672""],
      [""16"",
        ""137131"",
        ""104451""],
      [""15"",
        ""138176"",
        ""150151""],
      [""14"",
        ""186583"",
        ""148487""],
      [""13"",
        ""118569"",
        ""141223""],
      [""12"",
        ""126886"",
        ""156911""],
      [""11"",
        ""370069"",
        ""154013""],
      [""10"",
        ""148342"",
        ""139598""],
      [""9"",
        ""201650"",
        ""137359""],
      [""8"",
        ""135908"",
        ""136235""],
      [""7"",
        ""134333"",
        ""124449""],
      [""6"",
        ""134877"",
        ""137290""],
      [""5"",
        ""103616"",
        ""127167""],
      [""4"",
        ""95278"",
        ""141108""],
      [""3"",
        ""134625"",
        ""130764""],
      [""2"",
        ""100732"",
        ""107393""],
      [""1"",
        ""148930"",
        ""118780""
      ]
    ],
    ""regionVertices"":[
      [39871128,
        -84940131
      ],
      [38543908,
        -84940131
      ],
      [38477965,
        -83592264
      ],
      [39804551,
        -83592264
      ]
    ],
    ""timeToEndOfBaseCycleMs"":16976310,
    ""regionName"":""AM02-KILO-00""
  }
}");
            Assert.AreEqual(21, output.scoreHistory.Length);
            var results = output.Generate();

            var dict = results.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            Assert.AreEqual(21, dict.Count);
        }
    }
}
