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







        // Handle zones 
        //switch (zone["type"])
        //{
        //    case "ZoneType_Stack":
        //        if (zone.ContainsKey("objectInstanceIds"))
        //        {
        //            string list = "";
        //            foreach (var objectInstanceIds in zone["objectInstanceIds"])
        //            {
        //                list = list + ", " + objectInstanceIds;
        //            }
        //            //AppendEventText("ZoneType_Stack" + list);
        //        }
        //        break;

        //    case "ZoneType_Battlefield":
        //        if (zone.ContainsKey("objectInstanceIds"))
        //        {
        //            Battlefield.Clear();
        //            string list = "";
        //            foreach (var objectInstanceIds in zone["objectInstanceIds"])
        //            {
        //                list = list + ", " + objectInstanceIds;
        //                if (!Battlefield.ContainsKey(objectInstanceIds))
        //                    Battlefield.Add(objectInstanceIds, objectInstanceIds);
        //            }
        //            AppendEventText("ZoneType_Battlefield" + list);
        //        }
        //        break;
        //    case "ZoneType_Limbo":
        //        if (zone.ContainsKey("objectInstanceIds"))
        //        {
        //            string list = "";
        //            foreach (var objectInstanceIds in zone["objectInstanceIds"])
        //            {
        //                list = list + ", " + objectInstanceIds;
        //            }
        //            //AppendEventText("ZoneType_Limbo" + list);
        //        }
        //        break;
        //    case "ZoneType_Graveyard":
        //        if (zone.ContainsKey("objectInstanceIds"))
        //        {
        //            string list = "";
        //            foreach (var objectInstanceIds in zone["objectInstanceIds"])
        //            {
        //                list = list + ", " + objectInstanceIds;
        //            }
        //            //AppendEventText("ZoneType_Graveyard" + list);
        //        }
        //        break;
        //}




        // Player hand
        //string actions = "";
        //foreach (var action in actions1)
        //{
        //    if (action.Value.actionType != EnumActionType.ActionType_Activate)
        //    {
        //        string card_name = "";
        //        if (GameObjects.ContainsKey(action.Value.instanceId))
        //        {
        //            GameObjectClass go = GameObjects[action.Value.instanceId];
        //            if (dataBaseCards.ContainsKey(go.name))
        //                card_name = dataBaseCards[go.name]["name"];
        //        }
        //        actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
        //    }
        //}

        //if (actions != "")
        //    WriteLogText(xaml_player_hand, actions);



        //// Player cards
        //actions = "";
        //foreach (var action in actions1)
        //{
        //    if (action.Value.actionType == EnumActionType.ActionType_Activate)
        //    {

        //        string card_name = "";
        //        if (GameObjects.ContainsKey(action.Value.instanceId))
        //        {
        //            GameObjectClass go = GameObjects[action.Value.instanceId];
        //            if (dataBaseCards.ContainsKey(go.name))
        //                card_name = dataBaseCards[go.name]["name"];
        //        }
        //        actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
        //        if (Battlefield.ContainsKey(action.Value.instanceId))
        //            Battlefield.Remove(action.Value.instanceId);
        //    }
        //}
        //// Player  cards from battlefield
        //foreach (var card in Battlefield)
        //{
        //    string card_name = "";
        //    if (GameObjects.ContainsKey(card.Key))
        //    {
        //        GameObjectClass go = GameObjects[card.Key];
        //        if (dataBaseCards.ContainsKey(go.name))
        //            card_name = dataBaseCards[go.name]["name"];
        //        if (GameObjects[card.Key].controllerSeatId == 1)
        //            actions = string.Concat(actions, card_name, ", ", card.Key, "\r\n");
        //    }
        //}
        //if (actions != "")
        //    WriteLogText(xaml_player, actions);





        //// Opponent cards
        //actions = "";
        //foreach (var action in actions2)
        //{
        //    string card_name = "";
        //    if (GameObjects.ContainsKey(action.Value.instanceId))
        //    {
        //        GameObjectClass go = GameObjects[action.Value.instanceId];
        //        if (dataBaseCards.ContainsKey(go.name))
        //            card_name = dataBaseCards[go.name]["name"];
        //    }
        //    actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
        //    if (Battlefield.ContainsKey(action.Value.instanceId))
        //        Battlefield.Remove(action.Value.instanceId);
        //}

        //// Opponent cards from battlefield
        //foreach (var card in Battlefield)
        //{
        //    string card_name = "";
        //    if (GameObjects.ContainsKey(card.Key))
        //    {
        //        GameObjectClass go = GameObjects[card.Key];

        //        if (GameObjects[card.Key].controllerSeatId == 2)
        //        {
        //            if (dataBaseCards.ContainsKey(go.name))
        //            {
        //                card_name = dataBaseCards[go.name]["name"];
        //                actions = string.Concat(actions, card.Key, ", (", card_name, ")", "\r\n");
        //            }
        //            else
        //                actions = string.Concat(actions, card.Key, "\r\n");
        //        }
        //    }
        //}

        //if (actions != "")
        //    WriteLogText(xaml_opponent, actions);


    }
}
