using System;
using System.Collections.Generic;

namespace ImbaXIV
{
    class ImbaXIVCore
    {
        private ProcessReader reader;
        private GameData gameData;

        public bool IsAttached = false;
        public String[] Targets;
        public String TargetInfo;
        public LinkedList<Entity> QuestEntities;
        public Entity MainCharEntity;

        public ImbaXIVCore()
        {
            reader = new ProcessReader();
            gameData = new GameData();
            Targets = new string[0];
            QuestEntities = new LinkedList<Entity>();
            MainCharEntity = new Entity();
        }

        public bool AttachProcess()
        {
            if (!reader.AttachProcess())
                return false;
            IsAttached = true;
            gameData.Update(reader);
            return true;
        }

        public void UpdateMainChar()
        {
            Entity entity = EntityFactory.GetMainCharEntity(reader, gameData);
            if (entity == null)
            {
                MainCharEntity.Zeroise();
                return;
            }
            MainCharEntity = entity;
        }

        public bool Update()
        {
            if (!reader.CheckAlive())
            {
                IsAttached = false;
                MainCharEntity.Zeroise();
                return false;
            }

            UpdateMainChar();

            TargetInfo = "";
            QuestEntities = new LinkedList<Entity>();
            LinkedList<Entity> allEntities = EntityFactory.GetEntities(reader, gameData);

            foreach (var entity in allEntities)
            {
                // Check for quest entities
                if (entity.Type == EntityType.EventNpc || entity.Type == EntityType.BattleNpc)
                {
                    if (entity.QuestType != FloatingPlateType.UNKNOWN && entity.Name.Length > 0)
                    {
                        QuestEntities.AddLast(entity);
                    }
                }
                else if (entity.Type == EntityType.EventObject)
                {
                    // It is possible for the name to be empty - e.g. the purple circle surround enemy
                    if (entity.IsQuestObject)
                    {
                        QuestEntities.AddLast(entity);
                    }
                }

                foreach (var target in Targets)
                {
                    if (!entity.Name.Contains(target) && !target.Equals("All!!"))
                        continue;
                    TargetInfo += $"{entity.Name}: {entity.Pos.X,4:N1} {entity.Pos.Y,4:N1} {entity.Pos.Z,4:N1}\n";
                    for (int i = 0; i < 56; ++i)
                    {
                        long readAddr = entity.StructPtr + i * 8;
                        long val = reader.ReadInt64(readAddr);
                        TargetInfo += $"{readAddr:X16}\t0x{val:X16}\n";
                    }
                    TargetInfo += "\n";
                }
            }

            return true;
        }
    }
}