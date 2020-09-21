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
        public static double TransferRate(long fileSize, double bitsPerSecond)
        {
            double byteRate = bitsPerSecond / 8;
            double timeNeeded = 0;

            // Time needed in seconds
            if (fileSize < 100000)
            {
                timeNeeded = fileSize / byteRate * 2500;
            }
            else if (fileSize > 5000000)
            {
                timeNeeded = fileSize / byteRate / 75;
            }
            else
            {
                timeNeeded = fileSize / byteRate / 20;
            }

            // Return time needed
            return timeNeeded;
        }

        /// <summary>
        /// Copy a file with byte slowdown
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFile"></param>
        public static void TransferFiles(string sourceFile, string targetFile)
        {
            // Create input file stream
            FileStream fin = new FileStream(sourceFile, FileMode.Open);

            // Create target directory if if does not exist
            string targetDirectory = targetFile.Substring(0, targetFile.LastIndexOf(@"\"));
            if (Directory.Exists(targetDirectory) == false)
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Create output file stream
            FileStream fout = new FileStream(targetFile, FileMode.Create);

            int chr;
            int bitsPerSecond = 1000;
            long fileSize = fin.Length;
            double transferRate = TransferRate(fileSize, bitsPerSecond);
            DateTime start = DateTime.Now;

            Console.Write(string.Format("{0} -> {1} ", sourceFile, targetFile));

            do
            {
                // Read in byte from source
                chr = fin.ReadByte();
                if (chr != -1)
                {
                    // Write out byte to target
                    fout.WriteByte((byte)chr);

                    // Slow the writing
                    for (int i = 0; i < (int)transferRate; i++);
                }
            }
            while (chr != -1);

            TimeSpan timeSpan = DateTime.Now - start;
            Console.Write(" {0:ss\\:fff} sec\n", timeSpan);

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
                //sourceFile = @"C:\SSMCharacterizationHandler\Test\1185840_202003250942\1185840_202003250942_mode0.csv";
                //outputFile = @"C:\SSMCharacterizationHandler\Input Buffer\1185840_202003250942\1185840_202003250942_mode0.csv";
                sourceFile = @"C:\SSMCharacterizationHandler\Test\1185840_202003250942\75300037D00.xml";
                outputFile = @"C:\SSMCharacterizationHandler\Input Buffer\1185840_202003250942\75300037D00.xml";
            }

            TransferFiles(sourceFile, outputFile);
        }
    }
}
