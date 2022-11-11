using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bytes;
using FMOD;

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

            var playingByteStreamLoop = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    wp.PlayMelodyByByteMemoryStreamUsingMacOsExtensions(wp.getBytesOfAssetsArray()[new Random().Next(0, WaterPlayer.getCountOfAssets() - 1)]);
                }
            });

            playingLoopThread.Start();
            playingByteStreamLoop.Start();

            playingLoopThread.Join();
            playingByteStreamLoop.Join();
        }
    }
}