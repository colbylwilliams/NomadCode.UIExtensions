﻿using System;

#if __IOS__

using CoreGraphics;
using UIKit;

namespace NomadCode.UIExtensions
{
    public static class MediaExtensions
    {
        public static UIImage Crop (this UIImage image, CGRect rect)
        {
            rect = new CGRect (rect.X * image.CurrentScale,
                               rect.Y * image.CurrentScale,
                               rect.Width * image.CurrentScale,
                               rect.Height * image.CurrentScale);

            using (CGImage cr = image.CGImage.WithImageInRect (rect))
            {
                var cropped = UIImage.FromImage (cr, image.CurrentScale, image.Orientation);

                return cropped;
            }
        }


        public static void SaveAsJpeg (this UIImage image, string path)
        {
            using (var data = image.AsJPEG ())
            {
                data.Save (path, true);
            }
        }


        public static UIImage FixOrientation (this UIImage image)
        {
            if (image.Orientation == UIImageOrientation.Up)
            {
                return image;
            }

            var transform = CGAffineTransform.MakeIdentity ();

            switch (image.Orientation)
            {
                case UIImageOrientation.Down:
                case UIImageOrientation.DownMirrored:
                    transform = CGAffineTransform.Translate (transform, image.Size.Width, image.Size.Height);
                    transform = CGAffineTransform.Rotate (transform, (float)Math.PI);
                    break;
                case UIImageOrientation.Left:
                case UIImageOrientation.LeftMirrored:
                    transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
                    transform = CGAffineTransform.Rotate (transform, (float)Math.PI / 2);
                    break;
                case UIImageOrientation.Right:
                case UIImageOrientation.RightMirrored:
                    transform = CGAffineTransform.Translate (transform, 0, image.Size.Height);
                    transform = CGAffineTransform.Rotate (transform, -(float)Math.PI / 2);
                    break;
            }

            switch (image.Orientation)
            {
                case UIImageOrientation.UpMirrored:
                case UIImageOrientation.DownMirrored:
                    transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
                    transform = CGAffineTransform.Scale (transform, -1, 1);
                    break;
                case UIImageOrientation.LeftMirrored:
                case UIImageOrientation.RightMirrored:
                    transform = CGAffineTransform.Translate (transform, image.Size.Height, 0);
                    transform = CGAffineTransform.Scale (transform, -1, 1);
                    break;
            }

            using (var cgImg = image.CGImage)
            using (var ctx = new CGBitmapContext (null, (nint)image.Size.Width, (nint)image.Size.Height, cgImg.BitsPerComponent, 0, cgImg.ColorSpace, cgImg.BitmapInfo))
            {
                ctx.ConcatCTM (transform);

                switch (image.Orientation)
                {
                    case UIImageOrientation.Left:
                    case UIImageOrientation.LeftMirrored:
                    case UIImageOrientation.Right:
                    case UIImageOrientation.RightMirrored:
                        ctx.DrawImage (new CGRect (0, 0, image.Size.Height, image.Size.Width), cgImg);
                        break;
                    default:
                        ctx.DrawImage (new CGRect (0, 0, image.Size.Width, image.Size.Height), cgImg);
                        break;
                }

                using (var newCgImg = ctx.ToImage ())
                {
                    return UIImage.FromImage (newCgImg);
                }
            }
        }
    }
}

#endif