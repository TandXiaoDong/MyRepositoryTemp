using Nini.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vector.VLConfig.Data.ApplicationData;
using Vector.VLConfig.HardwareAccess.HardwareAbstraction;
using Vector.VLConfig.LoggerSpecifics;

namespace Vector.VLConfig.HardwareAccess.ToolInterfaces
{
	public class GL2000ctrl : GenericToolInterface
	{
		private readonly string SectionDriveLetters = "DriveLetters";

		private readonly string FieldLoggerDrivesCount = "LoggerDrivesCount";

		private readonly string FieldLoggerDrives = "LoggerDrives";

		private readonly string FieldRemovableDrivesCount = "RemovableDrivesCount";

		private readonly string FieldRemovableDrives = "RemovableDrives";

		private readonly string SectionLoggerStatus = "LoggerStatus";

		private readonly string SectionLogData = "LogData";

		private readonly string SectionConfigArea = "ConfigArea";

		private readonly string FieldArticleNum = "ArticleNum";

		private readonly string FieldSerNum = "SerNum";

		private readonly string FieldCardSizeMB = "CardSizeMB";

		private readonly string FieldBufferSizeKB = "BufferSizeKB";

		private readonly string FieldSingleFile = "SingleFile";

		private readonly string FieldCAN1baby = "CAN1baby";

		private readonly string FieldCAN2baby = "CAN2baby";

		private readonly string FieldCompileTime = "CompileTime";

		private readonly string FieldRTSversion = "RTSversion";

		private readonly string FieldRecordingBufs = "RecordingBufs";

		private readonly string FieldTriggeredBufs = "TriggeredBufs";

		private readonly string FieldTriggerTime_Dec = "TriggerTime{0}";

		private readonly string FieldLicense_Dec = "License{0}";

		private readonly string FieldFileName = "FileName";

		private readonly string FieldDataSizeKB = "DataSizeKB";

		private readonly string OpenBufferName = "OpenBuffer";

		public GL2000ctrl()
		{
			base.FileName = "GL2000ctrl.exe";
		}

		public bool GetAvailableLoggerDrives(out IList<string> driveLettersConnectedLoggers, out IList<string> driveLettersMemoryCards)
		{
			driveLettersConnectedLoggers = new List<string>();
			driveLettersMemoryCards = new List<string>();
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-l");
			base.RunSynchronous();
			if (!base.ParseLastStdOutAsIni())
			{
				return false;
			}
			IConfig config = base.StdOutAsIniConfigSource.Configs[this.SectionDriveLetters];
			if (config != null)
			{
				if (config.Contains(this.FieldLoggerDrivesCount))
				{
					int @int = config.GetInt(this.FieldLoggerDrivesCount);
					string @string = config.GetString(this.FieldLoggerDrives);
					for (int i = 0; i < @int; i++)
					{
						if (i <= @string.Length)
						{
							string item = @string.Substring(i, 1);
							driveLettersConnectedLoggers.Add(item);
						}
					}
				}
				if (config.Contains(this.FieldRemovableDrivesCount))
				{
					int int2 = config.GetInt(this.FieldRemovableDrivesCount);
					string string2 = config.GetString(this.FieldRemovableDrives);
					List<string> list = new List<string>();
					for (int j = 0; j < int2; j++)
					{
						string item2 = string2.Substring(j, 1);
						list.Add(item2);
					}
					Dictionary<string, DriveInfo> dictionary = new Dictionary<string, DriveInfo>();
					foreach (string current in list)
					{
						string driveName = current + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar;
						DriveInfo driveInfo = null;
						try
						{
							driveInfo = new DriveInfo(driveName);
						}
						catch (Exception)
						{
							continue;
						}
						if (driveInfo.DriveType == DriveType.Removable)
						{
							bool flag = false;
							try
							{
								string driveFormat = driveInfo.DriveFormat;
								if (driveFormat == "")
								{
									dictionary.Add(current, driveInfo);
								}
								else if (driveFormat == Constants.FileSystemFormatFAT || driveFormat == Constants.FileSystemFormatFAT32)
								{
									flag = true;
								}
							}
							catch (Exception)
							{
								dictionary.Add(current, driveInfo);
							}
							if (flag)
							{
								try
								{
									if (driveInfo.IsReady && driveInfo.RootDirectory.GetFileSystemInfos().Count<FileSystemInfo>() == 0)
									{
										driveLettersMemoryCards.Add(current);
									}
								}
								catch (Exception)
								{
								}
							}
						}
					}
					foreach (KeyValuePair<string, DriveInfo> current2 in dictionary)
					{
						GL2000Infos gL2000Infos = default(GL2000Infos);
						IList<ILogFile> list2 = new List<ILogFile>();
						if (this.GetLoggerInfos(current2.Key, ref gL2000Infos, ref list2))
						{
							driveLettersMemoryCards.Add(current2.Key);
						}
					}
				}
				base.ClearIniParser();
				return true;
			}
			base.ClearIniParser();
			return false;
		}

		public bool GetLoggerInfos(string driveLetter, ref GL2000Infos gl2000Infos, ref IList<ILogFile> logFiles)
		{
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-i");
			base.AddCommandLineArgument("-L " + driveLetter + ":");
			base.RunSynchronous();
			if (!base.ParseLastStdOutAsIni())
			{
				return false;
			}
			IConfig config = base.StdOutAsIniConfigSource.Configs[this.SectionLoggerStatus];
			if (config == null)
			{
				base.ClearIniParser();
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (config.Contains(this.FieldArticleNum))
			{
				stringBuilder.Append(config.GetString(this.FieldArticleNum));
				stringBuilder.Append("-");
			}
			if (config.Contains(this.FieldSerNum))
			{
				stringBuilder.Append(config.GetString(this.FieldSerNum));
			}
			gl2000Infos.serialNumber = stringBuilder.ToString();
			if (config.Contains(this.FieldCardSizeMB))
			{
				gl2000Infos.memCardSizeMB = (uint)config.GetInt(this.FieldCardSizeMB);
			}
			if (config.Contains(this.FieldSingleFile))
			{
				gl2000Infos.isSingleFile = (config.GetInt(this.FieldSingleFile) == 1);
			}
			if (config.Contains(this.FieldCAN1baby))
			{
				gl2000Infos.can1Baby = config.GetString(this.FieldCAN1baby);
			}
			if (config.Contains(this.FieldCAN2baby))
			{
				gl2000Infos.can2Baby = config.GetString(this.FieldCAN2baby);
			}
			if (config.Contains(this.FieldRTSversion))
			{
				gl2000Infos.firmwareVersion = config.GetString(this.FieldRTSversion);
			}
			uint num = 0u;
			if (config.Contains(this.FieldBufferSizeKB))
			{
				num = (uint)config.GetInt(this.FieldBufferSizeKB);
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			int num2 = 1;
			bool flag;
			do
			{
				flag = false;
				string key = string.Format(this.FieldLicense_Dec, num2);
				if (config.Contains(key))
				{
					flag = true;
					if (stringBuilder2.Length > 0)
					{
						stringBuilder2.Append(", ");
					}
					stringBuilder2.Append(config.GetString(key));
					num2++;
				}
			}
			while (flag);
			gl2000Infos.licenses = stringBuilder2.ToString();
			config = base.StdOutAsIniConfigSource.Configs[this.SectionConfigArea];
			if (config != null && config.Contains(this.FieldFileName))
			{
				gl2000Infos.configName = config.GetString(this.FieldFileName);
				if (!string.IsNullOrEmpty(gl2000Infos.configName) && config.Contains(this.FieldCompileTime))
				{
					string @string = config.GetString(this.FieldCompileTime);
					DateTime compileDateTime = new DateTime(0L);
					if (this.ParseDateTimeField(@string, ref compileDateTime))
					{
						gl2000Infos.compileDateTime = compileDateTime;
					}
				}
			}
			bool flag2 = false;
			config = base.StdOutAsIniConfigSource.Configs[this.SectionLogData];
			if (config != null)
			{
				if (!config.Contains(this.FieldRecordingBufs) || !config.Contains(this.FieldTriggeredBufs))
				{
					gl2000Infos.recordingBufs = 0u;
					gl2000Infos.triggeredBufs = 0u;
				}
				else
				{
					gl2000Infos.recordingBufs = (uint)config.GetInt(this.FieldRecordingBufs);
					gl2000Infos.triggeredBufs = (uint)config.GetInt(this.FieldTriggeredBufs);
				}
				gl2000Infos.freeSpaceMB = gl2000Infos.memCardSizeMB - (gl2000Infos.triggeredBufs + gl2000Infos.recordingBufs) * num / 1024u;
				logFiles.Clear();
				if (gl2000Infos.triggeredBufs + gl2000Infos.recordingBufs == 0u)
				{
					base.ClearIniParser();
					return true;
				}
				if (gl2000Infos.isSingleFile)
				{
					uint fileSize = (gl2000Infos.triggeredBufs + gl2000Infos.recordingBufs) * num;
					if (config.Contains(this.FieldDataSizeKB))
					{
						fileSize = (uint)config.GetInt(this.FieldDataSizeKB);
					}
					DateTime timestamp = default(DateTime);
					if (gl2000Infos.triggeredBufs > 0u && !this.ParseTriggerEntry(gl2000Infos.triggeredBufs, config, out timestamp))
					{
						flag2 = true;
					}
					if (!flag2)
					{
						ILogFile item = new LogFile("PermanentLogging", fileSize, timestamp);
						logFiles.Add(item);
					}
				}
				else
				{
					for (uint num3 = 1u; num3 <= gl2000Infos.triggeredBufs + gl2000Infos.recordingBufs; num3 += 1u)
					{
						bool flag3 = false;
						DateTime timestamp2 = default(DateTime);
						if (num3 > gl2000Infos.triggeredBufs && gl2000Infos.recordingBufs > 0u)
						{
							flag3 = true;
						}
						else if (!this.ParseTriggerEntry(num3, config, out timestamp2))
						{
							flag2 = true;
							break;
						}
						string defaultName;
						if (flag3)
						{
							defaultName = this.OpenBufferName;
						}
						else
						{
							defaultName = string.Format(Path.GetFileNameWithoutExtension(Constants.DefaultRawLogDataFileName) + "{0:D3}", num3);
						}
						ILogFile item2 = new LogFile(defaultName, num, timestamp2);
						logFiles.Add(item2);
					}
				}
			}
			base.ClearIniParser();
			return !flag2;
		}

		public bool DownloadFile(string driveLetter, FileConversionParameters conversionParameters, string destFilePath, IProgressIndicator progressIndicator, ProcessExitedDelegate processExitedDelegate, out string errorText)
		{
			GL1000DownloadProgressValueParser progressIndicatorValueParser = new GL1000DownloadProgressValueParser();
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-R");
			base.AddCommandLineArgument(string.Format("\"{0}\"", destFilePath));
			base.AddCommandLineArgument("-o");
			base.AddCommandLineArgument("-v");
			if (!string.IsNullOrEmpty(driveLetter))
			{
				base.AddCommandLineArgument("-L " + driveLetter + ":");
			}
			if (conversionParameters.SuppressBufferConcat)
			{
				base.AddCommandLineArgument("-0");
			}
			progressIndicator.SetMinMax(0, 100);
			base.RunAsynchronousWithProgressBar(progressIndicator, progressIndicatorValueParser, processExitedDelegate);
			if (base.LastExitCode != 0)
			{
				errorText = base.GetGinErrorCodeString(base.LastExitCode);
				return false;
			}
			errorText = "";
			return true;
		}

		public bool ClearMemoryCard(string driveLetter, bool deleteWithConfigAndUserData, out string errorText)
		{
			errorText = "";
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-v");
			base.AddCommandLineArgument("-n");
			base.AddCommandLineArgument("-d");
			if (deleteWithConfigAndUserData)
			{
				base.AddCommandLineArgument("-DC");
				base.AddCommandLineArgument("-DU");
			}
			if (!string.IsNullOrEmpty(driveLetter))
			{
				base.AddCommandLineArgument("-L " + driveLetter + ":");
			}
			base.RunSynchronous();
			if (base.LastExitCode != 0)
			{
				errorText = base.GetGinErrorCodeString(base.LastExitCode);
				return false;
			}
			return true;
		}

		public bool WriteConfiguration(string driveLetter, string codFilePath, out string errorText)
		{
			errorText = "";
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-v");
			base.AddCommandLineArgument("-n");
			base.AddCommandLineArgument(string.Format("-WC \"{0}\"", codFilePath));
			if (!string.IsNullOrEmpty(driveLetter))
			{
				base.AddCommandLineArgument("-L " + driveLetter + ":");
			}
			base.RunSynchronous();
			if (base.LastExitCode != 0)
			{
				errorText = base.GetGinErrorCodeString(base.LastExitCode);
				return false;
			}
			return true;
		}

		public bool SetRealTimeClock(string driveLetter, out string errorText)
		{
			errorText = "";
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-v");
			base.AddCommandLineArgument("-T");
			base.RunSynchronous();
			if (base.LastExitCode != 0)
			{
				errorText = base.GetGinErrorCodeString(base.LastExitCode);
				return false;
			}
			return true;
		}

		public bool SetVehicleName(string name, out string errorText)
		{
			errorText = "";
			base.DeleteCommandLineArguments();
			base.AddCommandLineArgument("-v");
			base.AddCommandLineArgument(string.Format("-N \"{0}\"", name));
			base.RunSynchronous();
			if (base.LastExitCode != 0)
			{
				errorText = base.GetGinErrorCodeString(base.LastExitCode);
				return false;
			}
			return true;
		}

		private bool ParseTriggerEntry(uint triggerPos, IConfig config, out DateTime timestamp)
		{
			timestamp = default(DateTime);
			string key = string.Format(this.FieldTriggerTime_Dec, triggerPos);
			if (!config.Contains(key))
			{
				return false;
			}
			string @string = config.GetString(key);
			if (string.IsNullOrEmpty(@string))
			{
				return false;
			}
			string[] array = @string.Split(new char[]
			{
				' ',
				':',
				'.'
			});
			if (array.Count<string>() != 6)
			{
				return false;
			}
			try
			{
				timestamp = new DateTime(Convert.ToInt32(array[2]), Convert.ToInt32(array[1]), Convert.ToInt32(array[0]), Convert.ToInt32(array[3]), Convert.ToInt32(array[4]), Convert.ToInt32(array[5]));
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		private bool ParseDateTimeField(string dateTimeString, ref DateTime parsedDateTimeValue)
		{
			if (string.IsNullOrEmpty(dateTimeString))
			{
				return false;
			}
			string[] array = dateTimeString.Split(new char[]
			{
				' ',
				':',
				'.'
			});
			if (array.Count<string>() == 6)
			{
				parsedDateTimeValue = new DateTime(Convert.ToInt32(array[2]), Convert.ToInt32(array[1]), Convert.ToInt32(array[0]), Convert.ToInt32(array[3]), Convert.ToInt32(array[4]), Convert.ToInt32(array[5]));
				return true;
			}
			return false;
		}
	}
}
