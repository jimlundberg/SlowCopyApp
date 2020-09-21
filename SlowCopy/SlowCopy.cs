using System;
using System.IO;
using System.Threading;

namespace TransferFilesNS
{
    class SlowCopy
    {
        /// <summary>
        /// Calculate the transfer rate
        /// </summary>
        /// <param name="fileSize"></param>
        /// <param name="bitsPerSecond"></param>
        /// <returns></returns>
        public static int TransferRate(long fileSize, float bitsPerSecond)
        {
            float byteRate = bitsPerSecond / 8;

            // Time needed in seconds
            float timeNeeded = fileSize / byteRate;

            // Convert the time needed in milliseconds
            int ms = (int)(timeNeeded * 1000);

            // Return transfer time
            return ms;
        }

        /// <summary>
        /// Copy a file with byte slowdown
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFile"></param>
        public static void TransferFiles(string sourceFile, string targetFile)
        {
            int chr;
            FileStream fin = new FileStream(sourceFile, FileMode.Open);
            FileStream fout = new FileStream(targetFile, FileMode.Create);
            int bitsPerSecond = 7;  // works imperically
            int fileSize = sourceFile.Length;
            int transferRate = TransferRate(fileSize, bitsPerSecond);
            DateTime start = DateTime.Now;

            Console.Write(string.Format("Copying {0} -> {1} ", sourceFile, targetFile));

            do
            {
                // Read in byte from source
                chr = fin.ReadByte();
                if (chr != -1)
                {
                    // Write out byte to target
                    fout.WriteByte((byte)chr);

                    // Slow the writing
                    for (int i = 0; i < transferRate; i++);
                }
            }
            while (chr != -1);

            TimeSpan timeSpan = DateTime.Now - start;
            Console.Write("Complete taking {0:hh\\:mm\\:ss}\n\n", timeSpan);

            fin.Close();
            fout.Close();
        }

        static void Main(string[] args)
        {
            string sourceFile;
            string outputFile;

            if (args.Length > 0)
            {
                sourceFile = args[0];
                outputFile = args[1];
            }
            else 
            {
                sourceFile = @"C:\SSMCharacterizationHandler\Test\1185840_202003250942\75300037D00.xml";
                outputFile = @"C:\SSMCharacterizationHandler\Input Buffer\1185840_202003250942\75300037D00.xml";
            }

            TransferFiles(sourceFile, outputFile);
        }
    }
}
