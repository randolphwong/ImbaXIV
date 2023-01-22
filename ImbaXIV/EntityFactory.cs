using System;
using System.Collections.Generic;
using System.Text;

namespace ImbaXIV
{
    class EntityFactory
    {
        public static LinkedList<Entity> GetEntities(ProcessReader reader, GameData gameData)
        {
            LinkedList<Entity> entities = new LinkedList<Entity>();
            long entityManagerPtr = reader.ReadInt64(gameData.EntityManagerPtrOffset, reader.ModuleBase);
            if (entityManagerPtr == 0)
            {
                return entities;
            }

            long entityListPtr = reader.ReadInt64(entityManagerPtr + GameData.EntityManagerEntityContainerListOffset);
            HashSet<long> seenEntities = new HashSet<long>();
            Queue<long> entityQueue = new Queue<long>();
            entityQueue.Enqueue(entityListPtr);
            while (entityQueue.Count > 0)
            {
                long curEntityContainer = entityQueue.Dequeue();
                seenEntities.Add(curEntityContainer);
                byte[] curEntityContainerBytes = reader.ReadBytes(curEntityContainer, GameData.EntityContainerSize);
                long first = BitConverter.ToInt64(curEntityContainerBytes, GameData.EntityContainerContainer1Offset);
                long second = BitConverter.ToInt64(curEntityContainerBytes, GameData.EntityContainerContainer2Offset);
                long third = BitConverter.ToInt64(curEntityContainerBytes, GameData.EntityContainerContainer3Offset);
                long unkStructPtr = BitConverter.ToInt64(curEntityContainerBytes, GameData.EntityContainerUnkStructOffset);
                long containerType = BitConverter.ToInt64(curEntityContainerBytes, GameData.EntityContainerTypeOffset);
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
                if (containerType == GameData.EntityContainerTypeInvalidValue)
                    continue;
                if (unkStructPtr == 0)
                    continue;
                long entityStructPtr = reader.ReadInt64(unkStructPtr + GameData.UnkStructEntityOffset);
                if (entityStructPtr == 0)
                    continue;

                Entity entity = EntityFactory.GetEntity(reader, gameData, entityStructPtr);
                entities.AddLast(entity);
            }
            return entities;
        }

        public static Entity GetEntity(ProcessReader reader, GameData gameData, long entityStructAddr)
        {
            Entity entity = new Entity();
            byte[] entityStructBytes = reader.ReadBytes(entityStructAddr, GameData.GenericEntityStructSize);

            int kind = entityStructBytes[GameData.EntityKindOffset];
            entity.Type = Enum.IsDefined(typeof(EntityType), kind) ? (EntityType)kind : EntityType.Unknown;

            FloatingPlateType questType = (FloatingPlateType)BitConverter.ToInt32(entityStructBytes, GameData.EntityFloatingPlateOffset);
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

            int questObjVar1 = entityStructBytes[GameData.EntityQuestObjVar1Offset];
            int entityVisibility = BitConverter.ToInt16(entityStructBytes, GameData.EntityVisibilityOffset) & 0xffff;
            int questObjVar3 = BitConverter.ToInt32(entityStructBytes, GameData.EntityQuestObjVar3Offset);
            entity.IsQuestObject = questObjVar1 == GameData.EntityQuestObjVar1Value &&
                                   (entityVisibility == GameData.EntityVisibilityQuestValue1 ||
                                    entityVisibility == GameData.EntityVisibilityQuestValue2) &&
                                   questObjVar3 == GameData.EntityQuestObjVar3Value;
            entity.IsVisible = (entityVisibility & 0xff) == GameData.EntityVisibilityIsVisible;

            byte[] nameBytes = reader.ReadBytes(entityStructAddr + GameData.EntityNameOffset, GameData.EntityNameSize);
            String tmp = Encoding.UTF8.GetString(nameBytes);
            int nullIdx = tmp.IndexOf('\0');
            nullIdx = nullIdx == -1 ? GameData.EntityNameSize : nullIdx;
            entity.Name = tmp.Substring(0, nullIdx);

            entity.Pos.X = BitConverter.ToSingle(entityStructBytes, GameData.EntityPosOffset + GameData.PosXOffset);
            entity.Pos.Y = BitConverter.ToSingle(entityStructBytes, GameData.EntityPosOffset + GameData.PosYOffset);
            entity.Pos.Z = BitConverter.ToSingle(entityStructBytes, GameData.EntityPosOffset + GameData.PosZOffset);
            entity.Pos.A = BitConverter.ToSingle(entityStructBytes, GameData.EntityPosOffset + GameData.PosAOffset);

            entity.StructPtr = entityStructAddr;
            return entity;
        }

        public static Entity GetMainCharEntity(ProcessReader reader, GameData gameData)
        {
            long mainCharStructCPtr = reader.ReadInt64(gameData.MainCharEntityPtrOffset, reader.ModuleBase);
            if (mainCharStructCPtr == 0)
            {
                return null;
            }

            return EntityFactory.GetEntity(reader, gameData, mainCharStructCPtr);
        }
    }
}
