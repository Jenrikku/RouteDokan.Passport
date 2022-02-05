using AampLibraryCSharp;
using System.IO;

namespace RouteDokan.Library.FileFormats {
    public class AAMP {
        public static byte[] ChangeVersion(byte[] bytes) {
            AampFile aamp;

            using(MemoryStream stream = new(bytes)) {
                aamp = AampFile.LoadFile(stream);
            }

            if(bytes[4] == 1) aamp.ConvertToVersion2();
            else aamp.ConvertToVersion1();

            using(MemoryStream stream = new()) {
                aamp.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
