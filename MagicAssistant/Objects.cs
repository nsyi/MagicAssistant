using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using static MagicAssistant.greTypes;
using RestSharp;

namespace MagicAssistant
{
    [Serializable]
    public class DataObject
    {
        public int ID;
        public string Name;
        public MASettings Settings = new MASettings();
        public MAMatch Match = new MAMatch();
        public string SerializeObject()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
            return jsonString;
        }
        public void PostToAPI()
        {
            var client = new RestClient("https://api.inresponse.gg/beta/companion/logmatch");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorizationToken", "4565453");
            request.AddHeader("Content-Type", "application/json");
            //"{\r\n    \"token\": \"3965cade-b3ac-4dc7-a564-75d1181464f7\",\r\n    \"user\": 1,\r\n    \"matchlog\":{\"dummylog\":\"dummy log json\"}\r\n  }"
            string parameter_value = String.Concat("{\"token\":\"3965cade-b3ac-4dc7-a564-75d1181464f7\",\"user\": 1,\"matchlog\":", SerializeObject(), "}");
            request.AddParameter("application/json", parameter_value, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
    }
    public class MASettings
    {
        public int General;
        public MAView Player;
        public MAView Opponet;
    }
    public class MAView
    {
        public bool viewOn;
        public int locationX;
        public int locationY;
        public int sizeX;
        public int sizeY;
    }
    public class MAMatch
    {
        public MAMatchSummary MatchSummary = new MAMatchSummary();
        public List<MAGame> MatchGames = new List<MAGame>();
        public MAMatchSnapShot MatchSnapShot = new MAMatchSnapShot();
    }
    public class MAMatchSummary
    {
        public string Player;
        public string Opponet;
        public string Format;
        public string Score;
        public string StartDateTime;
        public string EndDateTime;
    }
    public class MAGame
    {
        public string gameName;
        public MAGameSummary GameSummary = new MAGameSummary();
        public List<MATurn> GameTurns = new List<MATurn>();
    }
    public class MATurn
    {
        public string turnName;
        public int gainedLife;
        public int damageDealed; // including creatures
        public int usedMana;
        public int availableMana;
        
        // This should be for each player
        public List<GameObjectClass> BattleField = new List<GameObjectClass>(); // Difference
        public List<GameObjectClass> GraveYard = new List<GameObjectClass>();
        public List<GameObjectClass> Exile = new List<GameObjectClass>();
        public List<GameObjectClass> Hand = new List<GameObjectClass>();
        public List<GameObjectClass> Attacked = new List<GameObjectClass>();
    }
    public class MAGameSummary
    {
        public bool onPlay;
        public bool won;
        public List<GameObjectClass> StratingHand = new List<GameObjectClass>();
        public List<GameObjectClass> StartingDeck = new List<GameObjectClass>();
        public List<GameObjectClass> StartingSideBoard = new List<GameObjectClass>();
    }
    public class MAMatchSnapShot
    {
        public MAPlayer Player = new MAPlayer();
        public MAPlayer Opponent = new MAPlayer();
    }
    public class MAPlayer
    {
        public string name;
        public int lifeTotal;
        public int startingLifeTotal;
        public List<GameObjectClass> Deck = new List<GameObjectClass>();
        public List<GameObjectClass> SidBoard = new List<GameObjectClass>();
        public List<GameObjectClass> BattleField = new List<GameObjectClass>();
        public List<GameObjectClass> GraveYard = new List<GameObjectClass>();
        public List<GameObjectClass> Exile = new List<GameObjectClass>();
        public List<GameObjectClass> Hand = new List<GameObjectClass>();
        public List<GameObjectClass> Playable = new List<GameObjectClass>();
    }
}