using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogIsland_TEX_Unpacker
{
    public class DataRead
    {
        public unsafe static int ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                if (Program.LittleEndian)
                {
                    return *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
                }

                return (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
            }
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)ToInt32(value, startIndex);
        }
    }
}
