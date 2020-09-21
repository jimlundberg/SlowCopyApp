using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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

        public static bool HasWriteAccessToFolder(string path)
        {
            string NtAccountName = @"Jim";

            DirectoryInfo di = new DirectoryInfo(path);
            DirectorySecurity acl = di.GetAccessControl(AccessControlSections.All);
            AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

            //Go through the rules returned from the DirectorySecurity
            foreach (AuthorizationRule rule in rules)
            {
                //If we find one that matches the identity we are looking for
                if (rule.IdentityReference.Value.Equals(NtAccountName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var filesystemAccessRule = (FileSystemAccessRule)rule;

                    //Cast to a FileSystemAccessRule to check for access rights
                    if ((filesystemAccessRule.FileSystemRights & FileSystemRights.WriteData) > 0 && filesystemAccessRule.AccessControlType != AccessControlType.Deny)
                    {
                        Console.WriteLine(string.Format("{0} has write access to {1}", NtAccountName, path));
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("{0} does not have write access to {1}", NtAccountName, path));
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Copy from input file to output file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public static void CopyFile(string inputFile, string outputFile)
        {
            // Set file security
            FileInfo outputFileInfo = new FileInfo(outputFile);
            var sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            FileSecurity outputFileSecurity = outputFileInfo.GetAccessControl();
            outputFileSecurity.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.WriteData, AccessControlType.Allow));
            outputFileInfo.SetAccessControl(outputFileSecurity);

            bool outputFileHasAccess = HasWriteAccessToFolder(outputFile);
            if (outputFileHasAccess == false)
            {
                AddDirectorySecurity(outputFile, "Jim", FileSystemRights.WriteData, AccessControlType.Allow);
                File.Delete(outputFile);
            }

            FileStream inputFileStream = File.Open(inputFile, FileMode.Open);
            FileStream outputFileStream = File.Open(outputFile, FileMode.Open);
            var lengthOfFile = outputFileStream.Length;
            for (int x = 0; x < lengthOfFile; x++)
            {
                outputFileStream.WriteByte((byte)inputFileStream.ReadByte());
                Thread.Sleep(TransferRate(lengthOfFile, 1000));
                Console.WriteLine("Progress = {0}", (x / lengthOfFile) * 100);
            }
        }

        /// <summary>
        /// Adds an ACL entry on the specified directory for the specified account.
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Account"></param>
        /// <param name="Rights"></param>
        /// <param name="ControlType"></param>
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object
            DirectoryInfo dirInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the current security settings
            DirectorySecurity dSecurity = dirInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings
            dirInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// Transfer an input directory to an out directory
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void TransferFiles(string sourceDirectory, string targetDirectory)
        {
            try
            {
                // Check if working directory has permissions
                string workingDirectory = @"C:\SSMCharacterizationHandler";
                bool baseHasAccess = HasWriteAccessToFolder(workingDirectory);
                if (baseHasAccess == false)
                {
                    AddDirectorySecurity(workingDirectory, "Jim", FileSystemRights.WriteData, AccessControlType.Allow);
                }

                // Create Target directory
                bool targetHasAccess = HasWriteAccessToFolder(targetDirectory);
                if (targetHasAccess == false)
                {
                    AddDirectorySecurity(targetDirectory, "Jim", FileSystemRights.WriteData, AccessControlType.Allow);
                    Directory.Delete(targetDirectory);
                }

                // Copy individual files from input directory to output directory
                string[] sourceFileList = Directory.GetFiles(sourceDirectory, "*.*");
                foreach (string file in sourceFileList)
                {
                    // Get the file name from the path
                    string fileName = file.Substring(sourceDirectory.Length + 1);

                    // Use CopyFile method to do a slow copy
                    CopyFile(Path.Combine(sourceDirectory, fileName), Path.Combine(targetDirectory, fileName));
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
            }
        }

        static void Main(string[] args)
        {
            string sourceDir;
            string outputDir;

            if (args.Length > 0)
            {
                sourceDir = args[0];
                outputDir = args[1];
            }
            else 
            {
                sourceDir = @"C:\SSMCharacterizationHandler\Test\1185840_202003250942";
                outputDir = @"C:\SSMCharacterizationHandler\Input Buffer\1185840_202003250942";
            }

            TransferFiles(sourceDir, outputDir);
        }
    }
}
