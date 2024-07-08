using System;

namespace ImbaXIV
{
    class GameData
    {
        // Based on 7.0
        private const string EntityManagerPtrFindPattern = "48 89 05 ?? ?? ?? ?? 48 83 C4 28 E9 ?? ?? ?? 00 48 89 05 ?? ?? ?? ?? 48 83 C4 28 E9";
        public int EntityManagerPtrOffset = 0x027686a0;
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

        public const int EntityVisibilityOffset = 0x94;
        public const int EntityVisibilityQuestValue1 = 0x68ff;
        public const int EntityVisibilityQuestValue2 = 0xd1fb;
        public const int AetherCurrentVisibilityValue = 0x51fb; // becomes 0x415b when not visible
        public const int EntityVisibilityIsVisible = 0xff;

        public const int EntityQuestObjVar1Offset = 0x70;
        public const int EntityQuestObjVar3Offset = 0xd0;
        public const int EntityQuestObjVar1Value = 7;
        public const int EntityQuestObjVar3Value = 0x3f000000;

        private const string MainCharEntityFindPattern = "C7 05 ?? ?? ?? ?? 00 00 00 E0 48 89 ?? ?? ?? ?? ?? 88 ?? ?? ?? ?? 02 88 ?? ?? ?? ?? 02 48 83 C4";
        public int MainCharEntityPtrOffset = 0x0273e5e0;

        public void Update(ProcessReader reader)
        {
            int offset = Utils.BytePatternMatch(reader.ModuleMemory, EntityManagerPtrFindPattern);
            int nextPcAddress = offset + 7;
            int ptrPcOffset = BitConverter.ToInt32(reader.ModuleMemory, offset + 3);
            EntityManagerPtrOffset = nextPcAddress + ptrPcOffset;

            offset = Utils.BytePatternMatch(reader.ModuleMemory, MainCharEntityFindPattern);
            nextPcAddress = offset + 17;
            ptrPcOffset = BitConverter.ToInt32(reader.ModuleMemory, offset + 13);
            MainCharEntityPtrOffset = nextPcAddress + ptrPcOffset;
        }
    }
}
