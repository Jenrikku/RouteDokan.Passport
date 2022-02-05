using EveryFileExplorer;
using SARCExt;
using System;
using System.IO;
using System.Text;

namespace RouteDokan.Library.FileFormats {
    public class SZS {
        public static SarcData Open(string filepath) {
            return Open(File.ReadAllBytes(filepath));
        }

        public static SarcData Open(byte[] file) {
            if(Encoding.ASCII.GetString(file[0..4]).ToUpperInvariant() == "YAZ0")
                file = YAZ0.Decompress(file);

            return SARC.UnpackRamN(file);
        }

        public static void Save(SarcData sarc, string filepath) {
            File.WriteAllBytes(filepath, Save(sarc));
        }

        public static byte[] Save(SarcData sarc) {
            Tuple<int, byte[]> packed = SARC.PackN(sarc);

            return YAZ0.Compress(packed.Item2, 3, (uint) packed.Item1);
        }

        public static byte[] SaveNoCompression(SarcData sarc) {
            return SARC.PackN(sarc).Item2;
        }
    }
}
