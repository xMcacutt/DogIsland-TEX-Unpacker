using System.Text;

namespace DogIsland_TEX_Unpacker
{
    public class Program
    {
        static string _path;
        static bool _run = true;
        public static bool LittleEndian = false;

        public static void Main()
        {
            while (_run)
            {
                Console.Clear();
                Console.WriteLine("TEX Extractor for The DOG Island");

                string input = "";
                while (!string.Equals(input, "y", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(input, "n", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Would you like to extract ALL .tex files?   Y / N");
                    input = Console.ReadLine();
                }
                if (string.Equals(input, "y", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Please enter path to root directory");
                    _path = Console.ReadLine().Replace("\"", "");
                    while (!Directory.Exists(_path))
                    {
                        Console.WriteLine("Path was invalid");
                        Console.WriteLine("Please enter path to root directory");
                        _path = Console.ReadLine().Replace("\"", " ");
                    }
                    Console.WriteLine("Extracting...");
                    ExtractAll(_path);
                    Console.WriteLine("Extract completed successfully");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Please enter path to tex");
                    _path = Console.ReadLine().Replace("\"", "");
                    while (!File.Exists(_path) || !_path.EndsWith(".tex"))
                    {
                        Console.WriteLine("Path was invalid");
                        Console.WriteLine("Please enter path to tex");
                        _path = Console.ReadLine().Replace("\"", " ");
                    }
                    Console.WriteLine("Extracting...");
                    Extract(_path);
                    Console.WriteLine("Extract completed successfully");
                    Console.ReadLine();
                }
            }
        }

        public static void Extract(string path)
        {
            string texName = Path.GetFileNameWithoutExtension(path);
            string inDirPath = Path.GetDirectoryName(path);
            string outDirPath = Path.Combine(inDirPath, texName + "tex");
            if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

            var f = File.OpenRead(path);
            f.Seek(0xC, SeekOrigin.Begin);
            byte[] endianCheck = ReadBytes(f, new byte[4]);
            if (Enumerable.SequenceEqual(endianCheck, new byte[] { 0x0, 0x0, 0x0, 0x10 })) LittleEndian = false;
            else if (Enumerable.SequenceEqual(endianCheck, new byte[] { 0x10, 0x0, 0x0, 0x0 })) LittleEndian = true;
            else throw new Exception();
            f.Seek(0, SeekOrigin.Begin);
            int texFileCount = DataRead.ToInt32(ReadBytes(f, new byte[4]), 0);

            for (int i = 0; i < texFileCount; i++)
            {
                f.Seek(0x10 + 0x20 * i, SeekOrigin.Begin);
                string fileName = ReadString(ReadBytes(f, new byte[0x10]), 0);
                string fileExt = ReadString(ReadBytes(f, new byte[0x4]), 0);
                uint fileSize = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);
                uint fileOffset = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);
                if (fileName == "" || fileSize == 0) continue;
                f.Seek(fileOffset, SeekOrigin.Begin);
                var outFile = File.Create(Path.Combine(outDirPath, fileName + "." + fileExt));
                outFile.Write(ReadBytes(f, new byte[fileSize]), 0, (int)fileSize);
                outFile.Close();
            }
        }

        public static void ExtractAll(string dirPath)
        {
            foreach (var f in Directory.GetFiles(dirPath, "*.tex", SearchOption.AllDirectories))
            {
                Extract(f);
            }
        }

        public static byte[] ReadBytes(FileStream f, byte[] buffer)
        {
            f.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static string ReadString(byte[] bytes, int position)
        {
            int endOfString = Array.IndexOf<byte>(bytes, 0x0, position);
            if (endOfString == position) return string.Empty;
            string s = Encoding.ASCII.GetString(bytes, position, endOfString - position);
            return s;
        }
    }
}