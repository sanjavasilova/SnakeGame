using System.Drawing;
using System.IO;
using System;

public static class Images
{
    public static readonly Image Empty = LoadImage("Empty.png");
    public static readonly Image Body = LoadImage("Body.png");
    public static readonly Image Head = LoadImage("Head.png");
    public static readonly Image Food = LoadImage("Food.png");
    public static readonly Image DeadBody = LoadImage("DeadBody.png");
    public static readonly Image DeadHead = LoadImage("DeadHead.png");

    private static Image LoadImage(string fileName)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Image file '{fileName}' not found at path '{path}'.");
        }
        Image image = Image.FromFile(path);
        return ResizeImage(image, 30, 30);  
    }

    private static Image ResizeImage(Image image, int width, int height)
    {
        Bitmap newImage = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(newImage))
        {
            g.DrawImage(image, 0, 0, width, height);
        }
        return newImage;
    }
}
