using System;

namespace Vector.VLConfig.HardwareAccess.ToolInterfaces
{
	public class GL4000ctrl : GenericToolInterface
	{
		public GL4000ctrl()
		{
			base.FileName = "GL4000ctrl.exe";
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
	}
}
