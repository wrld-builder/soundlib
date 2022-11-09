using NAudio.Wave;
using soundlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using soundlib.Windows64;

namespace Bytes
{
    static internal class ByteWrapper
    {
        /* byte array inverse */
        /* using in low-level generation of sound */
        public static byte[] inverseByteArray(byte[] waveFileByteArray)
        {
            int startIndexOfDataChunk = AudioConverter.getStartIndexOfDataChunk(waveFileByteArray);
            byte[] audiodata = AudioConverter.createForwardsArrayWithOnlyAudioData(waveFileByteArray, startIndexOfDataChunk);

            byte[] result = null;
            try
            {
                for (int i = 0; i < audiodata.Length; ++i)
                {
                    audiodata[i] = (byte)~waveFileByteArray[i];
                }

                result = AudioConverter.combineArrays(AudioConverter.createForwardsArrayWithOnlyHeaders(waveFileByteArray, startIndexOfDataChunk), audiodata);
                if (audiodata is null) throw new Exception("Exception: byte array is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            return result;
        }

        /* slide byte array to right */
        public static byte[] slideRightByteArray(byte[] waveFileByteArray, int slideCoefficient)
        {
            int startIndexOfDataChunk = AudioConverter.getStartIndexOfDataChunk(waveFileByteArray);
            byte[] audiodata = AudioConverter.createForwardsArrayWithOnlyAudioData(waveFileByteArray, startIndexOfDataChunk);

            byte[] result = null;
            try
            {
                for (int i = 0; i < audiodata.Length; ++i)
                {
                    audiodata[i] = (byte)(waveFileByteArray[i] >> slideCoefficient);
                }

                result = AudioConverter.combineArrays(AudioConverter.createForwardsArrayWithOnlyHeaders(waveFileByteArray, startIndexOfDataChunk), audiodata);
                if (audiodata is null) throw new Exception("Exception: byte array is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            return result;
        }

        /* slide byte array to left */
        public static byte[] slideLeftByteArray(byte[] waveFileByteArray, int slideCoefficient)
        {
            int startIndexOfDataChunk = AudioConverter.getStartIndexOfDataChunk(waveFileByteArray);
            byte[] audiodata = AudioConverter.createForwardsArrayWithOnlyAudioData(waveFileByteArray, startIndexOfDataChunk);

            byte[] result = null;
            try
            {
                for (int i = 0; i < audiodata.Length; ++i)
                {
                    audiodata[i] = (byte)(waveFileByteArray[i] << slideCoefficient);
                }

                result = AudioConverter.combineArrays(AudioConverter.createForwardsArrayWithOnlyHeaders(waveFileByteArray, startIndexOfDataChunk), audiodata);
                if (audiodata is null) throw new Exception("Exception: byte array is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            return result;
        }

        /* slide byte array to random way (right or left) in sliding range */
        public static byte[] slideInRandomWayByteArray(byte[] waveFileByteArray, KeyValuePair<int, int> slidingRange)
        {
            int startIndexOfDataChunk = AudioConverter.getStartIndexOfDataChunk(waveFileByteArray);
            byte[] audiodata = AudioConverter.createForwardsArrayWithOnlyAudioData(waveFileByteArray, startIndexOfDataChunk);

            byte[] result = null;
            try
            {
                for (int i = 0; i < audiodata.Length; ++i)
                {
                    bool randomWay = Convert.ToBoolean(new Random().Next(2));

                    if (randomWay)
                    {
                        audiodata[i] = (byte)(waveFileByteArray[i] << new Random().Next(slidingRange.Key, slidingRange.Value));
                    }

                    else
                    {
                        audiodata[i] = (byte)(waveFileByteArray[i] >> new Random().Next(slidingRange.Key, slidingRange.Value));
                    }
                }

                result = AudioConverter.combineArrays(AudioConverter.createForwardsArrayWithOnlyHeaders(waveFileByteArray, startIndexOfDataChunk), audiodata);
                if (audiodata is null) throw new Exception("Exception: byte array is empty");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            return result;
        }
    }

    static internal class WaveFileUtils
    {
        public static string Cut(string filename, int secondsCutting)
        {
            FileInfo fi = new FileInfo(filename);
            var outputPath = System.IO.Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));

            TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(secondsCutting));
            return outputPath;
        }

        public static byte[] CutGetByteArray(string filename, int secondsCutting)
        {
            byte[] newByteArray = new byte[1024];
            string outputPath = Cut(filename, secondsCutting);

            try
            {
                newByteArray = AudioConverter.convertWaveFileInByteArray(outputPath);
                if (newByteArray is null) throw new Exception("Exception: byte array is null");
            }

            catch (Exception exception)
            {
                Except.generateException(exception);
            }

            finally
            {
                File.Delete(outputPath);
            }

            return newByteArray;
        }

        private static void TrimWavFile(string inPath, string outPath, TimeSpan duration)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    float bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000f;

                    int startPos = 0;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)Math.Round(duration.TotalMilliseconds * bytesPerMillisecond);
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    int endPos = endBytes;

                    TrimWavFile(reader, writer, startPos, endBytes);
                }
            }
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[reader.BlockAlign * 1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}