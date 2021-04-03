using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Windows.Documents;
using static MagicAssistant.greTypes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MagicAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        long offset;
        int nextByte;
        int message_count = 0;
        int error_count = 0;
        string user_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\Wizards Of The Coast\\MTGA\\";
        string data_base_file = AppDomain.CurrentDomain.BaseDirectory + "\\database.json";
        string log_file = "player.log";
        JavaScriptSerializer jss = new JavaScriptSerializer();
        string[] player_names = new string[2];
        int[] player_life = new int[4];
        EnumGameStage gameStage = EnumGameStage.GameStage_None;
        Dictionary<int, GameObjectClass> gameObjects = new Dictionary<int, GameObjectClass>();
        dynamic dataBaseJson;
        Dictionary<int, dynamic> dataBaseCards;
        Dictionary<int, int> Battlefield = new Dictionary<int, int>();
        public MainWindow()
        {
            InitializeComponent();

            string log_path = user_path + log_file;

            // Check if log exists
            CheckFile(log_path);

            // Read Data Base
            dataBaseJson = ReadDataBase(data_base_file, out dataBaseCards);

            //InterpretGreToClientEventMessage(play);

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

            string result = data.SerializeObject();
            Console.WriteLine(result);

            SeekLogThreaded(log_path);
        }
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

        void SeekLogThreaded(string filePath)
        {
            var t = new Thread(() => SeekLog(filePath));
            t.IsBackground = true;
            t.Priority = ThreadPriority.Highest;
            t.Name = "SeekLog";
            t.Start();
        }

        private void SeekLog(string filePath)
        {
            var initialFileSize = new FileInfo(filePath).Length;
            string text_prev = null;
            var lastReadLength = initialFileSize - 5000;
            if (lastReadLength < 0) lastReadLength = 0;

            while (true)
            {
                try
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
                                    if (lines[i][0] == '{') // If it is the data line wee need it starts with {
                                    {
                                        if (lines[i].Substring(0, 18) == "{ \"transactionId\":") // make sure it is the data line
                                        {
                                            // Log message
                                            WriteLogText(xaml_top_right, "Messages: " + ++message_count + ", Errors: " + error_count);
                                            WriteLogText(xaml_log, lines[i]);
                                            // Log Error
                                            if (lines[i][lines[i].Length - 1] != '}')
                                                WriteLogText(xaml_top_right, "Messages: " + message_count + ", Errors: " + ++error_count);
                                            else // Interpret Message
                                                InterpretGreToClientEventMessageThreaded(lines[i]);
                                        }
                                    }
                                }
                                text_prev = lines[lines.Length - 1];
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void WriteLogText(System.Windows.Controls.Label label, string text)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                label.Content = text;
            }));
        }
        private void WriteLogText(System.Windows.Controls.TextBox textBox, string text)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                textBox.Text = text;
                textBox.ScrollToEnd();
            }));
        }

        private void CheckFile(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show(path + " file does not exists. Application will now close.", "Log file does not exists.", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

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

        void InterpretGreToClientEventMessageThreaded(string line)
        {
            var t = new Thread(() => TryInterpretGreToClientEventMessage(line))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "InterpretGreToClientEventMessage"
            };
            t.Start();
        }

        public void TryInterpretGreToClientEventMessage(string line)
        {
            try
            {
                InterpretGreToClientEventMessage(line);
            }
            catch (Exception)
            {
                error_count++;
                AppendEventText("Could not parse Message");
            }
        }
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
                                                                                Battlefield.Clear();
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
                                                        case "players":
                                                            foreach (var player in gameStateMessage.Value)
                                                            {
                                                                if (player["systemSeatNumber"] == 1)
                                                                {
                                                                    player_life[0] = player["lifeTotal"];
                                                                    player_life[1] = player["startingLifeTotal"];
                                                                }
                                                                else
                                                                {
                                                                    player_life[2] = player["lifeTotal"];
                                                                    player_life[3] = player["startingLifeTotal"];
                                                                }
                                                            }
                                                            WriteLogText(xaml_top_left, "Seat 1: " + player_life[0] + " / " + player_life[1] + ", Seat 2: " + player_life[2] + " / " + player_life[3]);
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
                                                                    gameObjects.Add(gameObject["instanceId"], new GameObjectClass(gameObject["type"], gameObject["zoneId"], gameObject["controllerSeatId"],
                                                                    gameObject["grpId"], gameObject["instanceId"], gameObject["ownerSeatId"], gameObject["visibility"], gameObject["name"]));
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
                                                                    case "ActionType_Cast":
                                                                    case "ActionType_Play":
                                                                        actionItem = new ActionClass(act["seatId"], act["action"]["actionType"], act["action"]["instanceId"]);
                                                                        break;
                                                                    case "ActionType_Activate":
                                                                    default:
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
                                                                player_names[seatId - 1] = name;
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
    }
}
