using System.Collections.Generic;

namespace CSFileParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "p")
            {
                DocParser.ParseDoc(args[1]);
            }
            if (args[0] == "w")
            {
                DocWriter writer = new DocWriter(args[1], args[2]);
                writer.ReWrite();
            }
        }
    }
}
