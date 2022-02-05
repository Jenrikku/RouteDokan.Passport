using RouteDokan.Library.FileFormats;
using SARCExt;
using System;
using System.Collections.Generic;
using System.IO;

using static RouteDokan.Library.DokanPatches;

namespace RouteDokan.Library {
    public class DokanHandler {
        /// <returns>Whether or not the SZS was ported successfully (as if all known filetypes inside were ported).</returns>
        public static bool ApplyPatchesSZS(ref byte[] unpatched, string relativePath, DataType type, string wiiuromfs, string switchromfs, bool isWiiU) {
            // -- Wii U --
            if(isWiiU) {
                // Patches only get applied if romfs is specified and the file exists in the switch version of the game as well.
                if(switchromfs != "" && File.Exists(Path.Join(switchromfs, relativePath))) {
                    // Patches by types:
                    switch(type) {
                        case DataType.ObjectData:
                            ObjectData.ToSwitch(ref unpatched, switchromfs, relativePath);
                            return true; // The program does not continue any further as the file has already been ported.
                        case DataType.StageData:
                            // WIP: Change microphone platforms by looping through BYAML.
                            // [!] Use SZS.SaveNoCompression(SarcData) for patches.
                            break;
                        case DataType.MessageData:
                            // Codename: Too trippy.
                            break;
                    }
                }

                // Porting that does not require romfs:
                Tuple<SarcData, bool> result = CommonPatch(unpatched, isWiiU);

                // Stages are saved without compression since the need to be joined or separated later.
                if(type == DataType.StageData)
                    unpatched = SZS.SaveNoCompression(result.Item1);
                else
                    unpatched = SZS.Save(result.Item1);
                return true;

                // -- Switch --
            } else {
                // No current patches exist for porting from Switch to Wii U.

                // Neither ObjectData nor CubeMapTextureData are compatible.
                if(type == DataType.ObjectData || type == DataType.CubeMapTextureData)
                    return false;

                // Porting that does not require romfs:
                Tuple<SarcData, bool> result = CommonPatch(unpatched, isWiiU);

                unpatched = SZS.Save(result.Item1);
                return result.Item2;
            }
        }

        protected static Tuple<SarcData, bool> CommonPatch(byte[] unpatched, bool isWiiU) {
            SarcData sarc = SZS.Open(unpatched);
            bool complete = true;
            foreach(KeyValuePair<string, byte[]> file in sarc.Files) {
                byte[] ported = file.Value;

                switch(file.Key.ToLowerInvariant()) {
                    case string x when x.EndsWith(".aamp"):
                        PortByType(ref ported, typeof(AAMP));
                        break;
                    case string x when x.EndsWith(".bfres"):
                        if(isWiiU)
                            PortByType(ref ported, typeof(BFRES));
                        else
                            complete = false; // WIP
                        break;
                    case string x when x.EndsWith(".byml"):
                        PortByType(ref ported, typeof(FileFormats.BYAML));
                        break;
                    case string x when x.EndsWith(".kcl"):
                        PortByType(ref ported, typeof(KCL));
                        break;
                }

                sarc.Files[file.Key] = ported;
            }

            return new Tuple<SarcData, bool>(sarc, complete);
        }

        public enum DataType {
            Unknown,
            CubeMapTextureData,
            EffectData,
            LayoutData,
            ObjectData,
            ShaderData,
            SoundData,
            StageData,
            SystemData,
            MessageData,
            FontData
        }
    }
}
