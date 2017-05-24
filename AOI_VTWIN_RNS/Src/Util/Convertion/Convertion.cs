namespace CollectorPackage.Src.Util.Convertion
{
    class Convertion
    {
        public static bool stringToBool(string str)
        {
            return System.Convert.ToBoolean(System.Convert.ToInt32(str));
        }
    }
}
