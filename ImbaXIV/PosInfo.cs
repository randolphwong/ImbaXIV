namespace ImbaXIV
{
    public class PosInfo
    {
        public float X;
        public float Y;
        public float Z;
        public float A;

        public PosInfo()
        {
            Zeroise();
        }

        public PosInfo(float x, float y, float z, float a)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
        }

        public void Zeroise()
        {
            X = 0;
            Y = 0;
            Z = 0;
            A = 0;
        }
    }
}
