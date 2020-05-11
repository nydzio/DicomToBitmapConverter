using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomToBitmapConverter
{
    class Program
    {
        public static gdcm.Bitmap pxmap2jpeg2000(gdcm.Pixmap px)
        {
            gdcm.ImageChangeTransferSyntax change = new gdcm.ImageChangeTransferSyntax();
            change.SetForce(false);
            change.SetCompressIconImage(false);
            change.SetTransferSyntax(new gdcm.TransferSyntax(gdcm.TransferSyntax.TSType.JPEG2000Lossless));

            change.SetInput(px);
            if (!change.Change())
                throw new Exception("Nie przekonwertowano typu bitmapy na jpeg2000");

            gdcm.Bitmap outimg = change.GetOutputAsBitmap();

            return outimg;
        }

        public static Bitmap[] gdcmBitmap2Bitmap(gdcm.Bitmap bmjpeg2000)
        {
            uint cols = bmjpeg2000.GetDimension(0);
            uint rows = bmjpeg2000.GetDimension(1);
            uint layers = bmjpeg2000.GetDimension(2);

            Bitmap[] ret = new Bitmap[layers];

            byte[] bufor = new byte[bmjpeg2000.GetBufferLength()];
            if (!bmjpeg2000.GetBuffer(bufor))
                throw new Exception("błąd pobrania bufora");

            for (uint l = 0; l < layers; l++)
            {
                Bitmap X = new Bitmap((int)cols, (int)rows);
                double[,] Y = new double[cols, rows];
                double m = 0;

                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                    {
                        int j = ((int)(l * rows * cols) + (int)(r * cols) + (int)c) * 2;
                        Y[r, c] = (double)bufor[j + 1] * 256 + (double)bufor[j];
                        if (Y[r, c] > m)
                            m = Y[r, c];
                    }

                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                    {
                        int f = (int)(255 * (Y[r, c] / m));
                        X.SetPixel(c, r, Color.FromArgb(f, f, f));
                    }

                ret[l] = X;
            }
            return ret;
        }

        static void Main(string[] args)
        {
            string file = "C:\\Users\\nydzi\\Downloads\\database\\DATA_DICOM\\drsprg_053\\drsprg_053_ANT.dcm";
            gdcm.PixmapReader reader = new gdcm.PixmapReader();
            reader.SetFileName(file);
            if (!reader.Read())
            {
                Console.WriteLine("pomijam: {0}", file);
            }

            gdcm.Bitmap bmjpeg2000 = pxmap2jpeg2000(reader.GetPixmap());
            Bitmap[] X = gdcmBitmap2Bitmap(bmjpeg2000);

            for (int i = 0; i < X.Length; i++)
            {
                String name = String.Format("{0}_warstwa{1}.bmp", file, i);
                X[i].Save(name);
                Console.WriteLine("konwersja do: {0}", name);
                Console.ReadKey();
            }
        }
    }
}
