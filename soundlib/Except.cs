using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace soundlib
{
    internal class Except
    {
        public static void generateException(Exception exception)
        {
            System.Console.Error.WriteLine(exception.Message.ToString());
            System.Environment.Exit(-1);
        }

        public static void generateFMODexception(FMOD.RESULT obj)
        {
            if (obj != FMOD.RESULT.OK) throw new Exception("FMOD exception: " + obj.ToString());
        }
    }
}