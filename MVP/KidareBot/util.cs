using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Echo
{
    class util
    {

        public util()
        { }

        public void saveJpeg(string path, Bitmap img, long quality = 49L)
        {
            // Encoder parameter for image quality
            //long ImageQuality = 49L;
            EncoderParameter qualityParam =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            // Jpeg image codec
            string ImageCodec = "";
            if (path.Contains(".gif"))
                ImageCodec = "image/gif";
            else
                ImageCodec = "image/jpeg";

            ImageCodecInfo jpegCodec = getEncoderInfo(ImageCodec);

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
            //Console.WriteLine("Image was saved.");
        }

        private static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        public string CreateFolderNameForSaveImage(DateTime DT, string HostAddress, string IDName)
        {
            string DTMonth = DT.Month.ToString();
            if (DTMonth.Length <= 1) DTMonth = "0" + DTMonth;
            string dirName = String.Format("{0}\\{1}\\{2}", HostAddress, DT.Year, IDName);
            return dirName;
        }

        public void CreateFolderForSaveImageIfNotExist(string dirName)
        {
            if (!(System.IO.Directory.Exists(dirName)))
                System.IO.Directory.CreateDirectory(dirName);
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);
        public string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public string PriceSpliter(string price)
        {
            int TedadeVirgool = price.Length / 3;
            if ((price.Length % 3) == 0)
                TedadeVirgool = TedadeVirgool - 1;
            int InsertPos = 3;
            for (int i=0;i<TedadeVirgool;i++)
            {
                if (i > 0)
                    InsertPos = ((i+1) * 3)+i;
                price = price.Insert(price.Length - InsertPos, ",");
            }
            return price;
        }
        public string RemoveKeshidanAndNimSpace(string TextToConvert)
        {
            //٠١٢٣٤٥٦٧٨٩
            TextToConvert = TextToConvert.Replace("\u200C", " ");
            TextToConvert = TextToConvert.Replace("\u0640", "");
            TextToConvert = TextToConvert.Replace("\u200E", "");
            TextToConvert = TextToConvert.Replace("  ", " ");
            TextToConvert = TextToConvert.Replace("ي", "ی");
            TextToConvert = TextToConvert.Replace("ك", "ک");
            TextToConvert = TextToConvert.Replace("٠", "0");
            TextToConvert = TextToConvert.Replace("١", "1");
            TextToConvert = TextToConvert.Replace("٢", "2");
            TextToConvert = TextToConvert.Replace("٣", "3");
            TextToConvert = TextToConvert.Replace("٤", "4");
            TextToConvert = TextToConvert.Replace("٥", "5");
            TextToConvert = TextToConvert.Replace("٦", "6");
            TextToConvert = TextToConvert.Replace("٧", "7");
            TextToConvert = TextToConvert.Replace("٨", "8");
            TextToConvert = TextToConvert.Replace("٩", "9");
            //۱۲۳۴۵۶۷۸۹۰
            TextToConvert = TextToConvert.Replace("۰", "0");
            TextToConvert = TextToConvert.Replace("۱", "1");
            TextToConvert = TextToConvert.Replace("۲", "2");
            TextToConvert = TextToConvert.Replace("۳", "3");
            TextToConvert = TextToConvert.Replace("۴", "4");
            TextToConvert = TextToConvert.Replace("۵", "5");
            TextToConvert = TextToConvert.Replace("۶", "6");
            TextToConvert = TextToConvert.Replace("۷", "7");
            TextToConvert = TextToConvert.Replace("۸", "8");
            TextToConvert = TextToConvert.Replace("۹", "9");
            TextToConvert = TextToConvert.Trim();
            return TextToConvert;
        }
    }
}
