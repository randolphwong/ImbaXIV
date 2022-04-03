using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImbaXIV
{
    class EntityFactory
    {
        private const long ENTITY_MANAGER_PTR_OFFSET = 0x1ebdf70;
        private const long ENTITY_MANAGER_ENTITY_CONTAINER_LIST_OFFSET = 0x1a0;
        private const int ENTITY_CONTAINER_SIZE = 0x30;
        private const int ENTITY_CONTAINER_CONTAINER1_OFFSET = 0x0;
        private const int ENTITY_CONTAINER_CONTAINER2_OFFSET = 0x8;
        private const int ENTITY_CONTAINER_CONTAINER3_OFFSET = 0x10;
        private const int ENTITY_CONTAINER_TYPE_OFFSET = 0x18;
        private const int ENTITY_CONTAINER_TYPE_INVALID_VALUE = 0x101;
        private const int ENTITY_CONTAINER_UNK_STRUCT_OFFSET = 0x28;
        private const int UNK_STRUCT_ENTITY_OFFSET = 0x8;

        private const uint GENERIC_ENTITY_STRUCT_SIZE = 0x1c0;
        private const long NPC_VTABLE_OFFSET = 0x0176f0e0;
        private const long ENEMY_VTABLE_OFFSET = 0x0177f5f0;
        private const long OBJECT_VTABLE_OFFSET = 0x018d9b10;

        private const int ENTITY_NAME_OFFSET = 0x30;
        private const int ENTITY_NAME_SIZE = 0x40;

        private const int ENTITY_KIND_OFFSET = 0x8c;

        private const int ENTITY_POS_OFFSET = 0xa0;
        private const int POS_X_OFFSET = 0x0;
        private const int POS_Y_OFFSET = 0x8;
        private const int POS_Z_OFFSET = 0x4;
        private const int POS_A_OFFSET = 0x10;

        private const int ENTITY_FLOATING_PLATE_OFFSET = 0x100;

        private const int ENTITY_QUEST_OBJ_VAR1_OFFSET = 0x70;
        private const int ENTITY_QUEST_OBJ_VAR2_OFFSET = 0x94;
        private const int ENTITY_QUEST_OBJ_VAR3_OFFSET = 0xc0;
        private const int ENTITY_QUEST_OBJ_VAR4_OFFSET = 0x114;
        private const int ENTITY_QUEST_OBJ_VAR5_OFFSET = 0x124;
        private const int ENTITY_QUEST_OBJ_VAR1_VALUE = 7;
        private const int ENTITY_QUEST_OBJ_VAR2_VALUE1 = 0xd1bf;
        private const int ENTITY_QUEST_OBJ_VAR2_VALUE2 = 0xd1bc;
        private const int ENTITY_QUEST_OBJ_VAR3_VALUE = 0x3f000000;

        private const long MAIN_CHAR_STRUCTC_PTR_OFFSET = 0x01e7ccf0;

        public static LinkedList<Entity> GetEntities(ProcessReader reader)
        {
            LinkedList<Entity> entities = new LinkedList<Entity>();
            long entityManagerPtr = reader.ReadInt64(ENTITY_MANAGER_PTR_OFFSET, reader.ModuleBase);
            if (entityManagerPtr == 0)
            {
                return entities;
            }

            long entityListPtr = reader.ReadInt64(entityManagerPtr + ENTITY_MANAGER_ENTITY_CONTAINER_LIST_OFFSET);
            HashSet<long> seenEntities = new HashSet<long>();
            Queue<long> entityQueue = new Queue<long>();
            entityQueue.Enqueue(entityListPtr);
            while (entityQueue.Count > 0)
            {
                long curEntityContainer = entityQueue.Dequeue();
                seenEntities.Add(curEntityContainer);
                byte[] curEntityContainerBytes = reader.ReadBytes(curEntityContainer, ENTITY_CONTAINER_SIZE);
                long first = BitConverter.ToInt64(curEntityContainerBytes, ENTITY_CONTAINER_CONTAINER1_OFFSET);
                long second = BitConverter.ToInt64(curEntityContainerBytes, ENTITY_CONTAINER_CONTAINER2_OFFSET);
                long third = BitConverter.ToInt64(curEntityContainerBytes, ENTITY_CONTAINER_CONTAINER3_OFFSET);
                long unkStructPtr = BitConverter.ToInt64(curEntityContainerBytes, ENTITY_CONTAINER_UNK_STRUCT_OFFSET);
                long containerType = BitConverter.ToInt64(curEntityContainerBytes, ENTITY_CONTAINER_TYPE_OFFSET);
                if (!seenEntities.Contains(first))
                {
                    entityQueue.Enqueue(first);
                    seenEntities.Add(first);
                }
                if (!seenEntities.Contains(second))
                {
                    entityQueue.Enqueue(second);
                    seenEntities.Add(second);
                }
                if (!seenEntities.Contains(third))
                {
                    entityQueue.Enqueue(third);
                    seenEntities.Add(third);
                }
                if (containerType == ENTITY_CONTAINER_TYPE_INVALID_VALUE)
                    continue;
                if (unkStructPtr == 0)
                    continue;
                long entityStructPtr = reader.ReadInt64(unkStructPtr + UNK_STRUCT_ENTITY_OFFSET);
                if (entityStructPtr == 0)
                    continue;

                Entity entity = EntityFactory.GetEntity(reader, entityStructPtr);
                entities.AddLast(entity);
            }
            return entities;
        }

        public static Entity GetEntity(ProcessReader reader, long entityStructAddr)
        {
            Entity entity = new Entity();
            byte[] entityStructBytes = reader.ReadBytes(entityStructAddr, GENERIC_ENTITY_STRUCT_SIZE);

            int kind = entityStructBytes[ENTITY_KIND_OFFSET];
            entity.Type = Enum.IsDefined(typeof(EntityType), kind) ? (EntityType)kind : EntityType.Unknown;

            FloatingPlateType questType = (FloatingPlateType)BitConverter.ToInt32(entityStructBytes, ENTITY_FLOATING_PLATE_OFFSET);
            bool isValidFloatingQuestPlate = questType == FloatingPlateType.MSQ_ONGOING_QUEST ||
                                             questType == FloatingPlateType.SMALL_MSQ_ONGOING_QUEST ||
                                             questType == FloatingPlateType.MSQ_COMPLETED_QUEST ||
                                             questType == FloatingPlateType.GREEN_ONGOING_QUEST ||
                                             questType == FloatingPlateType.SMALL_GREEN_ONGOING_QUEST ||
                                             questType == FloatingPlateType.GREEN_COMPLETED_QUEST ||
                                             questType == FloatingPlateType.BLUE_ONGOING_QUEST ||
                                             questType == FloatingPlateType.SMALL_BLUE_ONGOING_QUEST ||
                                             questType == FloatingPlateType.BLUE_COMPLETED_QUEST;
            entity.QuestType = isValidFloatingQuestPlate ? questType : FloatingPlateType.UNKNOWN;

            int questObjVar1 = BitConverter.ToInt32(entityStructBytes, ENTITY_QUEST_OBJ_VAR1_OFFSET);
            int questObjVar2 = BitConverter.ToInt32(entityStructBytes, ENTITY_QUEST_OBJ_VAR2_OFFSET);
            int questObjVar3 = BitConverter.ToInt32(entityStructBytes, ENTITY_QUEST_OBJ_VAR3_OFFSET);
            int questObjVar4 = BitConverter.ToInt32(entityStructBytes, ENTITY_QUEST_OBJ_VAR4_OFFSET);
            int questObjVar5 = BitConverter.ToInt32(entityStructBytes, ENTITY_QUEST_OBJ_VAR5_OFFSET);
            entity.IsQuestObject = ((questObjVar1 & ENTITY_QUEST_OBJ_VAR1_VALUE) == ENTITY_QUEST_OBJ_VAR1_VALUE) &&
                                   (((questObjVar2 & ENTITY_QUEST_OBJ_VAR2_VALUE1) == ENTITY_QUEST_OBJ_VAR2_VALUE1) ||
                                   ((questObjVar2 & ENTITY_QUEST_OBJ_VAR2_VALUE2) == ENTITY_QUEST_OBJ_VAR2_VALUE2)) &&
                                   questObjVar3 == ENTITY_QUEST_OBJ_VAR3_VALUE &&
                                   questObjVar4 != 0 &&
                                   questObjVar5 != 0;

            byte[] nameBytes = reader.ReadBytes(entityStructAddr + ENTITY_NAME_OFFSET, ENTITY_NAME_SIZE);
            String tmp = Encoding.UTF8.GetString(nameBytes);
            int nullIdx = tmp.IndexOf('\0');
            nullIdx = nullIdx == -1 ? ENTITY_NAME_SIZE : nullIdx;
            entity.Name = tmp.Substring(0, nullIdx);

            entity.Pos.X = BitConverter.ToSingle(entityStructBytes, ENTITY_POS_OFFSET + POS_X_OFFSET);
            entity.Pos.Y = BitConverter.ToSingle(entityStructBytes, ENTITY_POS_OFFSET + POS_Y_OFFSET);
            entity.Pos.Z = BitConverter.ToSingle(entityStructBytes, ENTITY_POS_OFFSET + POS_Z_OFFSET);
            entity.Pos.A = BitConverter.ToSingle(entityStructBytes, ENTITY_POS_OFFSET + POS_A_OFFSET);

            entity.StructPtr = entityStructAddr;
            return entity;
        }

        public static Entity GetMainCharEntity(ProcessReader reader)
        {
            long mainCharStructCPtr = reader.ReadInt64(MAIN_CHAR_STRUCTC_PTR_OFFSET, reader.ModuleBase);
            if (mainCharStructCPtr == 0)
            {
                return null;
            }

            return EntityFactory.GetEntity(reader, mainCharStructCPtr);
        }
    }
}
