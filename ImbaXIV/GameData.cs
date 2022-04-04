using System;

namespace ImbaXIV
{
    class GameData
    {
        private const string EntityManagerPtrFindPattern = "48 83 3d ?? ?? ?? 01 00 74 1c e8 ?? ?? ?? ?? 48 8d 88 ?? 03 00 00 e8";
        public int EntityManagerPtrOffset = 0x1ebdf70;
        public int EntityManagerEntityContainerListOffset = 0x1a0;
        public int EntityContainerSize = 0x30;
        public int EntityContainerContainer1Offset = 0x0;
        public int EntityContainerContainer2Offset = 0x8;
        public int EntityContainerContainer3Offset = 0x10;
        public int EntityContainerTypeOffset = 0x18;
        public int EntityContainerTypeInvalidValue = 0x101;
        public int EntityContainerUnkStructOffset = 0x28;
        public int UnkStructEntityOffset = 0x8;
        
        public int GenericEntityStructSize = 0x1c0;

        public int EntityNameOffset = 0x30;
        public int EntityNameSize = 0x40;

        public int EntityKindOffset = 0x8c;

        public int EntityPosOffset = 0xa0;
        public int PosXOffset = 0x0;
        public int PosYOffset = 0x8;
        public int PosZOffset = 0x4;
        public int PosAOffset = 0x10;

        public int EntityFloatingPlateOffset = 0x100;

        public int EntityQuestObjVar1Offset = 0x70;
        public int EntityQuestObjVar2Offset = 0x94;
        public int EntityQuestObjVar3Offset = 0xc0;
        public int EntityQuestObjVar4Offset = 0x114;
        public int EntityQuestObjVar5Offset = 0x124;
        public int EntityQuestObjVar1Value = 7;
        public int EntityQuestObjVar2Value1 = 0xd1bf;
        public int EntityQuestObjVar2Value2 = 0xd1bc;
        public int EntityQuestObjVar3Value = 0x3f000000;

        private const string MainCharEntityFindPattern = "84 c9 74 15 80 3d ?? ?? ?? ?? 00 74 0c 48 8b 05 ?? ?? ?? ?? 48 85 c0 75 07 48 8b 05";
        public int MainCharEntityPtrOffset = 0x01e7ccf0;

        public void Update(ProcessReader reader)
        {
            int offset = Utils.BytePatternMatch(reader.ModuleMemory, EntityManagerPtrFindPattern);
            int nextPcAddress = offset + 8;
            int ptrPcOffset = BitConverter.ToInt32(reader.ModuleMemory, offset + 3);
            EntityManagerPtrOffset = nextPcAddress + ptrPcOffset;

            offset = Utils.BytePatternMatch(reader.ModuleMemory, MainCharEntityFindPattern);
            nextPcAddress = offset + 0x20;
            ptrPcOffset = BitConverter.ToInt32(reader.ModuleMemory, offset + 0x1c);
            MainCharEntityPtrOffset = nextPcAddress + ptrPcOffset;
        }
    }
}
