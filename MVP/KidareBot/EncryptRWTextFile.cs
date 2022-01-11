using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Telegram.Bot.Examples.Echo
{
    class EncryptRWTextFile
    {

        string passPhrase = "Goosfands";        // can be any string
        string saltValue = "sALtValue";        // can be any string
        string hashAlgorithm = "SHA1";             // can be "MD5"
        int passwordIterations = 7;                  // can be any number
        string initVector = "~1B2c3D4e6F6g7H8"; // must be 16 bytes
        int keySize = 256;                // can be 192 or 128

        public EncryptRWTextFile()
        {

        }

        public string ReadFromTextFile()
        {
            String line = "";
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("gidoravaredmishavad.txt");

                //Read the first line of text
                line = sr.ReadLine();

                ////Continue to read until you reach end of file
                //while (line != null)
                //{
                //    //write the lie to console window
                //    Console.WriteLine(line);
                //    //Read the next line
                //    line = sr.ReadLine();
                //}

                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return line;
        }

        public void WriteOnTextFile(string TextToWrite)
        {
            try
            {

                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter("gidoravaredmishavad.txt");

                //Write a line of text
                sw.WriteLine(TextToWrite);

                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        public string Encrypt(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(this.initVector);
            byte[] rgbSalt = Encoding.ASCII.GetBytes(this.saltValue);
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            byte[] rgbKey = new PasswordDeriveBytes(this.passPhrase, rgbSalt, this.hashAlgorithm, this.passwordIterations).GetBytes(this.keySize / 8);
            RijndaelManaged managed = new RijndaelManaged();
            managed.Mode = CipherMode.CBC;
            ICryptoTransform transform = managed.CreateEncryptor(rgbKey, bytes);
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.FlushFinalBlock();
            byte[] inArray = stream.ToArray();
            stream.Close();
            stream2.Close();
            return Convert.ToBase64String(inArray);
        }

        public string Decrypt(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(this.initVector);
            byte[] rgbSalt = Encoding.ASCII.GetBytes(this.saltValue);
            byte[] buffer = Convert.FromBase64String(data);
            byte[] rgbKey = new PasswordDeriveBytes(this.passPhrase, rgbSalt, this.hashAlgorithm, this.passwordIterations).GetBytes(this.keySize / 8);
            RijndaelManaged managed = new RijndaelManaged();
            managed.Mode = CipherMode.CBC;
            ICryptoTransform transform = managed.CreateDecryptor(rgbKey, bytes);
            MemoryStream stream = new MemoryStream(buffer);
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            byte[] buffer5 = new byte[buffer.Length];
            int count = stream2.Read(buffer5, 0, buffer5.Length);
            stream.Close();
            stream2.Close();
            return Encoding.UTF8.GetString(buffer5, 0, count);
        }
    }
}
