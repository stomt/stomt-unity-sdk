using System;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine;

namespace Stomt
{
	public class StomtLog
	{
		// private StomtAPI _api;
		private Thread fileReadThread;
		private string logFileContent = null;
		private bool isLogFileReadComplete = false;

		public StomtLog (StomtAPI api)
		{
			// this._api = api;
			this.fileReadThread = new Thread(LoadLogFileThread);
			this.fileReadThread.Start();
		}

		public bool hasCopletedLoading() {
			return this.isLogFileReadComplete;
		}

		public string getFileConent() {
			return this.logFileContent;
		}

		public void stopThread() {
			if (this.fileReadThread != null && !this.fileReadThread.IsAlive)
			{
				this.fileReadThread.Abort ();
			}
		}

		// PRIVATE HELPERS

		private void LoadLogFileThread()
		{
			logFileContent = ReadFile(GetLogFilePath());
			this.isLogFileReadComplete = true;
		}

		private string GetLogFilePath()
		{
			string logFilePath = "";

			//////////////////////////////////////////////////////////////////
			// Windows Paths
			//////////////////////////////////////////////////////////////////

			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Unity\\Editor\\Editor.log";
			}

			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				logFilePath = "_EXECNAME_Data_\\output_log.txt";
			}

			//////////////////////////////////////////////////////////////////
			// OSX Paths
			//////////////////////////////////////////////////////////////////

			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				logFilePath = "~/Library/Logs/Unity/Editor.log";
			}

			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				logFilePath = "~/Library/Logs/Unity/Player.log";
			}

			//////////////////////////////////////////////////////////////////
			// Linux Paths
			//////////////////////////////////////////////////////////////////

			// LinuxEditor is not available in all unity version (e.g. 5.4.6)
			// FIXME: The following code is not save to use in all versions
			//RuntimePlatform LinuxEditor = default(RuntimePlatform);
			//if (Enum.IsDefined(typeof(RuntimePlatform), "LinuxEditor"))
			//{
			//	LinuxEditor = (RuntimePlatform)Enum.ToObject(typeof(RuntimePlatform), "LinuxEditor");
			//}
			//
			//if (Application.platform == LinuxEditor)
			//{
			//	logFilePath = "~/.config/unity3d/CompanyName/ProductName/Editor.log";
			//}

			if (Application.platform == RuntimePlatform.LinuxPlayer)
			{
				logFilePath = "~/.config/unity3d/CompanyName/ProductName/Player.log";
			}

			if (!string.IsNullOrEmpty(logFilePath))
			{
				if (File.Exists (logFilePath))
				{
					return logFilePath;
				}
				else
				{
					Debug.Log ("Log file does not exist in this path: " + logFilePath);
				}
			}

			return "";
		}

		private string ReadFile(string FilePath)
		{
			if (string.IsNullOrEmpty(FilePath))
			{
				return null;
			}

			var fileInfo = new System.IO.FileInfo(FilePath);

			if (fileInfo.Length > 30000000)
			{
				Debug.LogWarning("Log file too big. Size: " + fileInfo.Length + "Bytes. Path: " + FilePath);
				return null;
			}

			string FileCopyPath = FilePath + ".tmp.copy";

			// Copy File for reading an already opened file
			File.Copy(FilePath, FileCopyPath, true);

			// Read File
			StreamReader reader = new StreamReader(FileCopyPath);
			string content = reader.ReadToEnd();

			// Close stream and delete file copy
			reader.Close();
			File.Delete(FilePath + ".tmp.copy");

			return content;
		}
	}
}
