using RouteDokan.Library;
using RouteDokan.Library.FileFormats;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;

using static RouteDokan.Library.DokanHandler;
using static RouteDokan.Passport.DokanConfiguration;

namespace RouteDokan.Passport {
    delegate void ProgressEvent(ProgressInfo info, string header);

    /// <summary>
    /// <see cref="DokanProcessor"/> is the class containing the methods that handle porting.
    /// </summary>
    class DokanProcessor {
        /// <returns>True if the platform is Wii U (BigEndian), false otherwise.
        /// Null means that the program did not find any szs files.</returns>
        public static bool? IdentificatePlatform(string modpath) {
            FileInfo file = GetExampleFile(modpath);

            if(file == null) return null;
            else return SZS.Open(File.ReadAllBytes(file.FullName)).endianness == ByteOrder.BigEndian;
        }

        public static event ProgressEvent CurrentProgressChanged;
        public static void StartWork(string modPath, string output, bool isWiiU) {
            foreach(Tuple<FileInfo, DataType> item in HuntDownFiles(modPath)) {
                // Invokes the event so the program can display the file ported at the moment.
                CurrentProgressChanged?.Invoke(new ProgressInfo() {
                        Name = item.Item1.Name,
                        Type = item.Item2
                    },
                    "Porting file: ");

                // Porting for szs files (common)
                if(item.Item1.Extension.ToLowerInvariant() == ".szs") {
                    // Reads the SZS.
                    byte[] unpatched = File.ReadAllBytes(item.Item1.FullName);

                    ApplyPatchesSZS(
                        ref unpatched,
                        Path.GetRelativePath(modPath, item.Item1.FullName),
                        item.Item2,
                        GetValue(AvailableKeys[0]),
                        GetValue(AvailableKeys[1]),
                        isWiiU);

                    // StageData
                    if(item.Item2 == DataType.StageData) {
                        Directory.CreateDirectory(Path.Join(output, "StageData", "temp"));
                        File.WriteAllBytes(Path.Join(output, "StageData", "temp", item.Item1.Name), unpatched);

                        continue; // Stages do not need further processing.
                    }

                    // Saves the ported SZS.
                    Directory.CreateDirectory(Path.Join(output, Path.GetRelativePath(modPath, item.Item1.DirectoryName)));
                    File.WriteAllBytes(
                        Path.Join(output, Path.GetRelativePath(modPath, item.Item1.FullName)), unpatched);
                }

                // To-Do: Porting other file types (sound)
            }

            // Executed if any stage needs to be merged / splitted.
            if(Directory.Exists(Path.Join(output, "StageData", "temp"))) {
                CurrentProgressChanged?.Invoke(null, "Processing stages...");

                if(isWiiU)
                    DokanPatches.StageData.JoinStages(Path.Join(output, "StageData", "temp"), Path.Join(output, "StageData"));
                else
                    DokanPatches.StageData.SplitStages(Path.Join(output, "StageData", "temp"), Path.Join(output, "StageData"));

                Directory.Delete(Path.Join(output, "StageData", "temp"), true);
            }
        }

        // Gets all the files inside the specified directory, searching through all subdirectories.
        protected static Tuple<FileInfo, DataType>[] HuntDownFiles(string parentPath) {
            List<Tuple<FileInfo, DataType>> list = new();

            foreach(FileSystemInfo fileSystemInfo in new DirectoryInfo(parentPath).GetFileSystemInfos())
                if(fileSystemInfo is FileInfo file) {
                    string notParsedType = file.Directory.Name;
                    if(notParsedType == "stream" || notParsedType == "streamSe") notParsedType = "SoundData";
                    else if(file.Name == "FontData.szs") notParsedType = "FontData";

                    list.Add(
                        new Tuple<FileInfo, DataType>(file, Enum.Parse<DataType>(notParsedType)));
                }
                else if(fileSystemInfo is DirectoryInfo)
                    list.AddRange(HuntDownFiles(fileSystemInfo.FullName));

            return list.ToArray();
        }

        // Returns the first file it finds inside the directory (.szs only). Null is returned if no file exists.
        protected static FileInfo GetExampleFile(string dir) {
            foreach(FileSystemInfo fileSystemInfo in new DirectoryInfo(dir).GetFileSystemInfos())
                if(fileSystemInfo is FileInfo file) {
                    if(file.Extension.ToLowerInvariant() == ".szs")
                        return file;
                } else {
                    FileInfo fileInfo = GetExampleFile(fileSystemInfo.FullName);
                    if(fileInfo != null) return fileInfo;
                }
            return null;
        }
    }

    class ProgressInfo {
        /// <summary>
        /// The name of the file being ported in the format: "<see cref="DataType"/>/<see cref="Name"/>"
        /// </summary>
        public string FullName { get { return Type.ToString() + "/" + Name; } }

        public string Name { get; internal set; }
        public DataType Type { get; internal set; } = default;
    }
}
