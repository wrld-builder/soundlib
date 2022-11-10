using FMOD;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace soundlib
{
    namespace Windows64
    {
        static class MetadataGatherer
        {
            internal static ushort GetTypeOfFormat(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 20;
                int endIndex = 21;
                byte[] typeOfFormatByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, startIndex, endIndex);
                ushort typeOfFormat = BitConverter.ToUInt16(typeOfFormatByteArray, 0);
                return typeOfFormat;
            }

            internal static void GetFmtText(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 12;
                int endIndex = 15;
                GetAsciiText(forwardsWavFileStreamByteArray, startIndex, endIndex);
            }

            internal static string GetWaveText(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 8;
                int endIndex = 11;
                return GetAsciiText(forwardsWavFileStreamByteArray, startIndex, endIndex);
            }

            internal static string GetRiffText(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 0;
                int endIndex = 3;
                return GetAsciiText(forwardsWavFileStreamByteArray, startIndex, endIndex);
            }

            internal static uint GetLengthOfFormatData(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 16;
                int endIndex = 19;
                byte[] lengthOfFormatDataByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, startIndex, endIndex);
                uint lengthOfFormatData = BitConverter.ToUInt32(lengthOfFormatDataByteArray, 0);
                return lengthOfFormatData;
            }

            internal static byte[] GetRelevantBytesIntoNewArray(byte[] forwardsWavFileStreamByteArray, int startIndex, int endIndex)
            {
                int length = endIndex - startIndex + 1;
                byte[] relevantBytesArray = new byte[length];
                Array.Copy(forwardsWavFileStreamByteArray, startIndex, relevantBytesArray, 0, length);
                return relevantBytesArray;
            }

            internal static uint GetFileSize(byte[] forwardsWavFileStreamByteArray)
            {
                int fileSizeStartIndex = 4;
                int fileSizeEndIndex = 7;
                byte[] fileSizeByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, fileSizeStartIndex, fileSizeEndIndex);
                uint fileSize = BitConverter.ToUInt32(fileSizeByteArray, 0) + 8; //need to add the size of the 
                return fileSize;
            }

            internal static string GetAsciiText(byte[] forwardsWavFileStreamByteArray, int startIndex, int endIndex)
            {
                string asciiText = "";
                for (int i = startIndex; i <= endIndex; i++)
                {
                    asciiText += Convert.ToChar(forwardsWavFileStreamByteArray[i]);
                }
                return asciiText;
            }

            internal static ushort GetNumOfChannels(byte[] forwardsWavFileStreamByteArray)
            {
                int numOfChannelsStartIndex = 22;
                int numOfChannelsEndIndex = 23;
                byte[] numOfChannelsByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, numOfChannelsStartIndex, numOfChannelsEndIndex);
                ushort numOfChannels = BitConverter.ToUInt16(numOfChannelsByteArray, 0); //need to add the size of the 
                return numOfChannels;
            }

            internal static uint GetSampleRate(byte[] forwardsWavFileStreamByteArray)
            {
                int sampleRateStartIndex = 24;
                int sampleRateEndIndex = 27;
                byte[] sampleRateByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, sampleRateStartIndex, sampleRateEndIndex);
                uint sampleRate = BitConverter.ToUInt32(sampleRateByteArray, 0); //need to add the size of the 
                return sampleRate;
            }

            internal static uint GetBytesPerSecond(byte[] forwardsWavFileStreamByteArray)
            {
                int bytesPerSecondStartIndex = 28;
                int bytesPerSecondEndIndex = 31;
                byte[] bytesPerSecondByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, bytesPerSecondStartIndex, bytesPerSecondEndIndex);
                uint bytesPerSecond = BitConverter.ToUInt32(bytesPerSecondByteArray, 0); //need to add the size of the 
                return bytesPerSecond;
            }

            internal static ushort GetBlockAlign(byte[] forwardsWavFileStreamByteArray)
            {
                int blockAlignStartIndex = 32;
                int blockAlignEndIndex = 33;
                byte[] blockAlignByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, blockAlignStartIndex, blockAlignEndIndex);
                ushort blockAlign = BitConverter.ToUInt16(blockAlignByteArray, 0); //need to add the size of the 
                return blockAlign;
            }

            internal static ushort GetBitsPerSample(byte[] forwardsWavFileStreamByteArray)
            {
                int bitsPerSampleStartIndex = 34;
                int bitsPerSampleEndIndex = 35;
                byte[] bitsPerSampleByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, bitsPerSampleStartIndex, bitsPerSampleEndIndex);
                ushort bitsPerSample = BitConverter.ToUInt16(bitsPerSampleByteArray, 0); //need to add the size of the 
                return bitsPerSample;
            }

            internal static void GetDataText(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 70;
                int endIndex = 73;
                GetAsciiText(forwardsWavFileStreamByteArray, startIndex, endIndex);
            }

            internal static void GetListText(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndex = 36;
                int endIndex = 39;
                GetAsciiText(forwardsWavFileStreamByteArray, startIndex, endIndex);
            }

            internal static uint GetDataSize(byte[] forwardsWavFileStreamByteArray)
            {
                int dataSizeStartIndex = 70;
                int dataSizeEndIndex = 73;
                byte[] dataSizeByteArray = GetRelevantBytesIntoNewArray(forwardsWavFileStreamByteArray, dataSizeStartIndex, dataSizeEndIndex);
                uint dataSize = BitConverter.ToUInt16(dataSizeByteArray, 0); //need to add the size of the 
                return dataSize;
            }
        }

        internal class AudioConverter
        {
            const int _bitsPerByte = 8;
            static int _bytesPerSample;

            public static int getBytesPerSample() => _bytesPerSample;

            public static void getWavMetadata(byte[] forwardsWavFileStreamByteArray)
            {
                MetadataGatherer.GetRiffText(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetFileSize(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetWaveText(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetFmtText(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetLengthOfFormatData(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetTypeOfFormat(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetNumOfChannels(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetSampleRate(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetBytesPerSecond(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetBlockAlign(forwardsWavFileStreamByteArray);
                _bytesPerSample = MetadataGatherer.GetBitsPerSample(forwardsWavFileStreamByteArray) / _bitsPerByte;
                MetadataGatherer.GetListText(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetDataText(forwardsWavFileStreamByteArray);
                MetadataGatherer.GetDataSize(forwardsWavFileStreamByteArray);
            }

            private static void writeReversedWavFileByteArrayToFile(byte[] reversedWavFileStreamByteArray, string reversedWavFilePath)
            {
                FileStream reversedFileStream = new FileStream(reversedWavFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                reversedFileStream.Write(reversedWavFileStreamByteArray, 0, reversedWavFileStreamByteArray.Length);
                reversedFileStream.Close();
            }

            public static byte[] populateReversedWavFileByteArray(byte[] forwardsWavFileStreamByteArray, int startIndexOfDataChunk, int bytesPerSample)
            {
                byte[] forwardsArrayWithOnlyHeaders = createForwardsArrayWithOnlyHeaders(forwardsWavFileStreamByteArray, startIndexOfDataChunk);

                byte[] forwardsArrayWithOnlyAudioData = createForwardsArrayWithOnlyAudioData(forwardsWavFileStreamByteArray, startIndexOfDataChunk);

                byte[] reversedArrayWithOnlyAudioData = reverseTheForwardsArrayWithOnlyAudioData(bytesPerSample, forwardsArrayWithOnlyAudioData);

                byte[] reversedWavFileStreamByteArray = combineArrays(forwardsArrayWithOnlyHeaders, reversedArrayWithOnlyAudioData);

                return reversedWavFileStreamByteArray;
            }

            public static byte[] combineArrays(byte[] forwardsArrayWithOnlyHeaders, byte[] reversedArrayWithOnlyAudioData)
            {
                byte[] reversedWavFileStreamByteArray = new byte[forwardsArrayWithOnlyHeaders.Length + reversedArrayWithOnlyAudioData.Length];
                Array.Copy(forwardsArrayWithOnlyHeaders, reversedWavFileStreamByteArray, forwardsArrayWithOnlyHeaders.Length);
                Array.Copy(reversedArrayWithOnlyAudioData, 0, reversedWavFileStreamByteArray, forwardsArrayWithOnlyHeaders.Length, reversedArrayWithOnlyAudioData.Length);
                return reversedWavFileStreamByteArray;
            }

            /* reverse the samples in WAVE file */
            private static byte[] reverseTheForwardsArrayWithOnlyAudioData(int bytesPerSample, byte[] forwardsArrayWithOnlyAudioData)
            {
                int length = forwardsArrayWithOnlyAudioData.Length;
                byte[] reversedArrayWithOnlyAudioData = new byte[length];

                int sampleIdentifier = 0;

                /* If we’re at an odd index, then we need to go one index forwards in our reverse for loop to get the last byte from the sample. */
                for (int i = 0; i < length; i++)
                {
                    /* we are in even or odd index of element */
                    if (i != 0 && i % bytesPerSample == 0)
                    {
                        sampleIdentifier += 2 * bytesPerSample;
                    }
                    int index = length - bytesPerSample - sampleIdentifier + i;
                    reversedArrayWithOnlyAudioData[i] = forwardsArrayWithOnlyAudioData[index];
                }
                return reversedArrayWithOnlyAudioData;
            }

            public static byte[] createForwardsArrayWithOnlyAudioData(byte[] forwardsWavFileStreamByteArray, int startIndexOfDataChunk)
            {
                byte[] forwardsArrayWithOnlyAudioData = new byte[forwardsWavFileStreamByteArray.Length - startIndexOfDataChunk];
                Array.Copy(forwardsWavFileStreamByteArray, startIndexOfDataChunk, forwardsArrayWithOnlyAudioData, 0, forwardsWavFileStreamByteArray.Length - startIndexOfDataChunk);
                return forwardsArrayWithOnlyAudioData;
            }

            /* create array by static constants in static internal class  */
            /* constants replaced. startIndexOfDataChunk in args = {} */
            public static byte[] createForwardsArrayWithOnlyHeaders(byte[] forwardsWavFileStreamByteArray, int startIndexOfDataChunk)
            {
                byte[] forwardsArrayWithOnlyHeaders = new byte[startIndexOfDataChunk];
                Array.Copy(forwardsWavFileStreamByteArray, 0, forwardsArrayWithOnlyHeaders, 0, startIndexOfDataChunk);
                return forwardsArrayWithOnlyHeaders;
            }

            /* convert forward wave file in byte[] array */
            public static byte[] populateForwardsWavFileByteArray(string forwardsWavFilePath)
            {
                FileStream forwardsWavFileStream = new FileStream(forwardsWavFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                byte[] forwardsWavFileStreamByteArray = new byte[forwardsWavFileStream.Length];
                forwardsWavFileStream.Read(forwardsWavFileStreamByteArray, 0, (int)forwardsWavFileStream.Length);
                forwardsWavFileStream.Close();
                return forwardsWavFileStreamByteArray;
            }

            public static int getStartIndexOfDataChunk(byte[] forwardsWavFileStreamByteArray)
            {
                int startIndexOfAudioData = 12;
                int charDAsciiDecimalCode = 100; //'d' //data is located at index 70 in my .wav file
                int charAAsciiDecimalCode = 97;  //'a'
                int charTAsciiDecimalCode = 116; //'t'

                int chunkSize;
                while (!(forwardsWavFileStreamByteArray[startIndexOfAudioData] == charDAsciiDecimalCode && forwardsWavFileStreamByteArray[startIndexOfAudioData + 1] == charAAsciiDecimalCode && forwardsWavFileStreamByteArray[startIndexOfAudioData + 2] == charTAsciiDecimalCode && forwardsWavFileStreamByteArray[startIndexOfAudioData + 3] == charAAsciiDecimalCode))
                {
                    startIndexOfAudioData += 4;
                    chunkSize = forwardsWavFileStreamByteArray[startIndexOfAudioData] + forwardsWavFileStreamByteArray[startIndexOfAudioData + 1] * 256 + forwardsWavFileStreamByteArray[startIndexOfAudioData + 2] * 65536 + forwardsWavFileStreamByteArray[startIndexOfAudioData + 3] * 16777216;
                    startIndexOfAudioData += 4 + chunkSize;
                }
                startIndexOfAudioData += 8;
                return startIndexOfAudioData;
            }

            /* get newly-created reversed WAVE file */
            /* @pathToAudio - path to forwarded WAVE file */
            /* @newtitle - a new title for reversed file (without const assetDir) */
            public static void convertWaveFileToReversedWaveFile(string pathToAudio, string newtitle)
            {
                byte[] forwardsWavFileStreamByteArray = populateForwardsWavFileByteArray(pathToAudio);

                getWavMetadata(forwardsWavFileStreamByteArray);

                int startIndexOfDataChunk = getStartIndexOfDataChunk(forwardsWavFileStreamByteArray);

                byte[] reversedWavFileStreamByteArray = populateReversedWavFileByteArray(forwardsWavFileStreamByteArray, startIndexOfDataChunk, _bytesPerSample);

                string reversedWavFilePath = newtitle;
                writeReversedWavFileByteArrayToFile(reversedWavFileStreamByteArray, reversedWavFilePath);
            }

            /* returned byte[] array of reversed file */
            public static byte[] convertWaveFileInReversedByteArray(string pathToAudio)
            {
                byte[] forwardsWavFileStreamByteArray = null;
                int startIndexOfDataChunk = 0;

                try
                {
                    if (!File.Exists(pathToAudio)) { throw new Exception("Exception: path not exists"); }

                    forwardsWavFileStreamByteArray = populateForwardsWavFileByteArray(pathToAudio);

                    getWavMetadata(forwardsWavFileStreamByteArray);

                    startIndexOfDataChunk = getStartIndexOfDataChunk(forwardsWavFileStreamByteArray);
                }

                // maybe exception:
                // Index was outside the bounds of the array
                catch (Exception exception)
                {
                    Except.generateException(exception);
                }

                return populateReversedWavFileByteArray(forwardsWavFileStreamByteArray, startIndexOfDataChunk, _bytesPerSample);
            }

            /* returned byte[] array of default wave file */
            public static byte[] convertWaveFileInByteArray(string pathToAudio)
            {
                return populateForwardsWavFileByteArray(pathToAudio);
            }
        }
    }

    namespace MacOs
    {
        internal class AudioConverter : Windows64.AudioConverter
        {
            public static new byte[] convertWaveFileInByteArray(string pathToAudio)
            {
                byte[] buffer = null;
                try
                {
                    if (!File.Exists(pathToAudio)) throw new FileNotFoundException("Exception: file not exists");
                }

                catch (FileNotFoundException exception)
                {
                    Except.generateException(exception);
                }

                try
                {
                    using (WaveFileReader reader = new WaveFileReader(pathToAudio))       // works only with 16-bit format audio
                    {
                        buffer = new byte[reader.Length];
                        int read = reader.Read(buffer, 0, buffer.Length);
                        short[] sampleBuffer = new short[read / 2];
                        Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
                    }
                }

                catch(Exception exception)
                {
                    Except.generateException(exception);
                }

                return buffer;
            }
        }
    }
}