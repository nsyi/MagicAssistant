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
        string test_log = "C:\\Users\\nsyig\\Documents\\My documents\\UW\\J017 - MTGA\\Player\\Player.log";

        JavaScriptSerializer jss = new JavaScriptSerializer();
        readonly dynamic dataBaseJson; // Database of cards
        Dictionary<int, dynamic> dataBaseCards;

        public DataObject MainData = new DataObject(); // Main data file 

        EnumGameStage gameStage = EnumGameStage.GameStage_None;
        Dictionary<int, GameObjectClass> gameObjects = new Dictionary<int, GameObjectClass>();

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
            Thread.Sleep(5000);
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
                        Thread.Sleep(20);
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
                WriteLogText(xaml_json, MainData.SerializeObject(), false);
            }
            catch (Exception e)
            {
                error_count++;
                Console.WriteLine(e);
                AppendEventText("Could not parse Message" + e);
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
                                                                                MainData.Match.MatchSnapShot.Player.GraveYard.Clear();
                                                                                MainData.Match.MatchSnapShot.Opponent.GraveYard.Clear();
                                                                                WriteLogText(xaml_message, "");
                                                                                WriteLogText(xaml_player, "");
                                                                                WriteLogText(xaml_opponent, "");
                                                                                WriteLogText(xaml_player_hand, "");
                                                                                WriteLogText(xaml_gameobjects, "");
                                                                                WriteLogText(xaml_top_left, "");
                                                                                gameObjects.Clear();
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
                                                                switch (zone["type"])
                                                                {
                                                                    case "ZoneType_Stack":
                                                                        if (zone.ContainsKey("objectInstanceIds"))
                                                                        {
                                                                            string list = "";
                                                                            foreach (var objectInstanceIds in zone["objectInstanceIds"])
                                                                            {
                                                                                list = list + ", " + objectInstanceIds;
                                                                            }
                                                                            //AppendEventText("ZoneType_Stack" + list);
                                                                        }
                                                                        break;

                                                                    case "ZoneType_Battlefield":
                                                                        if (zone.ContainsKey("objectInstanceIds"))
                                                                        {
                                                                            Battlefield.Clear();
                                                                            string list = "";
                                                                            foreach (var objectInstanceIds in zone["objectInstanceIds"])
                                                                            {
                                                                                list = list + ", " + objectInstanceIds;
                                                                                if (!Battlefield.ContainsKey(objectInstanceIds))
                                                                                    Battlefield.Add(objectInstanceIds, objectInstanceIds);
                                                                            }
                                                                            AppendEventText("ZoneType_Battlefield" + list);
                                                                        }
                                                                        break;
                                                                    case "ZoneType_Limbo":
                                                                        if (zone.ContainsKey("objectInstanceIds"))
                                                                        {
                                                                            string list = "";
                                                                            foreach (var objectInstanceIds in zone["objectInstanceIds"])
                                                                            {
                                                                                list = list + ", " + objectInstanceIds;
                                                                            }
                                                                            //AppendEventText("ZoneType_Limbo" + list);
                                                                        }
                                                                        break;
                                                                    case "ZoneType_Graveyard":
                                                                        if (zone.ContainsKey("objectInstanceIds"))
                                                                        {
                                                                            string list = "";
                                                                            foreach (var objectInstanceIds in zone["objectInstanceIds"])
                                                                            {
                                                                                list = list + ", " + objectInstanceIds;
                                                                            }
                                                                            //AppendEventText("ZoneType_Graveyard" + list);
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                            break;
                                                        case "gameObjects":
                                                            foreach (var gameObject in gameStateMessage.Value)
                                                                if (!gameObjects.ContainsKey(gameObject["instanceId"]))
                                                                {
                                                                    if (gameObject.ContainsKey("name") && gameObject.ContainsKey("zoneId"))
                                                                    {
                                                                        gameObjects.Add(gameObject["instanceId"], new GameObjectClass(gameObject["type"], gameObject["zoneId"], gameObject["controllerSeatId"],
                                                                            gameObject["grpId"], gameObject["instanceId"], gameObject["ownerSeatId"], gameObject["visibility"], gameObject["name"]));
                                                                    }
                                                                    else
                                                                    {
                                                                        gameObjects.Add(gameObject["instanceId"], new GameObjectClass(gameObject["type"], gameObject["controllerSeatId"],
                                                                            gameObject["grpId"], gameObject["instanceId"], gameObject["ownerSeatId"], gameObject["visibility"]));
                                                                    }
                                                                }
                                                            //AppendEventText(cards);
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
                                        } // switch (greToClientMessages["type"])
                                    } // Foreach 
                                    break;
                            } // switch (greToClientEvent.Key)
                        } // foreach (var greToClientEvent in record.Value)
                        break;
                    case "matchGameRoomStateChangedEvent":
                        foreach (var matchGameRoomStateChangedEvent in record.Value)
                        {
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
                                        }
                                    }
                                    break;

                            }
                        }
                        break;
                } // Switch Record.Key
            } // Foreach jsonRecord

            // Player hand
            string actions = "";
            foreach (var action in actions1)
            {
                if (action.Value.actionType != EnumActionType.ActionType_Activate)
                {

                    string card_name = "";
                    if (gameObjects.ContainsKey(action.Value.instanceId))
                    {
                        GameObjectClass go = gameObjects[action.Value.instanceId];
                        if (dataBaseCards.ContainsKey(go.name))
                            card_name = dataBaseCards[go.name]["name"];
                    }
                    actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
                }
            }

            if (actions != "")
                WriteLogText(xaml_player_hand, actions);

            // Player cards
            actions = "";
            foreach (var action in actions1)
            {
                if (action.Value.actionType == EnumActionType.ActionType_Activate)
                {

                    string card_name = "";
                    if (gameObjects.ContainsKey(action.Value.instanceId))
                    {
                        GameObjectClass go = gameObjects[action.Value.instanceId];
                        if (dataBaseCards.ContainsKey(go.name))
                            card_name = dataBaseCards[go.name]["name"];
                    }
                    actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
                    if (Battlefield.ContainsKey(action.Value.instanceId))
                        Battlefield.Remove(action.Value.instanceId);
                }
            }
            // Player  cards from battlefield
            foreach (var card in Battlefield)
            {
                string card_name = "";
                if (gameObjects.ContainsKey(card.Key))
                {
                    GameObjectClass go = gameObjects[card.Key];
                    if (dataBaseCards.ContainsKey(go.name))
                        card_name = dataBaseCards[go.name]["name"];
                    if (gameObjects[card.Key].controllerSeatId == 1)
                        actions = string.Concat(actions, card_name, ", ", card.Key, "\r\n");
                }
            }
            if (actions != "")
                WriteLogText(xaml_player, actions);

            // Opponent cards
            actions = "";
            foreach (var action in actions2)
            {
                string card_name = "";
                if (gameObjects.ContainsKey(action.Value.instanceId))
                {
                    GameObjectClass go = gameObjects[action.Value.instanceId];
                    if (dataBaseCards.ContainsKey(go.name))
                        card_name = dataBaseCards[go.name]["name"];
                }
                actions = string.Concat(actions, card_name, ", ", action.Value.actionType, ", ", action.Value.instanceId, "\r\n");
                if (Battlefield.ContainsKey(action.Value.instanceId))
                    Battlefield.Remove(action.Value.instanceId);
            }
            // Opponent cards from battlefield
            foreach (var card in Battlefield)
            {
                string card_name = "";
                if (gameObjects.ContainsKey(card.Key))
                {
                    GameObjectClass go = gameObjects[card.Key];
                    if (dataBaseCards.ContainsKey(go.name))
                        card_name = dataBaseCards[go.name]["name"];
                    if (gameObjects[card.Key].controllerSeatId == 2)
                        actions = string.Concat(actions, card_name, ", ", card.Key, "\r\n");
                }
            }

            if (actions != "")
                WriteLogText(xaml_opponent, actions);

            // Game objects
            string objects = "";
            foreach (var go in gameObjects)
            {
                string card_name = "";
                if (dataBaseCards.ContainsKey(go.Value.name))
                    card_name = dataBaseCards[go.Value.name]["name"];
                objects = string.Concat(objects, card_name, ", ", go.Key, ", ", go.Value.name, "\r\n");
            }

            if (actions != "")
            {
                WriteLogText(xaml_gameobjects, objects);
            }
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
