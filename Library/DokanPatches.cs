using RouteDokan.Library.FileFormats;
using SARCExt;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;

namespace RouteDokan.Library {
    public class DokanPatches {
        public class ObjectData {
            public static void ToSwitch(ref byte[] unpatched, string switchromfs, string relativePath) {
                SarcData wiiURomFS = SZS.Open(unpatched);
                SarcData switchRomFS = SZS.Open(Path.Join(switchromfs, relativePath));

                // Only ports selected files.
                BulkSARCFilesPorting(
                    wiiURomFS,
                    ref switchRomFS,
                    new Dictionary<string, Type> {
                        { "InitPartsFixInfo*", typeof(FileFormats.BYAML) },
                        { "InitClipping.byml", typeof(FileFormats.BYAML) },
                        { "InitSubActor.byml", typeof(FileFormats.BYAML) },
                        { "InitShadowMask.byml", typeof(FileFormats.BYAML) },
                        { "InitModel.byml", typeof(FileFormats.BYAML) },
                        { "InitEffect.byml", typeof(FileFormats.BYAML) }
                    });

                // Ports bfres and kcl.
                foreach(string file in wiiURomFS.Files.Keys)
                    if(file.EndsWith(".bfres"))
                        PortAndAddFileToSARC(wiiURomFS, ref switchRomFS, file, typeof(BFRES));
                    else if(file.EndsWith(".kcl"))
                        PortAndAddFileToSARC(wiiURomFS, ref switchRomFS, file, typeof(KCL));

                unpatched = SZS.Save(switchRomFS);
            }
        }

        public class StageData {
            public static void JoinStages(string stagespath, string output) {
                List<string> stageNames = new();

                // Counts all the stages (multiple files per stage) that exist in the folder.
                foreach(FileInfo file in new DirectoryInfo(stagespath).GetFiles()) {
                    string name = RemoveStringEnd(file.Name);
                    if(!stageNames.Contains(name)) stageNames.Add(name);
                }

                foreach(string name in stageNames) {
                    SarcData newSarc = new() { endianness = ByteOrder.LittleEndian, Files = new() };

                    CommonStageJoining(newSarc, name, "Design1.szs");
                    CommonStageJoining(newSarc, name, "Map1.szs");
                    CommonStageJoining(newSarc, name, "Sound1.szs");

                    SZS.Save(newSarc, Path.Join(output, name + ".szs"));
                }

                string RemoveStringEnd(string raw) {
                    if(raw.EndsWith("Map1.szs"))
                        return raw.Remove(raw.Length - 8);
                    if(raw.EndsWith("Design1.szs"))
                        return raw.Remove(raw.Length - 11);
                    if(raw.EndsWith("Sound1.szs"))
                        return raw.Remove(raw.Length - 10);
                    return raw;
                }

                void CommonStageJoining(SarcData sarc, string name, string type) {
                    if(File.Exists(Path.Join(stagespath, name + type)))
                        foreach(KeyValuePair<string, byte[]> file in SZS.Open(Path.Join(stagespath, name + type)).Files)
                            sarc.Files.Add(file.Key, file.Value);
                }
            }

            public static void SplitStages(string stagespath, string output) {
                foreach(FileInfo stageFile in new DirectoryInfo(stagespath).GetFiles()) {
                    SarcData design = new() { endianness = ByteOrder.BigEndian, Files = new() };
                    SarcData map = new() { endianness = ByteOrder.BigEndian, Files = new() };
                    SarcData sound = new() { endianness = ByteOrder.BigEndian, Files = new() };

                    string name = stageFile.Name.Remove(stageFile.Name.Length - 4);

                    foreach(KeyValuePair<string, byte[]> file in SZS.Open(stageFile.FullName).Files) {
                        switch(file.Key) {
                            case "CameraParam.byml":
                            case string x when x == name + "Map.byml":
                                map.Files.Add(file.Key, file.Value);
                                break;
                            case string x when x == name + "Sound.byml":
                                sound.Files.Add(file.Key, file.Value);
                                break;
                            default:
                                design.Files.Add(file.Key, file.Value);
                                break;
                        }
                    }

                    // File saving
                    if(design.Files.Count > 0)
                        SZS.Save(design, Path.Join(output, name + "Design1.szs"));
                    if(map.Files.Count > 0)
                        SZS.Save(map, Path.Join(output, name + "Map1.szs"));
                    if(sound.Files.Count > 0)
                        SZS.Save(sound, Path.Join(output, name + "Sound1.szs"));
                }
            }
        }

        public static void PortByType(ref byte[] origin, Type type) {
            switch(type.Name) {
                case nameof(AAMP):
                    origin = AAMP.ChangeVersion(origin);
                    break;
                case nameof(BFRES):
                    origin = BFRES.ChangePlatform(origin);
                    break;
                case nameof(FileFormats.BYAML):
                    origin = FileFormats.BYAML.ChangePlatform(origin);
                    break;
                case nameof(KCL):
                    origin = KCL.ChangePlatform(origin);
                    break;
            }
        }

        protected static void BulkSARCFilesPorting(SarcData origin, ref SarcData destination, Dictionary<string, Type> filenames) {
            // Loops through all specified files in the argument.
            foreach(KeyValuePair<string, Type> file in filenames) {
                // If the key ends with *, it means that all the files starting with that string should be ported.
                if(file.Key.EndsWith('*')) {
                    // Loops through all the files contained inside the szs until it finds one that matches the query.
                    foreach(string originFile in origin.Files.Keys)
                        if(originFile.StartsWith(file.Key.Remove(file.Key.Length - 1)))
                            PortAndAddFileToSARC(origin, ref destination, originFile, file.Value);
                } else if(origin.Files.ContainsKey(file.Key)) // Port all the files that do not contain *.
                    PortAndAddFileToSARC(origin, ref destination, file.Key, file.Value);
            }
        }

        protected static void PortAndAddFileToSARC(SarcData origin, ref SarcData destination, string name, Type type) {
            byte[] fileToPort = origin.Files[name];

            PortByType(ref fileToPort, type);
            if(destination.Files.ContainsKey(name))
                destination.Files[name] = fileToPort;
            else destination.Files.Add(name, fileToPort);
        }
    }
}
