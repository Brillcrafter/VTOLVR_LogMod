using vtolLogMod;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace imageWritingTest;

class Program
{
    private static PilotSave _pilotSave;
    private const string BackgroundImagePath =@"C:\Users\Brill\RiderProjects\VTOLVR Mod\vtolLogMod\BackgroundImage.png";
    private const string SaveImagePath = @"C:\Users\Brill\RiderProjects\VTOLVR Mod\vtolLogMod\saveImage.png";
    
    private const int FontSize = 80;
    
    static void Main(string[] args)
    {
        _pilotSave = new PilotSave(10, 15, 5, 12, 
            7, 2, 4, 1, 3);
        var bitmap = (Bitmap) Image.FromFile(BackgroundImagePath);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = InterpolationMode.High;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        using var font = new Font("Arial", FontSize);
        graphics.DrawString("Test", font, Brushes.White, 1920, 1080);
        var graphicsPath = new GraphicsPath();
        graphicsPath.AddString("Test", font.FontFamily, (int) font.Style,graphics.DpiY * FontSize / 72 , new PointF(1920, 1080), StringFormat.GenericDefault);
        //graphics.DrawPath(Pens.Black, graphicsPath);
        
            
            
        bitmap.Save(SaveImagePath);
    }
}