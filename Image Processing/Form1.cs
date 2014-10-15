using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace Image_Processing
{
    public partial class Form1 : Form
    {
        private GameStats GameStats;

        private const string SOURCE_IMAGE = "C:\\Scores.jpg";
        private const string TEMP_IMAGE = "C:\\temp.jpg";

        private const int PLAYER_WIDTH = 255;
        private const int PLAYER_HEIGHT = 27;
        private const int PLAYER_X = 965;

        private const int KDA_WIDTH = 35;
        private const int SCORE_WIDTH = 76;
        private const int KILL_X = 1224;
        private const int ASSIST_X = 1261;
        private const int DEATH_X = 1299;
        private const int SCORE_X = 1373;

        private const int MVP_HEIGHT = 15;
        private const int MVP_WIDTH = 21;
        private const int MVP_X = 1349;

        private const int PLAYER1_Y = 453;
        private const int PLAYER2_Y = 491;
        private const int PLAYER3_Y = 529;
        private const int PLAYER4_Y = 566;
        private const int PLAYER5_Y = 604;
        private const int PLAYER6_Y = 655;
        private const int PLAYER7_Y = 693;
        private const int PLAYER8_Y = 731;
        private const int PLAYER9_Y = 769;
        private const int PLAYER10_Y = 806;

        private const int MAP_X = 855;
        private const int MAP_Y = 391;
        private const int MAP_WIDTH = 150;
        private const int MAP_HEIGHT = 18;

        private const int FINAL_SCORE_WIDTH = 48;
        private const int FINAL_SCORE_HEIGHT = 37;
        private const int FINAL_SCORE_X = 868;

        private const int CT_SCORE_Y = 597;
        private const int T_SCORE_Y = 648;

        [DllImport("AspriseOCR.dll", EntryPoint = "OCR")]
        public static extern IntPtr OCR(string file, int type);

        public Form1()
        {
            InitializeComponent();

            GameStats = new GameStats();
            GameStats.CTStats = new List<PlayerStat>();
            GameStats.TStats = new List<PlayerStat>();

            ExtractGameStatInfo();
            
            ExtractPlayer(PLAYER1_Y, true);
            ExtractPlayer(PLAYER2_Y, true);
            ExtractPlayer(PLAYER3_Y, true);
            ExtractPlayer(PLAYER4_Y, true);
            ExtractPlayer(PLAYER5_Y, true);

            ExtractPlayer(PLAYER6_Y, false);
            ExtractPlayer(PLAYER7_Y, false);
            ExtractPlayer(PLAYER8_Y, false);
            ExtractPlayer(PLAYER9_Y, false);
            ExtractPlayer(PLAYER10_Y, false);
        }

        private void ExtractGameStatInfo()
        {
            GameStats.Map = ExtractGameData(MAP_X, MAP_Y, MAP_WIDTH, MAP_HEIGHT);

            string data = ExtractGameData(FINAL_SCORE_X, CT_SCORE_Y, FINAL_SCORE_WIDTH, FINAL_SCORE_HEIGHT);
            data = CleanseInt(data);
            GameStats.CTScore = int.Parse(data);

            data = ExtractGameData(FINAL_SCORE_X, T_SCORE_Y, FINAL_SCORE_WIDTH, FINAL_SCORE_HEIGHT);
            data = CleanseInt(data);
            GameStats.TScore = int.Parse(data);
        }

        private void ExtractPlayer(int y, bool ct)
        {
            PlayerStat stat = new PlayerStat();

            string data = string.Empty;
            int temp = -1;

            // Extract Player Name
            stat.PlayerName = ExtractGameData(PLAYER_X, y, PLAYER_WIDTH, PLAYER_HEIGHT);

            // Extract Kills
            data = ExtractGameData(KILL_X, y, KDA_WIDTH, PLAYER_HEIGHT);
            data = CleanseInt(data);
            stat.Kills = int.Parse(data);

            // Extract Assists
            data = ExtractGameData(ASSIST_X, y, KDA_WIDTH, PLAYER_HEIGHT);
            data = CleanseInt(data);
            stat.Assists = int.Parse(data);

            // Extract Deaths
            data = ExtractGameData(DEATH_X, y, KDA_WIDTH, PLAYER_HEIGHT);
            data = CleanseInt(data);
            stat.Deaths = int.Parse(data);

            // Extract MVPs (optional)
            data = ExtractGameData(MVP_X, y, MVP_WIDTH, MVP_HEIGHT);
            data = CleanseInt(data);
            if (int.TryParse(data, out temp))
                stat.MVPs = temp;

            // Extract Score
            data = ExtractGameData(SCORE_X, y, SCORE_WIDTH, PLAYER_HEIGHT);
            data = CleanseInt(data);
            stat.Score = int.Parse(data);

            if (ct)
                GameStats.CTStats.Add(stat);
            else
                GameStats.TStats.Add(stat);
        }

        private string CleanseInt(string data)
        {
            string foo = data;
            foo = foo.Replace('O', '0');
            foo = foo.Replace('o', '0');
            foo = foo.Replace('|', '1');
            foo = foo.Replace('l', '1');
            foo = foo.Replace('J', '7');
            
            return foo;
        }

        private string ExtractGameData(int x, int y, int width, int height)
        {
            // Extract the name of player 1
            Bitmap data = ExtractSubImage(x, y, width, height);

            // Enlarge image to 200%
            EnlargeImage(ref data);

            // Invert the colors of the image
            InvertImage(ref data);

            // Save since we cannot do the OCR in memory
            data.Save(TEMP_IMAGE, ImageFormat.Jpeg);

            // Get the string
            string result = Marshal.PtrToStringAnsi(OCR(TEMP_IMAGE, -1));

            // Delete the temp image
            File.Delete(TEMP_IMAGE);

            return result;
        }

        private void InvertImage(ref Bitmap player)
        {
            Color pixelColor;
            byte A, R, G, B;

            // Invert image
            for (int y = 0; y < player.Height; y++)
            {
                for (int x = 0; x < player.Width; x++)
                {
                    pixelColor = player.GetPixel(x, y);
                    A = (byte)(255 - pixelColor.A);
                    R = (byte)(255 - pixelColor.R);
                    G = (byte)(255 - pixelColor.G);
                    B = (byte)(255 - pixelColor.B);
                    player.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }
        }

        private void EnlargeImage(ref Bitmap player)
        {
            int newWidth = player.Width * 2;
            int newHeight = player.Height * 2;

            Bitmap enlargedBmp = new Bitmap(newWidth, newHeight);
            using (Graphics gr = Graphics.FromImage(enlargedBmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(player, new Rectangle(0, 0, newWidth, newHeight));
            }

            player = enlargedBmp;
        }

        private Bitmap ExtractSubImage(int x, int y, int width, int height)
        {
            Bitmap bmp = new Bitmap(SOURCE_IMAGE);
            return bmp.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppPArgb);            
        }
    }

    public class PlayerStat
    {
        public PlayerStat() {}

        public string PlayerName;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int MVPs;

        public int Score;
    }

    public class GameStats
    {
        public GameStats() { }

        public List<PlayerStat> CTStats;
        public List<PlayerStat> TStats;

        public string Map;

        public int CTScore;
        public int TScore;
    }
}
