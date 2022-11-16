using Microsoft.VisualBasic.FileIO;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using FMOD;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Collections;
using CSCore.Win32;
using ManagedBass;

namespace soundlib
{
    // for crossplatform code using
    internal class AudioEngine
    {
        public static void PlayAudioByteArray(FMOD.System mainSystem, FMOD.ChannelGroup channelGroup, FMOD.Channel mainChannel, byte[] fileByteArray, FMOD.DSP mainDSP) { }      // playing sound by reading bytes array using FMOD

        public static void PlayAudioByteArray(byte[] fileByteArray) { }            // playing sound by reading bytes

        public static void PlayAudioFilename(string fileName) { }                  // playing sound by playing wave file

        private static byte[] createTrimmingMerge(KeyValuePair<int, int> randomRange, int countOfBuffers = 2)
        {
            byte[] resultBuffer = null;

            try
            {
                for (int i = 0; i < countOfBuffers; ++i)
                {
                    string randomAsset = WaterPlayer.getRandomAssetFile();

                    byte[] buffer = Bytes.WaveFileUtils.CutGetByteArray(randomAsset, new Random().Next(randomRange.Key, randomRange.Value));

                    int startIndexOfDataChunk = Windows64.AudioConverter.getStartIndexOfDataChunk(buffer);

                    byte[] onlyHeaders = Windows64.AudioConverter.createForwardsArrayWithOnlyHeaders(buffer, startIndexOfDataChunk);

                    byte[] onlyAudioData = Windows64.AudioConverter.createForwardsArrayWithOnlyAudioData(buffer, startIndexOfDataChunk);

                    if (resultBuffer is null)
                    {
                        resultBuffer = Windows64.AudioConverter.combineArrays(onlyHeaders, onlyAudioData);
                    }

                    else
                    {
                        resultBuffer = Windows64.AudioConverter.combineArrays(resultBuffer, onlyAudioData);
                    }
                }
            }

            catch(Exception exception)
            {
                Except.generateException(exception);
            }

            return resultBuffer;
        }

        protected static PlayingStateEffectUtils ChooseRandomPLayingEffectUtil() => (PlayingStateEffectUtils)(new Random().Next(Convert.ToInt32(PlayingStateEffectUtils.FIRST) + 1, Convert.ToInt32(PlayingStateEffectUtils.END) - 1));      // getting random effect util

        protected static byte[] getFileByteArrayWithEffectUtilSetted(PlayingStateEffectUtils effectUtil, byte[] fileByteArray, int byteArrayindex = 0)          // set effect util to file byte[] array
        {
            byteArrayindex = Array.IndexOf(WaterPlayer.getBytesOfAssetsArray(), fileByteArray);
            switch (effectUtil)
            {
                case PlayingStateEffectUtils.DEFAULT:
                    return fileByteArray;
                case PlayingStateEffectUtils.REVERSE:
                    return WaterPlayer.getBytesOfReversedAssetsArray()[byteArrayindex];
                case PlayingStateEffectUtils.INVERSED:
                    return Bytes.ByteWrapper.inverseByteArray(fileByteArray);
                case PlayingStateEffectUtils.TRIMMED:
                    return createTrimmingMerge(new KeyValuePair<int, int>(1, 7), 5);
                default:
                    Environment.Exit(1);
                    break;
            }

            return null;
        }

        protected enum PlayingStateEffectUtils
        {
            FIRST = 0,
            DEFAULT,      // default playing byte[] sound array
            TRIMMED,          // trim byte[] sound array
            INVERSED,       // inverse byte[] sound array
            REVERSE,           // reverse byte[] sound array
            END
        }
    }

    namespace AudioEngineNamespaceFMOD
    {
        internal class AudioEngineFMOD : AudioEngine
        {
            // FMOD playing by byte array are not supported at most of usings [unstable]
            public static new void PlayAudioByteArray(FMOD.System mainSystem, FMOD.ChannelGroup channelGroup, FMOD.Channel mainChannel, byte[] fileByteArray, FMOD.DSP mainDSP)
            {
                FMOD.Sound sound = new FMOD.Sound();
                var info = new FMOD.CREATESOUNDEXINFO();

                mainSystem.createStream(fileByteArray, FMOD.MODE.CREATESTREAM, ref info, out sound);

                mainSystem.playSound(sound, channelGroup, false, out mainChannel);
                mainSystem.createDSPByType(Helper.FMODHelper.generateRandomDsp(3), out mainDSP);
                mainChannel.addDSP(0, mainDSP);
                System.Threading.Thread.Sleep(new Random().Next(5000, 8000));

                sound.release();
                mainChannel.removeDSP(mainDSP);
            }
        }
    }

    namespace Windows64
    {
        namespace AdditionalLibrariesSolutions
        {
            internal class AudioEngineAdditionalLibraries : AudioEngine
            {
                public static new void PlayAudioFilename(string fileName)
                {
                    try
                    {
                        if (!File.Exists(fileName)) throw new FileNotFoundException("Exception: file not exists");
                    }

                    catch (FileNotFoundException exception)
                    {
                        Except.generateException(exception);
                    }

                    try
                    {
                        using (var audioFile = new AudioFileReader(fileName))
                        using (var outputDevice = new WaveOutEvent())
                        {
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            while (outputDevice.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }

                    catch (Exception exception)
                    {
                        Except.generateException(exception);
                    }
                }
            }

            internal class AudioEngineWindows64 : AudioEngine
            {
                // playing byte[] array using windows extenssions in project [System.Windows.Extensions.dll]
                public new static void PlayAudioByteArray(byte[] fileByteArray)
                {
                    using(MemoryStream memoryStream = new MemoryStream(fileByteArray))
                    {
                        try
                        {
                            var soundPlayer = new System.Media.SoundPlayer(memoryStream);
                            soundPlayer.Play();

                            System.Threading.Thread.Sleep(new Random().Next(5000, 8000));
                        }

                        catch(PlatformNotSupportedException exception)
                        {
                            Except.generateException(exception);
                        }
                    }
                }
            }
        }
    }

    namespace MacOs
    {
        internal class AudioEngineMacOs : AudioEngine
        {
            public static new void PlayAudioByteArray(byte[] fileByteArray)      // stable using of bass.dll extensions
            {
                int localStream = 0;
                try
                {
                    if (ManagedBass.Bass.Init())
                    {
                        var randomEffectUtil = AudioEngine.ChooseRandomPLayingEffectUtil();
                        var bufferToPlay = AudioEngine.getFileByteArrayWithEffectUtilSetted(randomEffectUtil, fileByteArray);

                        localStream = ManagedBass.Bass.CreateStream(bufferToPlay, 0, bufferToPlay.Length, ManagedBass.BassFlags.Default);

                        if (localStream != 0)
                        {
                            ManagedBass.Bass.ChannelPlay(localStream);
                        }
                    }
                }

                catch(Exception exception)
                {
                    Except.generateException(exception);
                }

                finally
                {
                    System.Threading.Thread.Sleep(new Random().Next(2000, 8000));
                    ManagedBass.Bass.StreamFree(localStream);
                    ManagedBass.Bass.Free();
                }
            }
        }
    }

    internal class WaterPlayer
    {
        private FMOD.System mainSystem;

        private FMOD.Channel mainChannel;

        private FMOD.ChannelGroup channelGroup;

        private FMOD.DSP mainDSP;

        private int maxChannels;

        private FMOD.INITFLAGS initFlags;

        private IntPtr extradriverdata;

        private const string assetsDir = "assets/";        // path to dir with assets

        private Helper.OS.TypeOS typeOS = Helper.OS.SystemInfoGetter.getTypeOfOperationSystem();         // get operation system info (using for crossplatfrom)

        private static byte[][] bytesOfAssetsArray = new byte[getCountOfAssets() - 1][];            // bytes array for all assets without reversed (initial assets)

        private static byte[][] bytesOfReversedAssetsArray = new byte[getCountOfAssets() - 1][];       // bytes array only for reversed assets

        public static byte[][] getBytesOfAssetsArray() => bytesOfAssetsArray;

        public static byte[][] getBytesOfReversedAssetsArray() => bytesOfReversedAssetsArray;

        public string getAssetsDir() => assetsDir;

        public WaterPlayer(int maxChannels, FMOD.INITFLAGS initFlags, IntPtr extradriverdata)
        {
            //initialization of system configuration

            Helper.OS.OsHelper.initConfig(ref this.typeOS);      // typeOfOperatingSystem is using only for setting data, cos using ref argument

            // warning: use typeOS only like variable
            // else reference logic error

            Console.WriteLine(this.typeOS.ToString());

            try
            {
                var creatingResult = FMOD.Factory.System_Create(out mainSystem);
                Except.generateFMODexception(creatingResult);
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            finally
            {
                this.mainChannel = new FMOD.Channel();

                this.channelGroup = new FMOD.ChannelGroup();

                this.mainDSP = new FMOD.DSP();

                this.maxChannels = maxChannels;

                this.initFlags = initFlags;

                this.extradriverdata = extradriverdata;

                try
                {
                    var initresult = mainSystem.init(this.maxChannels, this.initFlags, this.extradriverdata);
                    Except.generateFMODexception(initresult);
                }

                catch (Exception exception)
                {
                    Except.generateException(exception);
                }

                finally
                {
                    this.mainChannel.setChannelGroup(this.channelGroup);
                    this.mainSystem.createChannelGroup(null, out channelGroup);

                    // only for window stable working
                    if (this.typeOS == Helper.OS.TypeOS.WINDOWS_SYSTEM)
                    {
                        this.fillBytesOfAssetsArray();           // fill default bytes array with assets
                        this.fillBytesOfReversedAssetsArray();   // fill reversed bytes array with reversed assets
                    }

                    else if(this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                    {
                        this.fillBytesOfAssetsArray();
                        this.fillBytesOfReversedAssetsArray();
                    }
                }
            }
        }

        ~WaterPlayer()
        {
            this.mainSystem.release();
            this.channelGroup.release();
            this.mainDSP.release();

            this.mainChannel.clearHandle();
            this.mainChannel.removeDSP(this.mainDSP);
            this.channelGroup.clearHandle();
        }

                                                                /* initialize bytesOfAssetsArray byte[][] */
                                                                /* calling in WaterPlayer() constructor */
        private void fillBytesOfAssetsArray()
        {
            string[] allfiles = Directory.GetFiles(assetsDir);

            try
            {
                if (allfiles is null) throw new Exception("Exception: dir is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            finally
            {
                if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    allfiles = Helper.MacOs.MacOsHelper.deleteDsStoreFileFromAssetDir(allfiles, assetsDir);
                }
            }

            for (int i = 0; i < allfiles.Length - 1; ++i)
            {
                if (allfiles[i] is null) continue;

                /* returned byte[] array of asset file */
                else if (this.typeOS == Helper.OS.TypeOS.WINDOWS_SYSTEM)         // windows64 solution
                {
                    WaterPlayer.bytesOfAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInByteArray(allfiles[i]);
                }

                else if(this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)          // mac_os solution
                {
                    if (allfiles[i] == assetsDir + ".DS_Store") continue;           // specially MAC OS desktop-settings file, which automatically creates [bin]
                    WaterPlayer.bytesOfAssetsArray[i] = soundlib.MacOs.AudioConverter.convertWaveFileInByteArray(allfiles[i]);
                }
            }
        }

        /* initialize bytesOfReversedAssetsArray byte[][] */
        /* calling in WaterPlayer() constructor */
        /* reversed byte array filled */
        private void fillBytesOfReversedAssetsArray()
        {
            string[] allfiles = Directory.GetFiles(assetsDir);

            try
            {
                if (allfiles is null) throw new Exception("Exception: dir is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            finally
            {
                if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    allfiles = Helper.MacOs.MacOsHelper.deleteDsStoreFileFromAssetDir(allfiles, assetsDir);
                }
            }

            for (int i = 0; i < allfiles.Length - 1; ++i)
            {
                /* returned byte[] array of reversed asset file */
                if (this.typeOS == Helper.OS.TypeOS.WINDOWS_SYSTEM)
                {
                    WaterPlayer.bytesOfReversedAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInReversedByteArray(allfiles[i]);
                }

                else if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    if (allfiles[i] == assetsDir + ".DS_Store") continue;           // specially MAC OS file, bin
                    WaterPlayer.bytesOfReversedAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInReversedByteArray(allfiles[i]);      // Windows64 method also stable works at mac os
                }
            }
            
        }

        /* get count of assets in assets directory */
        internal static int getCountOfAssets()
        {
            int count = 0;
            foreach (var filename in Directory.GetFiles(assetsDir))
            {
                if (filename == assetsDir + ".DS_Store") continue;
                ++count;
            }

            return count;
        }

        /* get random asset file from const assetDir */
        public static string getRandomAssetFile()
        {
            string[] allfiles = null;

            try
            {
                allfiles = Directory.GetFiles(assetsDir);
                if (allfiles is null) throw new Exception("Exception: dir is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            allfiles = Helper.MacOs.MacOsHelper.deleteDsStoreFileFromAssetDir(allfiles, assetsDir);

            return allfiles[new Random().Next(0, allfiles.Length)];
        }

        public void PlayMelodyByByteMemoryStream(byte[] fileByteArray)        // FMOD supporting
        {
            AudioEngineNamespaceFMOD.AudioEngineFMOD.PlayAudioByteArray(fileByteArray);
        }

        public void PlayMelodyByByteMemoryStreamUsingWindowsExtensions(byte[] fileByteArray)       // playing byte[] array using windows extenssions in project [System.Windows.Extensions.dll]
        {
            Windows64.AdditionalLibrariesSolutions.AudioEngineWindows64.PlayAudioByteArray(fileByteArray);
        }

        public void PlayMelodyByByteMemoryStreamUsingMacOsExtensions(byte[] fileByteArray)       // playing byte[] array using mac os extenssions in project
        {
            MacOs.AudioEngineMacOs.PlayAudioByteArray(fileByteArray);
        }

        public void set3DReverbationToSound(FMOD.VECTOR reverbationPosition, float minimalDist, float maximumDist, FMOD.VECTOR listenerPosition,
            FMOD.VECTOR velocityVector, FMOD.VECTOR forwardVector, FMOD.VECTOR upPositionVector)
        {
            FMOD.Reverb3D reverbation = new FMOD.Reverb3D();

            try
            {
                var resultReverbationSetting = this.mainSystem.createReverb3D(out reverbation);
                Except.generateFMODexception(resultReverbationSetting);
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            finally
            {
                reverbation.set3DAttributes(ref reverbationPosition, minimalDist, maximumDist);
                mainSystem.set3DListenerAttributes(0, ref listenerPosition, ref velocityVector, ref forwardVector, ref upPositionVector);
            }
        }

        private void clearDspControlsByIndex(FMOD.DSP localDsp)
        {
            this.mainChannel.removeDSP(localDsp);
        }

        /* generate infinity loop (different sound) */
        /* @playingMode - mode for playing (loop, deafult e.g.) */
        /* @dspType - type of effects to sounds (chorus, echo e.g.) */
        public void playAudioUsingFMOD(FMOD.MODE playingMode, FMOD.DSP_TYPE dspType)
        {
            FMOD.Sound sound = new FMOD.Sound();
            var randomAsset = getRandomAssetFile();

            try
            {
                this.mainSystem.createStream(randomAsset, playingMode, out sound);
                this.mainSystem.playSound(sound, channelGroup, false, out mainChannel);
                this.mainSystem.createDSPByType(dspType, out mainDSP);
                this.mainChannel.addDSP(0, mainDSP);
            }

            catch(Exception exception)
            {
                Except.generateException(exception);
            }

            System.Threading.Thread.Sleep(new Random().Next(5000, 8000));

            sound.release();
            clearDspControlsByIndex(mainDSP);
        }

        public void playAudioUsingNetStreamFMOD(string assetUrl, FMOD.MODE playingMode, FMOD.DSP_TYPE dspType)
        {
            FMOD.Sound sound = new FMOD.Sound();

            try
            {
                this.mainSystem.createStream(assetUrl, playingMode, out sound);
                this.mainSystem.playSound(sound, channelGroup, false, out mainChannel);
                this.mainSystem.createDSPByType(dspType, out mainDSP);
                this.mainChannel.addDSP(0, mainDSP);
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            System.Threading.Thread.Sleep(new Random().Next(5000, 8000));

            sound.release();
            clearDspControlsByIndex(mainDSP);
        }

        /* bad net streaming by using FMOD core engine 
         so i decided to using nAduio */
        public void playAudioNetStreamUsingNAudio(string url)
        {
            try
            {
                using (var mediaFoundation = new MediaFoundationReader(url))
                {
                    using (var wasApiOut = new WasapiOut())
                    {
                        wasApiOut.Init(mediaFoundation);
                        wasApiOut.Play();

                        while (wasApiOut.PlaybackState == NAudio.Wave.PlaybackState.Playing) System.Threading.Thread.Sleep(1000);
                    }
                }
            }

            catch(Exception exception)
            {
                Except.generateException(exception);
            }
        }
    }
}