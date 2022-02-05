using KclLibrary;
using Syroot.BinaryData;
using System.IO;

namespace RouteDokan.Library.FileFormats {
    public class KCL {
        public static byte[] ChangePlatform(byte[] bytes) {
            KCLFile kcl;

            using(MemoryStream stream = new(bytes)) {
                kcl = new(stream);
            }

            if(kcl.ByteOrder == ByteOrder.BigEndian)
                kcl.ByteOrder = ByteOrder.LittleEndian;
            else
                kcl.ByteOrder = ByteOrder.BigEndian;

            using(MemoryStream stream = new()) {
                kcl.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
