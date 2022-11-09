using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bytes;

namespace soundlib
{
    internal class MainClass
    {
        public static void Main()
        {
            var wp = new WaterPlayer(512, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

            var playingLoopThread = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    wp.playAudioUsingFMOD(FMOD.MODE.CREATESTREAM | FMOD.MODE.CREATESAMPLE, Helper.FMODHelper.generateRandomDsp(1));
                }
            });

            playingLoopThread.Start();
            playingLoopThread.Join();
        }
    }
}