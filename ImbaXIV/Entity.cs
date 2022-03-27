using System;

namespace ImbaXIV
{
    enum EntityType
    {
        NPC,
        OBJECT,
        ENEMY,
        UNKNOWN
    }

    enum FloatingPlateType
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

    class Entity
    {
        public String Name;
        public PosInfo Pos;
        public EntityType Type;
        public FloatingPlateType QuestType;
        public bool IsQuestObject;
        public long StructPtr;

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
