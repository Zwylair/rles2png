using System.Drawing;
using System.Runtime.InteropServices;
using Pfim;
using s4pi.ImageResource;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Invalid arguments lenght. Using: exe <input_file> <output_file | print>");
            Environment.Exit(1);
        }

        FileStream opened_rles = null;

        try
        {
            opened_rles = new FileStream(args[0], FileMode.Open);
        } catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(1);
        }

        var rleResource = new RLEResource(1, opened_rles);
        var image = Pfimage.FromStream(rleResource.ToDDS());

        var dataHandle = Marshal.AllocHGlobal(image.DataLen);
        Marshal.Copy(image.Data, 0, dataHandle, image.DataLen);

        using (Bitmap bitmap = new Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, dataHandle))
        {
            if (args[1] == "print")
            {
                using (MemoryStream mstream = new MemoryStream())
                {
                    bitmap.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);

                    byte[] byteArray = mstream.ToArray();
                    string base64String = Convert.ToBase64String(byteArray);

                    Console.WriteLine(base64String);
                }
            } else
            {
                try
                {
                    using (FileStream fstream = new FileStream(args[1], FileMode.Create))
                    {
                        bitmap.Save(fstream, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                }
                
            }
        }
        Marshal.FreeHGlobal(dataHandle);
    }
}
