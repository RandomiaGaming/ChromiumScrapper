using CefSharp.WinForms;
using CefSharp;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

public class MiniBrowser : BetterForm
{
	#region Public Variables
	public string URL { get { return _chromiumWebBrowser.Address; } }
	#endregion
	#region Private Variables
	private ChromiumWebBrowser _chromiumWebBrowser = null;
	private bool _loaded = false;
	#endregion
	#region Public Constructors
	public MiniBrowser()
	{
		lock (_miniBrowserCountLock)
		{
			if (_miniBrowserCount is 0)
			{
				InitializeCef();
			}

			_miniBrowserCount++;
		}

		Screen screen = Screen.PrimaryScreen;
		Point screenPos = screen.Bounds.Location;
		Size screenSize = screen.Bounds.Size;

		StartPosition = FormStartPosition.Manual;

		Location = new Point(screenPos.X + (screenSize.Width / 8), screenPos.Y + (screenSize.Height / 8));
		Size = new Size((screenSize.Width * 3) / 4, (screenSize.Height * 3) / 4);

		WindowState = FormWindowState.Maximized;

		_chromiumWebBrowser = new ChromiumWebBrowser("");
		_chromiumWebBrowser.FrameLoadEnd += (object sender, FrameLoadEndEventArgs e) =>
		{
			if (e.Frame.IsMain)
			{
				OnPageLoaded();
			}
		};

		Controls.Add(_chromiumWebBrowser);
	}
	#endregion
	#region Finalizer
	~MiniBrowser()
	{
		lock (_miniBrowserCountLock)
		{
			_miniBrowserCount--;

			if(_miniBrowserCount is 0)
			{
				ShutdownCef();
			}
		}
	}
	#endregion
	#region Public Methods
	public object RunScript(string code)
	{
		return RunLambda(async () =>
		{
			JavascriptResponse response = await _chromiumWebBrowser.EvaluateScriptAsync(code);

			if (!response.Success)
			{
				throw new Exception("Javascript exception: " + response.Message);
			}

			return response.Result;
		});
	}
	public string RunScriptOutString(string script)
	{
		object response = RunScript(script);
		if (response is null)
		{
			return null;
		}
		if (response.GetType() == typeof(string))
		{
			return (string)response;
		}
		else
		{
			throw new Exception("Javascript response was not a string.");
		}
	}
	public void LoadUrl(string url)
	{
		_loaded = false;

		RunLambda(() =>
		{
			_chromiumWebBrowser.Load(url);
		});

		while (!_loaded)
		{
			Thread.Sleep(100);
		}
	}
	#endregion
	#region Private Methods
	private void OnPageLoaded()
	{
		_loaded = true;
	}
	#endregion
	#region Private Static Variables
	private static object _miniBrowserCountLock = new object();
	private static int _miniBrowserCount = 0;
	#endregion
	#region Private Static Methods
	private static void InitializeCef()
	{
		if (Cef.IsInitialized)
		{
			Cef.Shutdown();
		}

		CefSettings settings = new CefSettings();

		settings.BrowserSubprocessPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + "\\CefSharp.BrowserSubprocess.exe";
		string roamingAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string chromiumScraperAppdataPath = Path.Combine(roamingAppdataPath, "ChromiumScraper");
		if (!Directory.Exists(chromiumScraperAppdataPath))
		{
			Directory.CreateDirectory(chromiumScraperAppdataPath);
		}
		string internalBrowserCachePath = Path.Combine(chromiumScraperAppdataPath, "InternalBrowserCache");
		if (!Directory.Exists(internalBrowserCachePath))
		{
			Directory.CreateDirectory(internalBrowserCachePath);
		}
		settings.CachePath = internalBrowserCachePath;
		
		settings.CefCommandLineArgs.Add("enable-blink-features", "BrowserContentPrefetch");
		
		settings.CefCommandLineArgs.Add("no-proxy-server", "1");
		
		Cef.Initialize(settings);
	}
	private static void ShutdownCef()
	{
		//Cef.Shutdown();
	}
	#endregion
}