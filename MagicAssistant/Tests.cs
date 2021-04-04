using System;
using RestSharp;

namespace MagicAssistant
{
    public static class Tests
    {
        public static string SerializationTest()
        {
            // Serial Test
            DataObject data = new DataObject();
            data.ID = 1;
            data.Name = ".Net";
            data.Settings.General = 0;
            data.Match.MatchSummary.Player = "player name";

            MAGame game1 = new MAGame();
            game1.gameName = "game 1";
            game1.GameSummary.onPlay = false;
            data.Match.MatchGames.Add(game1);

            MAGame game2 = new MAGame();
            game2.gameName = "game 2";
            game2.GameSummary.onPlay = true;
            data.Match.MatchGames.Add(game2);

            data.Match.MatchSnapShot.Player.name = "player name";
            data.Match.MatchSnapShot.Opponent.name = "opponet name";

            string json = data.SerializeObject();
            return json;
            //APITest(json);
        }
        public static void APITest(string log)
        {
            var client = new RestClient("https://api.inresponse.gg/beta/companion/logmatch");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorizationToken", "4565453");
            request.AddHeader("Content-Type", "application/json");
            //"{\r\n    \"token\": \"3965cade-b3ac-4dc7-a564-75d1181464f7\",\r\n    \"user\": 1,\r\n    \"matchlog\":{\"dummylog\":\"dummy log json\"}\r\n  }"
            string parameter_value = String.Concat("{\"token\":\"3965cade-b3ac-4dc7-a564-75d1181464f7\",\"user\": 1,\"matchlog\":", log, "}");
            request.AddParameter("application/json", parameter_value, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

        }
    }
}
