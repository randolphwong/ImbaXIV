using System;

namespace ImbaXIV
{
    class GameData
    {
        private const string EntityManagerPtrFindPattern = "48 83 3d ?? ?? ?? 01 00 74 1c e8 ?? ?? ?? ?? 48 8d 88 ?? 03 00 00 e8";
        public int EntityManagerPtrOffset = 0x1ebdf70;
        public const int EntityManagerEntityContainerListOffset = 0x1a0;
        public const int EntityContainerSize = 0x30;
        public const int EntityContainerContainer1Offset = 0x0;
        public const int EntityContainerContainer2Offset = 0x8;
        public const int EntityContainerContainer3Offset = 0x10;
        public const int EntityContainerTypeOffset = 0x18;
        public const int EntityContainerTypeInvalidValue = 0x101;
        public const int EntityContainerUnkStructOffset = 0x28;
        public const int UnkStructEntityOffset = 0x8;
        
        public const int GenericEntityStructSize = 0x1c0;

        public const int EntityNameOffset = 0x30;
        public const int EntityNameSize = 0x40;

        public const int EntityKindOffset = 0x8c;

        public const int EntityPosOffset = 0xb0;
        public const int PosXOffset = 0x0;
        public const int PosYOffset = 0x8;
        public const int PosZOffset = 0x4;
        public const int PosAOffset = 0x10;

        public const int EntityFloatingPlateOffset = 0x110;

        public const int EntityVisibilityOffset = 0x95;
        public const int EntityVisibilityQuestValue1 = 0x68ff;
        public const int EntityVisibilityQuestValue2 = 0xd1bc;
        public const int EntityVisibilityIsVisible = 0xff;

        public const int EntityQuestObjVar1Offset = 0x70;
        public const int EntityQuestObjVar3Offset = 0xd0;
        public const int EntityQuestObjVar1Value = 7;
        public const int EntityQuestObjVar3Value = 0x3f000000;

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
