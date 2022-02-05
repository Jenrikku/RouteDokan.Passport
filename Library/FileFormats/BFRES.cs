using BfresLibrary;
using BfresLibrary.PlatformConverters;
using Syroot.BinaryData;
using System;
using System.IO;

namespace RouteDokan.Library.FileFormats {
    public class BFRES {
        public static byte[] ChangePlatform(byte[] bytes) {
            ResFile bfres;

            using(MemoryStream stream = new(bytes)) {
                bfres = new(stream);
            }

            if(bfres.ByteOrder == ByteOrder.BigEndian) {
                bfres.ChangePlatform(true, 4096, 0, 9, 0, 0, ConverterHandle.SM3DW);
                bfres.Alignment = 0x0C;
            } else
                throw new NotImplementedException("Model porting from Switch to Wii U is currently not possible.");

            using(MemoryStream stream = new()) {
                bfres.Save(stream, true);
                return stream.ToArray();
            }
        }
    }
}
