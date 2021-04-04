using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicAssistant
{
    public class greTypes
    {
        public enum EnumGameStage
        {
            GameStage_None = 0,
            GameStage_Start = 1,
            GameStage_Play = 2,
            GameStage_GameOver = 3
        }

        public enum EnumActionType
        {
            ActionType_None = 0,
            ActionType_Cast = 1,
            ActionType_Activate = 2,
            ActionType_Play = 3,
            ActionType_Activate_Mana = 4,
            ActionType_Pass = 5,
            ActionType_Activate_Test = 6,
            ActionType_Special = 7,
            ActionType_Special_TurnFaceUp = 8,
            ActionType_ResolutionCost = 9,
            ActionType_CastLeft = 10,
            ActionType_CastRight = 11,
            ActionType_Make_Payment = 12,
            ActionType_CastingTimeOption = 13,
            ActionType_CombatCost = 14,
            ActionType_OpeningHandAction = 15,
            ActionType_CastAdventure = 16,
            ActionType_FloatMana = 17,
            ActionType_Placeholder1 = 18,
            ActionType_Placeholder2 = 19,
            ActionType_Placeholder3 = 20,
            ActionType_Placeholder4 = 21,
            ActionType_Placeholder5 = 22
        }

        public enum EnumVisibility
        {
            VISIBILITY_NONE = 0,
            VISIBILITY_PUBLIC = 1,
            VISIBILITY_PRIVATE = 2,
            VISIBILITY_HIDDEN = 3
        };

        public enum GameObjectType
        {
            GameObjectType_None = 0,
            GameObjectType_Card = 1,
            GameObjectType_Token = 2,
            GameObjectType_Ability = 3,
            GameObjectType_Emblem = 4,
            GameObjectType_SplitCard = 5,
            GameObjectType_SplitLeft = 6,
            GameObjectType_SplitRight = 7,
            GameObjectType_RevealedCard = 8,
            GameObjectType_MDFCBack = 9
        };

        public class GameObjectClass
        {
            public GameObjectType type;
            public int zoneId;
            public int controllerSeatId;
            public int grpId;
            public int instanceId;
            public int ownerSeatId;
            public EnumVisibility visibility;
            public int name;
            public GameObjectClass()
            {

            }
            public GameObjectClass(string type, int zoneId, int controllerSeatId, int grpId, int instanceId, int ownerSeatId, string visibility, int name)
            {
                this.type = (GameObjectType)Enum.Parse(typeof(GameObjectType), type, true);
                this.zoneId = zoneId;
                this.controllerSeatId = controllerSeatId;
                this.grpId = grpId;
                this.instanceId = instanceId;
                this.ownerSeatId = ownerSeatId;
                this.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), visibility, true);
                this.name = name;
            }
            public GameObjectClass(string type, int controllerSeatId, int grpId, int instanceId, int ownerSeatId, string visibility, int name)
            {
                this.type = (GameObjectType)Enum.Parse(typeof(GameObjectType), type, true);
                this.controllerSeatId = controllerSeatId;
                this.grpId = grpId;
                this.instanceId = instanceId;
                this.ownerSeatId = ownerSeatId;
                this.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), visibility, true);
                this.name = name;
            }
            public GameObjectClass(string type, int zoneId, int controllerSeatId, int grpId, int instanceId, int ownerSeatId, string visibility)
            {
                this.type = (GameObjectType)Enum.Parse(typeof(GameObjectType), type, true);
                this.zoneId = zoneId;
                this.controllerSeatId = controllerSeatId;
                this.grpId = grpId;
                this.instanceId = instanceId;
                this.ownerSeatId = ownerSeatId;
                this.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), visibility, true);
            }
            public GameObjectClass(string type, int controllerSeatId, int grpId, int instanceId, int ownerSeatId, string visibility)
            {
                this.type = (GameObjectType)Enum.Parse(typeof(GameObjectType), type, true);
                this.controllerSeatId = controllerSeatId;
                this.grpId = grpId;
                this.instanceId = instanceId;
                this.ownerSeatId = ownerSeatId;
                this.visibility = (EnumVisibility)Enum.Parse(typeof(EnumVisibility), visibility, true);
            }
        }
        public class ActionClass
        {
            public int seatId;
            public EnumActionType actionType;
            public int instanceId;
            public ActionClass() { }
            public ActionClass(int seatId, string actionType, int instanceId)
            {
                this.seatId = seatId;
                this.actionType = (EnumActionType)Enum.Parse(typeof(EnumActionType), actionType, true);
                this.instanceId = instanceId;
            }
        }
    }
}
