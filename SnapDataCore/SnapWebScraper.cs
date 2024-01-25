using System;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using CefSharp.WinForms;
using CefSharp;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SnapData.Core
{
	public static class SnapWebScraper
	{
		private static bool _initializedInternalBrowser = false;
		private static InternalBrowser _internalBrowser = null;
		private static object _miniBrowserStateLock = new object();
		public static Bitmap GetBitmoji(string username)
		{
			CheckInternalBrowser();

			return null;
		}
		public static string GetDisplayName(string username)
		{
			CheckInternalBrowser();

			return null;
		}
		public static string GetCurrentUsername(string username)
		{
			CheckInternalBrowser();

			return null;
		}
		private static void CheckInternalBrowser()
		{
			if (_initializedInternalBrowser)
			{
				return;
			}

			lock (_miniBrowserStateLock)
			{


				_internalBrowser = new InternalBrowser();
				Thread internalBrowserThread = new Thread(() =>
				{
					_internalBrowser.ShowDialog();
				});
				internalBrowserThread.Start();
				_initializedInternalBrowser = true;
			}
		}
		private sealed class AddPageRequest
		{
			public string inUsername;
			public string outUsername;
			public string outDisplayName;
			public string outSnapcodeUrl;
		}
		private sealed class 
	}
}