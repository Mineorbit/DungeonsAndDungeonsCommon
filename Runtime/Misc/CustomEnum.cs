namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class CustomEnum
    {
        public int cardinal;


        public string Value;

        public CustomEnum(string val, int card = 0)
        {
            Value = val;
            cardinal = card;
        }

        public CustomEnum(int card)
        {
            Value = "";
            cardinal = card;
        }

        public int Cardinal()
        {
            return cardinal;
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(CustomEnum a, CustomEnum b)
        {
            return a.cardinal == b.cardinal;
        }

        public static bool operator !=(CustomEnum a, CustomEnum b)
        {
            return a.cardinal != b.cardinal;
        }
    }
}