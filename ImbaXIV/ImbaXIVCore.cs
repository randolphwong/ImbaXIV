using System;
using System.Collections.Generic;
using System.Text;

namespace ImbaXIV
{
    public class ImbaXIVCore
    {
        private readonly ProcessReader _reader = new ProcessReader();
        private readonly GameData _gameData = new GameData();

        public bool IsAttached = false;
        public String[] Targets;
        private StringBuilder _targetInfoSb = new StringBuilder();
        public String TargetInfo { get { return _targetInfoSb.ToString(); } }
        public LinkedList<Entity> QuestEntities = new LinkedList<Entity>();
        public Entity MainCharEntity;
        public string ManualTargetName { get; set; }
        private LinkedList<Entity> _manualTargetEntity = new LinkedList<Entity>();
        public LinkedList<Entity> ManualTargetEntity { get { return _manualTargetEntity; } }

        public ImbaXIVCore()
        {
            Targets = new string[0];
            MainCharEntity = new Entity();
        }

        public bool AttachProcess()
        {
            if (!_reader.AttachProcess())
                return false;
            IsAttached = true;
            _gameData.Update(_reader);
            return true;
        }

        public void UpdateMainChar()
        {
            Entity entity = EntityFactory.GetMainCharEntity(_reader, _gameData);
            if (entity == null)
            {
                MainCharEntity.Zeroise();
                return;
            }
            MainCharEntity = entity;
        }

        public bool Update()
        {
            if (!_reader.CheckAlive())
            {
                IsAttached = false;
                MainCharEntity.Zeroise();
                return false;
            }

            UpdateMainChar();

            _targetInfoSb.Clear();
            QuestEntities.Clear();
            _manualTargetEntity.Clear();
            LinkedList<Entity> allEntities = EntityFactory.GetEntities(_reader, _gameData);

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

                if (ManualTargetName != null && ManualTargetName.Length > 0)
                {
                    if (entity.Name.Contains(ManualTargetName) && entity.IsVisible)
                        _manualTargetEntity.AddLast(entity);
                }

                foreach (var target in Targets)
                {
                    if (!entity.Name.Contains(target) && !target.Equals("All!!"))
                        continue;
                    _targetInfoSb.Append($"{entity.Name}: {entity.Pos.X,4:N1} {entity.Pos.Y,4:N1} {entity.Pos.Z,4:N1}\n");
                    for (int i = 0; i < 56; ++i)
                    {
                        long readAddr = entity.StructPtr + i * 8;
                        long val = _reader.ReadInt64(readAddr);
                        _targetInfoSb.Append($"{readAddr:X16}\t0x{val:X16}\n");
                    }
                    _targetInfoSb.Append("\n");
                }
            }

            return true;
        }
    }
}