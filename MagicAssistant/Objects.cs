using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MagicAssistant.greTypes;

namespace MagicAssistant
{
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
        public List<MAGameTurns> GameTurns = new List<MAGameTurns>();
    }
    public class MAGameTurns
    {
        public int life;
        public int damageDealed;
        public int usedMana;
        public int availableMana;
        public List<GameObjectClass> Deck = new List<GameObjectClass>();
        public List<GameObjectClass> SidBoard = new List<GameObjectClass>();
        public List<GameObjectClass> BattleField = new List<GameObjectClass>();
        public List<GameObjectClass> GraveYard = new List<GameObjectClass>();
        public List<GameObjectClass> Exile = new List<GameObjectClass>();
        public List<GameObjectClass> Hand = new List<GameObjectClass>();
        public List<GameObjectClass> Playable = new List<GameObjectClass>();
        public List<GameObjectClass> Attacked = new List<GameObjectClass>();
    }
    public class MAGameSummary
    {
        public bool onPlay;
        public bool won;
        public List<GameObjectClass> StratingHand = new List<GameObjectClass>();
        public List<GameObjectClass> StartingDeck = new List<GameObjectClass>();
        public List<GameObjectClass> StartingSidBoard = new List<GameObjectClass>();
    }
    public class MAMatchSnapShot
    {
        public MAPlayer Player = new MAPlayer();
        public MAPlayer Opponent = new MAPlayer();
    }
    public class MAPlayer
    {
        public string name;
        public int life;
        public List<GameObjectClass> Deck = new List<GameObjectClass>();
        public List<GameObjectClass> SidBoard = new List<GameObjectClass>();
        public List<GameObjectClass> BattleField = new List<GameObjectClass>();
        public List<GameObjectClass> GraveYard = new List<GameObjectClass>();
        public List<GameObjectClass> Exile = new List<GameObjectClass>();
        public List<GameObjectClass> Hand = new List<GameObjectClass>();
        public List<GameObjectClass> Playable = new List<GameObjectClass>();
    }
}
