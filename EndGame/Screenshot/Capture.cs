﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using HDT.Plugins.EndGame.Properties;
using Hearthstone_Deck_Tracker.Stats;
using System.Drawing.Drawing2D;

using ScreenImage = HDT.Plugins.EndGame.Screenshot.Image;
using DrawImage = System.Drawing.Image;
using System.Threading.Tasks;


namespace HDT.Plugins.EndGame.Screenshot
{
	public class Capture
	{
		// Take a screenshot using the print screen button
		public static async Task Simple(int delay)
		{
			Logger.WriteLine("Capture (Simple) @ " + delay, "EndGame");
			await Task.Delay(delay);
			var sim = new WindowsInput.InputSimulator();
			sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.SNAPSHOT);
		}

		// Take a series of screenshots and display them so one can be selected to be saved
		public static async Task Advanced(int delay, string dir, int num, int delayBetween)
		{
			Logger.WriteLine("Capture (Advanced) @ " + delay + "/" + delayBetween, "EndGame");

			List<ScreenImage> screenshots = new List<ScreenImage>();

			await Task.Delay(delay);
			// disable overlay, before captures
			ForceHideOverlay(true);
			// take num screenshots
			for (int i = 0; i < num; i++)
			{
				Bitmap img = CaptureScreenShot();
				if(img != null)
				{
					Bitmap thb = ResizeImage(img);
					screenshots.Add(new ScreenImage(img, ToMediaImage(thb)));
					await Task.Delay(delayBetween);
				}
				else
				{
					Logger.WriteLine("Capture failed, reverting to Simple mode.", "EndGame");
					await Simple(delay);
				}
			}
			// enable overlay
			ForceHideOverlay(false);

			new NoteDialog(Game.CurrentGameStats, screenshots);
		}

		private static void ForceHideOverlay(bool force = true) {
			Helper.MainWindow.Overlay.ForceHidden = force;
			Helper.MainWindow.Overlay.UpdatePosition();
		}

		private static Bitmap CaptureScreenShot()
		{
			var rect = Helper.GetHearthstoneRect(true);
			var bmp = Helper.CaptureHearthstone(new Point(0,0), rect.Width, rect.Height);			
			return bmp;
		}

		// Based on: http://stackoverflow.com/a/2001692
		// "c# Image resizing to different size while preserving aspect ratio"
		// Resize and crop to 4:3
		private static Bitmap ResizeImage(Bitmap original)
		{
			double ratio = 4.0 / 3.0;
			int height = 100;
			int width = Convert.ToInt32(height * ratio);

			int cropWidth = Convert.ToInt32(original.Height * ratio);
			int posX = Convert.ToInt32((original.Width - cropWidth) / 2);

			DrawImage thumbnail = new Bitmap(width, height);
			Graphics graphic = Graphics.FromImage(thumbnail);
			graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphic.SmoothingMode = SmoothingMode.HighQuality;
			graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphic.CompositingQuality = CompositingQuality.HighQuality;

			graphic.DrawImage(original,
				new Rectangle(0, 0, width, height),
				new Rectangle(posX, 0, cropWidth, original.Height),
				GraphicsUnit.Pixel);

			graphic.Dispose();

			return new Bitmap(thumbnail);
		}

		// Based on: http://stackoverflow.com/a/3427387
		// "using XAML to bind to a System.Drawing.Image into a System.Windows.Image control"
		// Convert a bitmap to a Bitmap Image to be used as XAML Image source
		private static BitmapImage ToMediaImage(Bitmap bmp)
		{
			if (bmp == null)
				return null;

			var image = (DrawImage)bmp;

			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			MemoryStream memoryStream = new MemoryStream();
			// Save to a memory stream
			image.Save(memoryStream, ImageFormat.Bmp);
			// Rewind the stream
			memoryStream.Seek(0, SeekOrigin.Begin);
			bitmap.StreamSource = memoryStream;
			bitmap.EndInit();

			return bitmap;
		}
	}
}