using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PalApi
{
    using SubProfile;

    public static class ImageExentions
    {
        #region Imaging Extensions
        public const string UserAvatarUrl = "https://clientavatars.palapi.net/FileServerSpring/subscriber/avatar/{0}?size=500#0";
        public const string GroupAvatarUrl = "https://clientavatars.palapi.net/FileServerSpring/group/avatar/{0}?size=500";

        public static Bitmap ToBitmap(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return new Bitmap(ms);
            }
        }

        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            using (var b = new Bitmap(bitmap))
            {
                b.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public static Bitmap CropToCircle(this Bitmap srcImage, Color backGround, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            var dstImage = new Bitmap(srcImage.Width, srcImage.Height, format);
            using (var g = Graphics.FromImage(dstImage))
            {
                g.Clear(backGround);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, dstImage.Width, dstImage.Height);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);
            }
            return dstImage;
        }

        public static Bitmap ResizeImage(this Bitmap value, int newWidth, int newHeight)
        {
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (var gfx = Graphics.FromImage(resizedImage))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.CompositingQuality = CompositingQuality.HighQuality;

                gfx.DrawImage(value, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }

        public static Bitmap DownloadImage(this WebClient client, string url)
        {
            using (client)
            {
                return client.DownloadData(url).ToBitmap();
            }
        }

        public static async Task<Bitmap> UserAvatar(this int userId)
        {
            using (var client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(string.Format(UserAvatarUrl, userId));
                return data.ToBitmap();
            }
        }

        public static async Task<Bitmap> Avatar(this User user)
        {
            return await UserAvatar(user.Id);
        }

        public static async Task<Bitmap> GroupAvatar(this int groupId)
        {
            using (var client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(string.Format(GroupAvatarUrl, groupId));
                return data.ToBitmap();
            }
        }

        public static async Task<Bitmap> Avatar(this Group group)
        {
            return await GroupAvatar(group.Id);
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (brush == null)
                throw new ArgumentNullException("brush");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }
        #endregion

        #region Bot Extenders
        public static async Task<Bitmap> GetImage(this IPalBot bot, string url)
        {
            using (var client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(url);
                return data.ToBitmap();
            }
        }

        public static async Task<byte[]> GetBytes(this IPalBot bot, string url)
        {
            using (var client = new WebClient())
            {
                return await client.DownloadDataTaskAsync(url);
            }
        }

        public static async Task<bool> Private(this IPalBot bot, int id, Bitmap image)
        {
            return await bot.Private(id, image.ToByteArray());
        }

        public static async Task<bool> Group(this IPalBot bot, int id, Bitmap image)
        {
            return await bot.Group(id, image.ToByteArray());
        }

        public static async Task<bool> Reply(this IPalBot bot, Message msg, Bitmap image)
        {
            return await bot.Reply(msg, image.ToByteArray());
        }

        public static async Task<bool> Avatar(this IPalBot bot, Bitmap image)
        {
            return await ((PalBot)bot).UpdateAvatar(image.ToByteArray());
        }

        public static async Task<bool> Avatar(this IPalBot bot, byte[] image)
        {
            return await ((PalBot)bot).UpdateAvatar(image);
        }
        #endregion
    }
}
