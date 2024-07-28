using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using CefSharp.WinForms;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Threading;

public static class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		MiniBrowser miniBrowser = new MiniBrowser();

		miniBrowser.ShowOnSubThread();

		string[] usernames = File.ReadAllLines("D:\\Usernames.txt");

		foreach (string username in usernames)
		{
			SaveUserData(miniBrowser, username);
		}

		miniBrowser.RunLambda(() => { miniBrowser.Close(); });
	}
	public static void SaveUserData(MiniBrowser miniBrowser, string username)
	{
		UserData userData = GetUserData(miniBrowser, username);

		if (!Directory.Exists("D:\\Snapchat Profiles"))
		{
			Directory.CreateDirectory("D:\\Snapchat Profiles");
		}

		if (!Directory.Exists($"D:\\Snapchat Profiles\\{username}"))
		{
			Directory.CreateDirectory($"D:\\Snapchat Profiles\\{username}");
		}

		File.WriteAllText($"D:\\Snapchat Profiles\\{username}\\Account Info.txt", $"Original UserName: {userData.originalUsername}\nCurrent UserName: {userData.currentUsername}\nDisplay Name: {userData.displayName}");

		userData.bitmoji.Save($"D:\\Snapchat Profiles\\{username}\\Bitmoji.png");

		userData.bitmoji.Dispose();
	}
	public static UserData GetUserData(MiniBrowser miniBrowser, string username)
	{
		UserData output = new UserData();
		output.originalUsername = username;

		miniBrowser.LoadUrl("https://www.snapchat.com/add/" + username);

		try
		{
			output.displayName = miniBrowser.RunScriptOutString("document.getElementsByClassName(\"UserDetailsCard_container__WnRUt\")[0].children[0].textContent");
		}
		catch
		{
			//Secondary method for public profiles
			try
			{
				output.displayName = miniBrowser.RunScriptOutString("document.getElementsByClassName(\"PublicProfileDetailsCard_container__6EIHN\")[0].children[0].getElementsByClassName(\"PublicProfileDetailsCard_displayNameContainer__s83wG\")[0].textContent");
			}
			catch
			{
				output.displayName = $"UNKNOWN - {username}";
			}
		}

		try
		{
			output.currentUsername = miniBrowser.RunScriptOutString("document.getElementsByClassName(\"UserDetailsCard_container__WnRUt\")[0].children[1].textContent");
		}
		catch
		{
			try
			{
				output.currentUsername = miniBrowser.RunScriptOutString("document.getElementsByClassName(\"PublicProfileDetailsCard_container__6EIHN\")[0].children[0].getElementsByClassName(\"PublicProfileDetailsCard_textWrapper__y_pD5\")[0].textContent");
			}
			catch
			{
				output.currentUsername = $"UNKNOWN - {username}";
			}
		}

		try
		{
			string snapcodeUrl = miniBrowser.RunScriptOutString("document.getElementsByClassName(\"UserCard_verticalSnapcode__wUiCr\")[0].getElementsByTagName(\"img\")[0].src");

			miniBrowser.LoadUrl(snapcodeUrl);

			string bitmojiString = miniBrowser.RunScriptOutString("document.getElementsByTagName(\"image\")[0].href.baseVal");
			byte[] bitmojiBytes = Convert.FromBase64String(bitmojiString.Substring("data:image/png;base64,".Length));
			MemoryStream bitmojiStream = new MemoryStream(bitmojiBytes);
			output.bitmoji = (Bitmap)Image.FromStream(bitmojiStream);
			bitmojiStream.Dispose();
		}
		catch
		{
			output.bitmoji = new Bitmap("DefaultBitmoji.png");
		}

		return output;
	}
	public class UserData
	{
		public string originalUsername;
		public string currentUsername;
		public string displayName;
		public Bitmap bitmoji;
	}
}