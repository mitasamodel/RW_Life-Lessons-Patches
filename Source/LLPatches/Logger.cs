using LLPatches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLPatches
{
	public static class Logger
	{
		private static bool _init = false;
		static readonly string logFile = @Environment.CurrentDirectory + @"\Mods\LLPatches.log";

		public static void Init()
		{
			if (!_init)
			{
				_init = true;
				if (LLPatchesMod.settings?.patchCEAmmo_Logging == true || LLPatchesMod.settings?.patchCEAmmo_LogUnpatched == true)
					File.WriteAllText(logFile, "[LLPatch] Debug start\n");    //create/rewrite file
#if DEBUG
				File.WriteAllText(logFile, "[LLPatch] Debug start\n");    //force in debug
#endif
			}
		}

		public static void Log(string msg)
		{
			if (!_init) Init();
			File.AppendAllText(logFile, msg + "\n");
		}

		public static void Log_Error(string str)
		{
			Verse.Log.Error($"[Life Lessons: Patches] {str}");
#if DEBUG
			if (!_init) Init();
			File.AppendAllText(logFile, str + "\n");
#else
			if (!_init) Init();
			if (LLPatchesMod.settings.patchCEAmmo_Logging)
				File.AppendAllText(logFile, str + "\n");
#endif
		}

		public static void DebugLog(string str)
		{
#if DEBUG
			if (!_init) Init();
			File.AppendAllText(logFile, str + "\n");
#endif
		}
	}
}
