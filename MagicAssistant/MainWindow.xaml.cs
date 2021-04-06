using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Windows.Documents;
using static MagicAssistant.greTypes;
using System.Linq;

namespace MagicAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int message_count = 0;
        int error_count = 0;

        readonly string user_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\Wizards Of The Coast\\MTGA\\";
        readonly string data_base_file = AppDomain.CurrentDomain.BaseDirectory + "\\database.json";
        readonly string log_file = "player.log";
        readonly string test_log = "C:\\Users\\nsyig\\Documents\\My documents\\UW\\J017 - MTGA\\Player\\Player.log";

        JavaScriptSerializer jss = new JavaScriptSerializer();
        readonly dynamic dataBaseJson; // Database of cards
        readonly Dictionary<int, dynamic> dataBaseCards;

        public DataObject MainData = new DataObject(); // Main data file 

        EnumGameStage gameStage = EnumGameStage.GameStage_None;

        Dictionary<int, GameObjectClass> GameObjects = new Dictionary<int, GameObjectClass>();
        Dictionary<int, ZoneClass> Zones = new Dictionary<int, ZoneClass>();

        Dictionary<int, int> Battlefield = new Dictionary<int, int>();


        /// <summary>
        /// Main Window Entry Code
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            string log_path = user_path + log_file;
            CheckFile(log_path); // Check if log exists

            dataBaseJson = ReadDataBase(data_base_file, out dataBaseCards); // Read Data Base

            //InterpretGreToClientEventMessage(play);
            //string json = Tests.SerializationTest();
            //WriteLogText(xaml_json, json);
            //Tests.APITest("\"test\" : \"test\""); // Test the api

            ReadTestLogThreaded(test_log);

            //SeekLogThreaded(log_path);
        }

        /// <summary>
        /// Read the data base of cards
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataBaseCards"></param>
        /// <returns></returns>
        dynamic ReadDataBase(string file, out Dictionary<int, object> dataBaseCards)
        {
            if (File.Exists(file))
            {
                StreamReader sr = new StreamReader(file);
                jss.MaxJsonLength = 10000000;
                dynamic dataBase = jss.Deserialize<dynamic>(sr.ReadLine());
                dataBaseCards = new Dictionary<int, object>(dataBase["cards"].Count);
                foreach (var card in dataBase["cards"])
                {
                    if (!dataBaseCards.ContainsKey(card.Value["titleId"]))
                        dataBaseCards.Add(card.Value["titleId"], card.Value);
                }
                return dataBase;
            }
            else
            {
                dataBaseCards = null;
                return null;
            }
        }

        /// <summary>
        /// Seek the log file threaded
        /// </summary>
        /// <param name="filePath"></param>
        void SeekLogThreaded(string filePath)
        {
            var t = new Thread(() => SeekLog(filePath));
            t.IsBackground = true;
            t.Priority = ThreadPriority.Highest;
            t.Name = "SeekLog";
            t.Start();
        }

        /// <summary>
        /// Seek the log file
        /// </summary>
        /// <param name="filePath"></param>
        private void SeekLog(string filePath)
        {
            var initialFileSize = new FileInfo(filePath).Length;
            string text_prev = null;
            var lastReadLength = initialFileSize - 5000;
            if (lastReadLength < 0) lastReadLength = 0;

            while (true)
            {
                var fileSize = new FileInfo(filePath).Length;
                if (fileSize > lastReadLength)
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(lastReadLength, SeekOrigin.Begin);
                        var buffer = new byte[1024];
                        while (true)
                        {
                            var bytesRead = fs.Read(buffer, 0, buffer.Length);
                            lastReadLength += bytesRead;
                            if (bytesRead == 0)
                                break;
                            var text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            string[] stringSeparators = new string[] { "\r\n" };
                            string[] lines = text.Split(stringSeparators, StringSplitOptions.None);

                            if (text_prev != null)
                                lines[0] = string.Concat(text_prev, lines[0]);

                            for (int i = 0; i < lines.Length - 1; i++)
                            {
                                if (lines[i].Length > 0 && lines[i][0] == '{') // If it is the data line we need it starts with {
                                {
                                    if (lines[i].Length > 18 && lines[i].Substring(0, 18) == "{ \"transactionId\":") // make sure it is the data line
                                    {
                                        // Log message
                                        WriteLogText(xaml_top_right, "Messages: " + ++message_count + ", Errors: " + error_count);
                                        WriteLogText(xaml_log, lines[i]);
                                        // Log Error
                                        if (lines[i][lines[i].Length - 1] != '}')
                                        {
                                            WriteLogText(xaml_top_right, "Messages: " + message_count + ", Errors: " + ++error_count);
                                        }
                                        else // Interpret Message
                                            TryInterpretGreToClientEventMessage(lines[i]);
                                    }
                                }
                            }
                            text_prev = lines[lines.Length - 1];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read the test log threaded. Used for testing with old logs
        /// </summary>
        /// <param name="filePath"></param>
        void ReadTestLogThreaded(string filePath)
        {
            var t = new Thread(() => ReadTestLog(filePath));
            t.IsBackground = true;
            t.Priority = ThreadPriority.Highest;
            t.Name = "ReadTestLog";
            t.Start();
        }

        /// <summary>
        /// Read the test log. Used for testing with old logs
        /// </summary>
        /// <param name="filePath"></param>
        void ReadTestLog(string filePath)
        {
            //Thread.Sleep(5000);
            string line;
            int counter = 0;
            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] == '{') // If it is the data line we need it starts with {
                {
                    if (line.Length > 18 && line.Substring(0, 18) == "{ \"transactionId\":") // make sure it is the data line
                    {
                        // Log message
                        WriteLogText(xaml_top_right, "Messages: " + ++message_count + ", Errors: " + error_count);
                        WriteLogText(xaml_log, line);
                        // Log Error
                        if (line[line.Length - 1] != '}')
                            WriteLogText(xaml_top_right, "Messages: " + message_count + ", Errors: " + ++error_count);
                        else // Interpret Message
                            TryInterpretGreToClientEventMessage(line);
                        //Thread.Sleep(50);
                    }
                }
                Console.WriteLine(counter++);
            }
            file.Close();
        }

        /// <summary>
        /// Interpret message with try
        /// </summary>
        /// <param name="line"></param>
        public void TryInterpretGreToClientEventMessage(string line)
        {
            try
            {
                InterpretGreToClientEventMessage(line);
                WriteLogText(xaml_json, MainData.SerializeObject(), true);
            }
            catch (Exception e)
            {
                error_count++;
                Console.WriteLine(e);
                AppendEventText("Message Parsing Error");
            }
        }

        /// <summary>
        /// Interpret Message
        /// </summary>
        /// <param name="line"></param>
        public void InterpretGreToClientEventMessage(string line)
        {
            dynamic jsonRecord = jss.Deserialize<dynamic>(line);
            Dictionary<int, ActionClass> actions1 = new Dictionary<int, ActionClass>();
            Dictionary<int, ActionClass> actions2 = new Dictionary<int, ActionClass>();
            string timeStamp = "";
            foreach (var record in jsonRecord)
            {
                switch (record.Key)
                {
                    case "transactionId":
                        break;
                    case "requestId":
                        break;
                    case "authenticateResponse":
                        break;
                    case "timestamp":
                        timeStamp = record.Value;
                        break;
                    case "greToClientEvent":
                        foreach (var greToClientEvent in record.Value)
                        {
                            switch (greToClientEvent.Key)
                            {
                                case "greToClientMessages":
                                    foreach (var greToClientMessages in greToClientEvent.Value)
                                    {
                                        switch (greToClientMessages["type"])
                                        {
                                            case "GREMessageType_GameStateMessage":
                                                foreach (var gameStateMessage in greToClientMessages["gameStateMessage"])
                                                {
                                                    switch (gameStateMessage.Key)
                                                    {
                                                        case "type":
                                                            switch (gameStateMessage.Value)
                                                            {
                                                                case "GameStateType_Diff":
                                                                    // At turns afterwards the first turn
                                                                    break;
                                                                case "GameStateType_Full":
                                                                    // At the beginning of each game
                                                                    break;
                                                            }
                                                            break;
                                                        case "gameInfo":
                                                            foreach (var gameInfo in gameStateMessage.Value)
                                                            {
                                                                switch (gameInfo.Key)
                                                                {
                                                                    case "matchID":
                                                                        break;
                                                                    case "gameNumber":
                                                                        if (MainData.Match.MatchGames.Count < gameInfo.Value)
                                                                        {
                                                                            MAGame game = new MAGame();
                                                                            game.gameName = "Game " + gameInfo.Value;
                                                                            game.GameSummary.onPlay = false;
                                                                            MainData.Match.MatchGames.Add(game);
                                                                            AppendEventText("Game " + gameInfo.Value);
                                                                        }
                                                                        break;
                                                                    case "stage":
                                                                        gameStage = (EnumGameStage)Enum.Parse(typeof(EnumGameStage), gameInfo.Value, true);
                                                                        WriteLogText(xaml_message, gameStage.ToString());
                                                                        AppendEventText(gameStage.ToString());
                                                                        switch (gameStage)
                                                                        {
                                                                            case EnumGameStage.GameStage_None:
                                                                                break;
                                                                            case EnumGameStage.GameStage_Start:
                                                                                break;
                                                                            case EnumGameStage.GameStage_Play:
                                                                                break;
                                                                            case EnumGameStage.GameStage_GameOver:
                                                                                WriteLogText(xaml_message, "");
                                                                                WriteLogText(xaml_pl_lib, "");
                                                                                WriteLogText(xaml_op_bf, "");
                                                                                WriteLogText(xaml_pl_hd, "");
                                                                                WriteLogText(xaml_gameobjects, "");
                                                                                WriteLogText(xaml_top_left, "");
                                                                                ClearAllCards();
                                                                                Zones.Clear();
                                                                                GameObjects.Clear();
                                                                                break;
                                                                            default:
                                                                                break;
                                                                        }
                                                                        break;
                                                                } // switch (gameInfo.Key)
                                                            } // foreach (var gameInfo in gameStateMessage.Value)
                                                            break;
                                                        case "turnInfo":
                                                            foreach (var turnInfo in gameStateMessage.Value)
                                                            {
                                                                switch (turnInfo.Key)
                                                                {
                                                                    case "phase":
                                                                        break;
                                                                    case "step":
                                                                        break;
                                                                    case "turnNumber":
                                                                        if (MainData.Match.MatchGames.Last().GameTurns.Count < turnInfo.Value)
                                                                        {
                                                                            MATurn turn = new MATurn();
                                                                            turn.turnName = "Turn " + turnInfo.Value;
                                                                            AppendEventText("Turn " + turnInfo.Value);
                                                                            MainData.Match.MatchGames.Last().GameTurns.Add(turn);
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                            break;
                                                        case "players":
                                                            foreach (var player in gameStateMessage.Value)
                                                            {
                                                                if (player["systemSeatNumber"] == 1)
                                                                {
                                                                    if (player.ContainsKey("lifeTotal"))
                                                                        MainData.Match.MatchSnapShot.Player.lifeTotal = player["lifeTotal"];
                                                                    if (player.ContainsKey("startingLifeTotal"))
                                                                        MainData.Match.MatchSnapShot.Player.startingLifeTotal = player["startingLifeTotal"];
                                                                }
                                                                else
                                                                {
                                                                    if (player.ContainsKey("lifeTotal"))
                                                                        MainData.Match.MatchSnapShot.Opponent.lifeTotal = player["lifeTotal"];
                                                                    if (player.ContainsKey("startingLifeTotal"))
                                                                        MainData.Match.MatchSnapShot.Opponent.startingLifeTotal = player["startingLifeTotal"];
                                                                }
                                                            }
                                                            WriteLogText(xaml_top_left, "Seat 1: " + MainData.Match.MatchSnapShot.Player.lifeTotal + " / " +
                                                                MainData.Match.MatchSnapShot.Player.startingLifeTotal + ", Seat 2: " +
                                                                MainData.Match.MatchSnapShot.Opponent.lifeTotal + " / " +
                                                                MainData.Match.MatchSnapShot.Opponent.startingLifeTotal);
                                                            break;
                                                        case "zones":
                                                            foreach (var zone in gameStateMessage.Value)
                                                            {
                                                                ZoneClass currentZone = new ZoneClass();
                                                                if (zone.ContainsKey("zoneId"))
                                                                    currentZone.zoneId = zone["zoneId"];
                                                                if (zone.ContainsKey("type"))
                                                                    currentZone.type = (EnumZoneType)Enum.Parse(typeof(EnumZoneType), zone["type"], true);
                                                                if (zone.ContainsKey("visibility"))
                                                                    currentZone.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), zone["visibility"], true);
                                                                if (zone.ContainsKey("ownerSeatId"))
                                                                    currentZone.ownerSeatId = zone["ownerSeatId"];
                                                                if (zone.ContainsKey("viewers"))
                                                                    foreach (var viewers in zone["viewers"])
                                                                        currentZone.viewers.Add(viewers);
                                                                if (zone.ContainsKey("objectInstanceIds"))
                                                                    foreach (var objectInstanceIds in zone["objectInstanceIds"])
                                                                        currentZone.objectInstanceIds.Add(objectInstanceIds);
                                                                if (Zones.ContainsKey(currentZone.zoneId))
                                                                    Zones.Remove(currentZone.zoneId);
                                                                Zones.Add(currentZone.zoneId, currentZone);
                                                            }
                                                            break;
                                                        case "gameObjects":
                                                            foreach (var gameObject in gameStateMessage.Value)
                                                            {
                                                                GameObjectClass go = new GameObjectClass();
                                                                if (gameObject.ContainsKey("type"))
                                                                    go.type = (EnumGameObjectType)Enum.Parse(typeof(EnumGameObjectType), gameObject["type"], true);
                                                                if (gameObject.ContainsKey("zoneId"))
                                                                    go.zoneId = gameObject["zoneId"];
                                                                if (gameObject.ContainsKey("controllerSeatId"))
                                                                    go.controllerSeatId = gameObject["controllerSeatId"];
                                                                if (gameObject.ContainsKey("grpId"))
                                                                    go.grpId = gameObject["grpId"];
                                                                if (gameObject.ContainsKey("instanceId"))
                                                                    go.instanceId = gameObject["instanceId"];
                                                                if (gameObject.ContainsKey("ownerSeatId"))
                                                                    go.ownerSeatId = gameObject["ownerSeatId"];
                                                                if (gameObject.ContainsKey("visibility"))
                                                                    go.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), gameObject["visibility"], true);
                                                                if (gameObject.ContainsKey("name"))
                                                                    go.name = gameObject["name"];
                                                                if (GameObjects.ContainsKey(go.instanceId))
                                                                    GameObjects.Remove(go.instanceId);
                                                                GameObjects.Add(go.instanceId, go);
                                                            }
                                                            break;
                                                        case "diffDeletedInstanceIds":

                                                            break;
                                                        case "annotations":
                                                            break;
                                                        case "actions":
                                                            foreach (var act in gameStateMessage.Value)
                                                            {
                                                                ActionClass actionItem = new ActionClass();
                                                                switch (act["action"]["actionType"])
                                                                {
                                                                    case EnumActionType.ActionType_Cast:
                                                                    case EnumActionType.ActionType_Play:
                                                                        actionItem = new ActionClass(act["seatId"], act["action"]["actionType"], act["action"]["instanceId"]);
                                                                        break;
                                                                    case EnumActionType.ActionType_Activate:
                                                                        actionItem = new ActionClass(act["seatId"], act["action"]["actionType"], act["action"]["sourceId"]);
                                                                        break;
                                                                }
                                                                switch (act["seatId"])
                                                                {
                                                                    case 1:
                                                                        if (!actions1.ContainsKey(actionItem.instanceId))
                                                                            actions1.Add(actionItem.instanceId, actionItem);
                                                                        break;
                                                                    case 2:
                                                                        if (!actions2.ContainsKey(actionItem.instanceId))
                                                                            actions2.Add(actionItem.instanceId, actionItem);
                                                                        break;
                                                                }
                                                            }
                                                            break;
                                                    } // switch (gameStateMessage.Key)
                                                }
                                                break;
                                            case "GREMessageType_SetSettingsResp":
                                                break;
                                            case "GREMessageType_GetSettingsResp":
                                                break;
                                            case "GREMessageType_ActionsAvailableReq":
                                                break;
                                            case "GREMessageType_ConnectResp":
                                                foreach (var connectResp in greToClientMessages["connectResp"])
                                                {
                                                    switch (connectResp.Key)
                                                    {
                                                        case "deckMessage":
                                                            foreach (var deckMessage in connectResp.Value)
                                                            { 
                                                                switch (deckMessage.Key)
                                                                {
                                                                    case "deckCards":
                                                                        int a = 5;
                                                                        break;
                                                                    case "sideboardCards":
                                                                        int b = 5;
                                                                        break;
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }

                                                break;
                                        } // switch (greToClientMessages["type"])
                                    } // Foreach 
                                    break;
                            } // switch (greToClientEvent.Key)
                        } // foreach (var greToClientEvent in record.Value)
                        break;
                    case "matchGameRoomStateChangedEvent":
                        foreach (var matchGameRoomStateChangedEvent in record.Value)
                        {
                            string eventId = "";
                            switch (matchGameRoomStateChangedEvent.Key)
                            {
                                case "gameRoomInfo":
                                    foreach (var gameRoomInfo in matchGameRoomStateChangedEvent.Value)
                                    {
                                        switch (gameRoomInfo.Key)
                                        {
                                            case "gameRoomConfig":
                                                foreach (var gameRoomConfig in gameRoomInfo.Value)
                                                {
                                                    switch (gameRoomConfig.Key)
                                                    {
                                                        case "eventId":
                                                            eventId = gameRoomConfig.Value;
                                                            break;
                                                        case "reservedPlayers":
                                                            foreach (var reservedPlayers in gameRoomConfig.Value)
                                                            {
                                                                string name = reservedPlayers["playerName"];
                                                                int seatId = reservedPlayers["systemSeatId"];
                                                                if (seatId == 1)
                                                                    MainData.Match.MatchSnapShot.Player.name = name;
                                                                else
                                                                    MainData.Match.MatchSnapShot.Opponent.name = name;
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;
                                            case "stateType":
                                                switch (gameRoomInfo.Value)
                                                {
                                                    case "MatchGameRoomStateType_MatchCompleted":
                                                        MainData.Match.MatchSummary.EndDateTime = timeStamp; // set end time
                                                        MainData.PostToAPI(); // Send the data to the api
                                                        AppendEventText("Data Sent to API");
                                                        break;
                                                    case "MatchGameRoomStateType_Playing":
                                                        MainData.Match.MatchSummary.StartDateTime = timeStamp; // set start time
                                                        MainData.Match.MatchSummary.Format = eventId; // set the match format
                                                        break;
                                                }
                                                break;
                                            case "players":

                                                break;
                                            case "finalMatchResult":
                                                foreach (var finalMatchResult in gameRoomInfo.Value)
                                                {
                                                    switch (finalMatchResult.Key)
                                                    {
                                                        case "matchId":

                                                            break;
                                                        case "matchCompletedReason":

                                                            break;
                                                        case "resultList":
                                                            foreach (var resultList in finalMatchResult.Value)
                                                            {
                                                                foreach (var result in resultList)
                                                                {
                                                                    switch (result.Key)
                                                                    {
                                                                        case "scope":
                                                                            //MatchScope_Game
                                                                            //MatchScope_Match
                                                                            break;
                                                                        case "result":
                                                                            //ResultType_WinLoss
                                                                            break;
                                                                        case "winningTeamId":
                                                                            // 1 2
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;

                            }
                        }
                        break;
                } // Switch Record.Key
            } // Foreach jsonRecord


            ClearAllCards(); // Clear opponent and player cards

            // Snapshot based on Zones
            foreach (KeyValuePair<int, ZoneClass> zone in Zones)
            {
                foreach (int id in zone.Value.objectInstanceIds)
                {
                    if (!GameObjects.ContainsKey(id))
                    {
                        //continue;
                        GameObjects.Add(id, new GameObjectClass(id));
                    }
                    if (zone.Value.ownerSeatId == 1 || GameObjects[id].ownerSeatId == 1) // Player
                    {
                        switch (zone.Value.type)
                        {
                            case EnumZoneType.ZONETYPE_NONE:
                                break;
                            case EnumZoneType.ZONETYPE_LIBRARY:
                                MainData.Match.MatchSnapShot.Player.Library.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_HAND:
                                MainData.Match.MatchSnapShot.Player.Hand.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_BATTLEFIELD:
                                MainData.Match.MatchSnapShot.Player.BattleField.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_STACK:
                                break;
                            case EnumZoneType.ZONETYPE_GRAVEYARD:
                                MainData.Match.MatchSnapShot.Player.GraveYard.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_EXILE:
                                MainData.Match.MatchSnapShot.Player.Exile.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_COMMAND:
                                break;
                            case EnumZoneType.ZONETYPE_REVEALED:
                                break;
                            case EnumZoneType.ZONETYPE_LIMBO:
                                break;
                            case EnumZoneType.ZONETYPE_SIDEBOARD:
                                MainData.Match.MatchSnapShot.Player.SideBoard.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_PENDING:
                                break;
                            case EnumZoneType.ZONETYPE_PHASEDOUT:
                                break;
                            default:
                                break;
                        }
                    }
                    else if (zone.Value.ownerSeatId == 2 || GameObjects[id].ownerSeatId == 2)// Opponent
                    {
                        switch (zone.Value.type)
                        {
                            case EnumZoneType.ZONETYPE_NONE:
                                break;
                            case EnumZoneType.ZONETYPE_LIBRARY:
                                MainData.Match.MatchSnapShot.Opponent.Library.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_HAND:
                                MainData.Match.MatchSnapShot.Opponent.Hand.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_BATTLEFIELD:
                                MainData.Match.MatchSnapShot.Opponent.BattleField.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_STACK:
                                break;
                            case EnumZoneType.ZONETYPE_GRAVEYARD:
                                MainData.Match.MatchSnapShot.Opponent.GraveYard.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_EXILE:
                                MainData.Match.MatchSnapShot.Opponent.Exile.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_COMMAND:
                                break;
                            case EnumZoneType.ZONETYPE_REVEALED:
                                break;
                            case EnumZoneType.ZONETYPE_LIMBO:
                                break;
                            case EnumZoneType.ZONETYPE_SIDEBOARD:
                                MainData.Match.MatchSnapShot.Opponent.SideBoard.Add(GameObjects[id]);
                                break;
                            case EnumZoneType.ZONETYPE_PENDING:
                                break;
                            case EnumZoneType.ZONETYPE_PHASEDOUT:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Game objects
            AddToGameObjectView(GameObjects);

            AddToView("Player Library", MainData.Match.MatchSnapShot.Player.Library, xaml_pl_lib, xaml_pl_lib_title);

            AddToView("Player Hand", MainData.Match.MatchSnapShot.Player.Hand, xaml_pl_hd, xaml_pl_hd_title);
            AddToView("Player BattleField", MainData.Match.MatchSnapShot.Player.BattleField, xaml_pl_bf, xaml_pl_bf_title);
            AddToView("Player GraveYard", MainData.Match.MatchSnapShot.Player.GraveYard, xaml_pl_gy, xaml_pl_gy_title);
            AddToView("Player Exile", MainData.Match.MatchSnapShot.Player.Exile, xaml_pl_ex, xaml_pl_ex_title);

            AddToView("Opponent Hand", MainData.Match.MatchSnapShot.Opponent.Hand, xaml_op_hd, xaml_op_hd_title);
            AddToView("Opponet BattleField", MainData.Match.MatchSnapShot.Opponent.BattleField, xaml_op_bf, xaml_op_bf_title);
            AddToView("Opponent GraveYard", MainData.Match.MatchSnapShot.Opponent.GraveYard, xaml_op_gy, xaml_op_gy_title);
            AddToView("Opponent Exile", MainData.Match.MatchSnapShot.Opponent.Exile, xaml_op_ex, xaml_op_ex_title);
        }

        public void ClearAllCards()
        {
            MainData.Match.MatchSnapShot.Player.Library.Clear();
            MainData.Match.MatchSnapShot.Player.Hand.Clear();
            MainData.Match.MatchSnapShot.Player.BattleField.Clear();
            MainData.Match.MatchSnapShot.Player.GraveYard.Clear();
            MainData.Match.MatchSnapShot.Player.Exile.Clear();
            MainData.Match.MatchSnapShot.Player.SideBoard.Clear();

            MainData.Match.MatchSnapShot.Opponent.Library.Clear();
            MainData.Match.MatchSnapShot.Opponent.Hand.Clear();
            MainData.Match.MatchSnapShot.Opponent.BattleField.Clear();
            MainData.Match.MatchSnapShot.Opponent.GraveYard.Clear();
            MainData.Match.MatchSnapShot.Opponent.Exile.Clear();
            MainData.Match.MatchSnapShot.Opponent.SideBoard.Clear();
        }

        public void AddToGameObjectView(Dictionary<int, GameObjectClass> GameObjects)
        {
            string cards = "";
            foreach (var go in GameObjects)
            {
                if (dataBaseCards.ContainsKey(go.Value.name))
                {
                    string card_name = dataBaseCards[go.Value.name]["name"];
                    cards = string.Concat(cards, go.Key, ", ", go.Value.name, ", (", card_name, ")", "\r\n");
                }
                else
                    cards = string.Concat(cards, go.Key, ", ", go.Value.name, "\r\n");
            }
            WriteLogText(xaml_gameobjects, cards);

        }

        public void AddToView(string title, List<GameObjectClass> go, System.Windows.Controls.TextBox tb, System.Windows.Controls.Label lb)
        {
            string cards = "";
            int counter = 0;
            foreach (var item in go)
            {
                if (dataBaseCards.ContainsKey(item.name))
                {
                    string card_name = dataBaseCards[item.name]["name"];
                    cards = string.Concat(cards, item.instanceId, ", ", item.name, ", (", card_name, ")", "\r\n");
                }
                else
                    cards = string.Concat(cards, item.instanceId, ", ", item.name, "\r\n");
                counter++;
            }
            WriteLogText(tb, cards);
            WriteLogText(lb, string.Concat(title, " (", counter, " Cards)"));
        }

        /// <summary>
        /// Write the text to label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        private void WriteLogText(System.Windows.Controls.Label label, string text)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                label.Content = text;
            }));
        }

        /// <summary>
        /// Write the text to textbox
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        private void WriteLogText(System.Windows.Controls.TextBox textBox, string text, bool scrollToEnd = true)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                textBox.Text = text;
                if (scrollToEnd)
                    textBox.ScrollToEnd();
            }));
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="path"></param>
        private void CheckFile(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show(path + " file does not exists. Application will now close.", "Log file does not exists.", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Mouse down for window drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Append event text
        /// </summary>
        /// <param name="text"></param>
        public void AppendEventText(string text)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                while (xaml_events.Document.Blocks.Count > 100)
                    xaml_events.Document.Blocks.Remove(xaml_events.Document.Blocks.FirstBlock); // Remove First line
                TextRange tr = new TextRange(xaml_events.Document.ContentEnd, xaml_events.Document.ContentEnd);
                tr.Text = $"{DateTime.Now.ToString("[HH:mm:ss] ")}{text}{Environment.NewLine}";
                xaml_events.ScrollToEnd();
            }));
        }

    }
}
