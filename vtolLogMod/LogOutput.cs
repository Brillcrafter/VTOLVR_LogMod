using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace vtolLogMod;

public class LogOutput
{
    
    private const string BackgroundImagePath =@"C:\Users\Brill\RiderProjects\VTOLVR Mod\vtolLogMod\BackgroundImage.png";
    private const string SaveImagePath = @"C:\Users\Brill\RiderProjects\VTOLVR Mod\vtolLogMod\saveImage.png";
    
    private const int FontSize = 80;
    
    
    private Bitmap bitmap;
    private Graphics g;
    private Font font;
    
    
    public LogOutput()
    {
        bitmap = (Bitmap) Image.FromFile(BackgroundImagePath);

        g = Graphics.FromImage(bitmap);
        g.InterpolationMode = InterpolationMode.High;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.CompositingQuality = CompositingQuality.HighQuality;
        font = new Font("Arial", FontSize);
    }

    public void AddText(string text, int x, int y)
    {
        g.DrawString(text, font, Brushes.White, x, y);
        var graphicsPath = new GraphicsPath();
        graphicsPath.AddString(text, font.FontFamily, (int) font.Style, font.Size, new PointF(x, y), StringFormat.GenericDefault);
        g.DrawPath(Pens.Black, graphicsPath);
    }

    public void FinishImage()
    {
        bitmap.Save(SaveImagePath);
        bitmap.Dispose();
        g.Dispose();
        font.Dispose();
    }

    public void WriteText(PilotSave pilotSave)
    {
        var text= $"Total Missions:{pilotSave.NumberOfFailedMissions + pilotSave.NumberOfSuccessfulMissions}\n\n" +
                  $"Successful Missions:{pilotSave.NumberOfSuccessfulMissions}\n\n" +
                  $"Failed Missions:{pilotSave.NumberOfFailedMissions}\n\n" +
                  $"Total Deaths:{pilotSave.NumberOfDeaths}\n\n" +
                  $"Total Ejections:{pilotSave.NumberOfEjections}\n\n" +
                  $"Total Takeoffs:{pilotSave.NumberOfTakeoffs}\n\n" +
                  $"Total Landings:{pilotSave.NumberOfLandings}\n\n" +
                  $"Total Kills:{pilotSave.A2AKills+pilotSave.A2GKills+pilotSave.A2ShipKills}\n\n" +
                  $"Total A2A Kills:{pilotSave.A2AKills}\n\n" +
                  $"Total A2G Kills:{pilotSave.A2GKills}\n\n" +
                  $"Total Ship Kills:{pilotSave.A2ShipKills}\n\n";
        File.WriteAllText(Path.Combine(Main.Instance.saveFolder, Main.LogFileName), text);
    }
}