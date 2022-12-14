using System.Text;

namespace SignalMQ.Core.Utilities
{
    public static class Randomizer
    {
        private const string RandomSeed = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly static Random Rnd = new Random((int)DateTime.Now.Ticks);

        public static string Generate(int length = 12, bool alphabetOnly = false)
        {
            string generatedCode;
            do
            {
                generatedCode = "";
                var chars = alphabetOnly ? RandomSeed[..^10] : RandomSeed;
                var sb = new StringBuilder();
                for (int i = 0; i < length; i++)
                    if (i == 0)
                        sb.Append(chars[..^10][Rnd.Next(0, chars[..^10].Length)]);
                    else
                        sb.Append(chars[Rnd.Next(0, chars.Length)]);

                generatedCode = sb.ToString();
            }
            while (!EnsureNoDuplicate(generatedCode) && !string.IsNullOrWhiteSpace(generatedCode));

            return generatedCode;
        }

        private static bool EnsureNoDuplicate(string code)
        {
            return true;
        }
    }
}