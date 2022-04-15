using System;

namespace ImbaXIV
{
    public enum EntityType
    {
        PlayerCharacter = 1,
        BattleNpc = 2,
        EventNpc = 3,
        Aetheryte = 5,
        EventObject = 7,
        Companion = 9,
        Retainer = 10,
        HousingEventObject = 12,
        Unknown
    }

    public enum FloatingPlateType
    {
        MSQ_ONGOING_QUEST = 0x11623,
        SMALL_MSQ_ONGOING_QUEST = 0x11624,
        MSQ_COMPLETED_QUEST = 0x11625,
        GREEN_ONGOING_QUEST = 0x11637,
        SMALL_GREEN_ONGOING_QUEST = 0x11638,
        GREEN_COMPLETED_QUEST = 0x11639,
        BLUE_ONGOING_QUEST = 0x116af,
        SMALL_BLUE_ONGOING_QUEST = 0x116b0,
        BLUE_COMPLETED_QUEST = 0x116b1,
        UNKNOWN
    }

    public class Entity
    {
        public String Name;
        public PosInfo Pos;
        public EntityType Type;
        public FloatingPlateType QuestType;
        public bool IsQuestObject;
        public long StructPtr;
        public bool IsVisible;

        public Entity()
        {
            Pos = new PosInfo();
            Zeroise();
        }

        public Entity(String name, PosInfo pos)
        {
            Name = name;
            Pos = pos;
        }

        public void Zeroise()
        {
            Name = "";
            Pos.Zeroise();
            StructPtr = 0;
        }
    }
}
