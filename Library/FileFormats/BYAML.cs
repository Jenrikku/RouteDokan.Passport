using BYAML;
using Syroot.BinaryData;
using System.IO;
using System.Text;

namespace RouteDokan.Library.FileFormats {
    public class BYAML {
        public static byte[] ChangePlatform(byte[] bytes) {
            BymlFileData byaml;
            ByteOrder endianess = GetEndianess(bytes);

            using(MemoryStream stream = new(bytes)) {
                byaml = ByamlFile.LoadN(stream, true, endianess);
            }

            if(endianess == ByteOrder.BigEndian) {
                byaml.Version = 2;
                byaml.byteOrder = ByteOrder.LittleEndian;
            } else {
                byaml.Version = 1;
                byaml.byteOrder = ByteOrder.BigEndian;
            }

            byaml.SupportPaths = false;

            return ByamlFile.SaveN(byaml);

            ByteOrder GetEndianess(byte[] bytes) {
                return Encoding.ASCII.GetString(bytes[0..2]) switch {
                    "BY" => ByteOrder.BigEndian,
                    "YB" => ByteOrder.LittleEndian,
                    _ => default
                };
            }
        }
    }
}
