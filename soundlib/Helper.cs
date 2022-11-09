using System;
using System.Text.Json;
using FMOD;

namespace Helper
{
    internal static class FMODHelper
    {
        // generating random DSP effects (using already preparing effects)
        // TO DO: generating random DSP systems, to generationg more powerful and beautiful sound
        public static FMOD.DSP_TYPE generateRandomDsp(int dspCount)
        {
            FMOD.DSP_TYPE[] dspTypesArray = new FMOD.DSP_TYPE[dspCount];
            for (int i = 0; i < dspTypesArray.Length; ++i)
            {
                dspTypesArray[i] = (FMOD.DSP_TYPE)new Random().Next(0, 37);
            }

            FMOD.DSP_TYPE result = new FMOD.DSP_TYPE();
            foreach (var dsp in dspTypesArray)
            {
                result |= dsp;
            }

            return result;
        }
    }

    namespace OS
    {
        // enum class of OS types, using for crossplatform solutions (most powerful solution)
        enum TypeOS
        {
            MAC_OS_SYSTEM,
            WINDOWS_SYSTEM,
            LINUX
        }

        internal class OsHelper
        {
            public static string pathToConfigFile = "config.json";          // path to config file of program

            public string compilation_os { get; }

            public string name { get; }

            // constructor for inline using with fileStream and json serializer
            public OsHelper(string compilation_os, string name)
            {
                this.compilation_os = compilation_os;
                this.name = name;
            }

            // to validate config file.
            // without config.json file starting of program is unavailable
            // ref using, beacuse argument setter using here
            public static void initConfig(ref TypeOS typeOfOperationSystem)
            {
                try
                {
                    if (!File.Exists(pathToConfigFile)) throw new FileNotFoundException("Exception: config file not exists");

                    using (FileStream filestream = new FileStream(pathToConfigFile, FileMode.Open))
                    {
                        try
                        {
                            var configuration = JsonSerializer.Deserialize<OsHelper>(filestream);
                            if (configuration?.name is null || configuration?.name.Length == 0) throw new Exception("Exception: name of project is empty");

                            else
                            {
                                try
                                {
                                    switch (configuration?.compilation_os)
                                    {
                                        case "mac_os":
                                            typeOfOperationSystem = TypeOS.MAC_OS_SYSTEM;
                                            break;
                                        case "win64":
                                            typeOfOperationSystem = TypeOS.WINDOWS_SYSTEM;
                                            break;
                                        case "linux":
                                            typeOfOperationSystem = TypeOS.LINUX;
                                            break;
                                        default:
                                            throw new Exception("Exception: your OS is not supported");
                                    }
                                }

                                catch (Exception exception)
                                {
                                    soundlib.Except.generateException(exception);
                                }
                            }
                        }

                        catch (Exception exception)
                        {
                            soundlib.Except.generateException(exception);
                        }
                    }
                }

                catch(FileNotFoundException exception)
                {
                    soundlib.Except.generateException(exception);
                }
            }
        }

        internal static class SystemInfoGetter
        {
            private static bool validateOperationSystemtype(string typeOfOperationSystemNameString)
            {
                switch (typeOfOperationSystemNameString)
                {
                    case "mac_os":
                        return true;
                    case "win64":
                        return true;
                    case "linux":
                        return true;
                    default:
                        return false;
                }
            }

            private static TypeOS typeOfOperationSystem;

            public static TypeOS getTypeOfOperationSystem() => typeOfOperationSystem;

            public static string getSystemTypeFromJsonConfig()
            {
                OsHelper? configuration = null;
                try
                {
                    using (FileStream filestream = new FileStream(OsHelper.pathToConfigFile, FileMode.Open))
                    {
                        try
                        {
                            configuration = JsonSerializer.Deserialize<OsHelper>(filestream);
                        }

                        catch(Exception exception)
                        {
                            soundlib.Except.generateException(exception);
                        }
                    }
                }

                catch (FileNotFoundException exception)
                {
                    soundlib.Except.generateException(exception);
                }

                if(!validateOperationSystemtype(configuration?.compilation_os))
                {
                    System.Console.WriteLine("Not supported operation syste type. Exiting with code 1");
                    System.Environment.Exit(1);
                }

                return configuration?.compilation_os;
            }
        }
    }          // namespace OS
}
