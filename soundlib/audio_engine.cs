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

namespace soundlib
{
    // for crossplatform code using
    internal class AudioEngine
    {
        public static void PlayAudioByteArray(FMOD.System mainSystem, FMOD.ChannelGroup channelGroup, FMOD.Channel mainChannel, byte[] fileByteArray, FMOD.DSP mainDSP) { }      // playing sound by reading bytes array using FMOD

        public static void PlayAudioByteArray(byte[] fileByteArray) { }            // playing sound by reading bytes

        public static void PlayAudioFilename(string fileName) { }                  // playing sound by playing wave file
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
                            while (outputDevice.PlaybackState == PlaybackState.Playing)
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
            public new static void PlayAudioByteArray(byte[] fileByteArray)
            {
                // mac os stable version?
                int localStream = 0;
                try
                {
                    if (ManagedBass.Bass.Init())
                    {
                        localStream = ManagedBass.Bass.CreateStream(fileByteArray, 0, fileByteArray.Length, ManagedBass.BassFlags.Default);

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
                    System.Threading.Thread.Sleep(new Random().Next(5000, 8000));
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

        private byte[][] bytesOfAssetsArray = new byte[getCountOfAssets() - 1][];            // bytes array for all assets without reversed (initial assets)

        private byte[][] bytesOfReversedAssetsArray = new byte[getCountOfAssets() - 1][];       // bytes array only for reversed assets

        public byte[][] getBytesOfAssetsArray() => this.bytesOfAssetsArray;

        public byte[][] getBytesOfReversedAssetsArray() => this.bytesOfReversedAssetsArray;

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

            finally
            {
                if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    for(int i = 0; i < allfiles.Length; ++i)
                    {
                        if (allfiles[i] == assetsDir + ".DS_Store")
                        {
                            var buffer = new List<string>(allfiles);
                            buffer.RemoveAt(i);
                            allfiles = buffer.ToArray();
                        }
                    }
                }
            }

            for (int i = 0; i < allfiles.Length - 1; ++i)
            {
                if (allfiles[i] is null) continue;

                /* returned byte[] array of asset file */
                else if (this.typeOS == Helper.OS.TypeOS.WINDOWS_SYSTEM)         // windows64 solution
                {
                    this.bytesOfAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInByteArray(allfiles[i]);
                }

                else if(this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)          // mac_os solution
                {
                    if (allfiles[i] == assetsDir + ".DS_Store") continue;           // specially MAC OS desktop-settings file, which automatically creates [bin]
                    this.bytesOfAssetsArray[i] = soundlib.MacOs.AudioConverter.convertWaveFileInByteArray(allfiles[i]);
                }
            }
        }

        /* initialize bytesOfReversedAssetsArray byte[][] */
        /* calling in WaterPlayer() constructor */
        /* reversed byte array filled */
        private void fillBytesOfReversedAssetsArray()
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

            finally
            {
                if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    for (int i = 0; i < allfiles.Length; ++i)
                    {
                        if (allfiles[i] == assetsDir + ".DS_Store")
                        {
                            var buffer = new List<string>(allfiles);
                            buffer.RemoveAt(i);
                            allfiles = buffer.ToArray();
                        }
                    }
                }
            }

            for (int i = 0; i < allfiles.Length - 1; ++i)
            {
                /* returned byte[] array of reversed asset file */
                if (this.typeOS == Helper.OS.TypeOS.WINDOWS_SYSTEM)
                {
                    this.bytesOfReversedAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInReversedByteArray(allfiles[i]);
                }

                else if (this.typeOS == Helper.OS.TypeOS.MAC_OS_SYSTEM)
                {
                    if (allfiles[i] == assetsDir + ".DS_Store") continue;           // specially MAC OS file, bin
                    this.bytesOfReversedAssetsArray[i] = soundlib.Windows64.AudioConverter.convertWaveFileInReversedByteArray(allfiles[i]);      // Windows64 method also stable works at mac os
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
        protected string getRandomAssetFile()
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

                        while (wasApiOut.PlaybackState == PlaybackState.Playing) System.Threading.Thread.Sleep(1000);
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