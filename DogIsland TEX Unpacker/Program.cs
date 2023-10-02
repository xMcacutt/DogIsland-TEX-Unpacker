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
                while (!string.Equals(input, "wii", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(input, "ps2", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Which version are you extracting files from?   \"wii\" / \"ps2\"");
                    input = Console.ReadLine();
                }
                if (string.Equals(input, "ps2", StringComparison.CurrentCultureIgnoreCase))
                    LittleEndian = true;

                input = "";
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
            string pacName = Path.GetFileNameWithoutExtension(path);
            string inDirPath = Path.GetDirectoryName(path);
            string outDirPath = Path.Combine(inDirPath, pacName + "tex");
            if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

            var f = File.OpenRead(path);
            int pacFileCount = DataRead.ToInt32(ReadBytes(f, new byte[4]), 0);

            for (int i = 0; i < pacFileCount; i++)
            {
                f.Seek(0x10 + 0x20 * i, SeekOrigin.Begin);
                string fileName = ReadString(ReadBytes(f, new byte[0x10]), 0);
                string fileExt = ReadString(ReadBytes(f, new byte[0x4]), 0);
                uint fileSize = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);
                uint fileOffset = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);

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