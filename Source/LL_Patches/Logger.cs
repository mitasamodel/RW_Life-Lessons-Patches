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
		static readonly string logFile = @Environment.CurrentDirectory + @"\Mods\LLPatches.log";

		public static void Init()
		{
			if (LLPatchesMod.settings.patchCEAmmo_Logging || LLPatchesMod.settings.patchCEAmmo_LogUnpatched)
				File.WriteAllText(logFile, "[LLPatch] Debug start\n");    //create/rewrite file
		}

		public static void Log(string msg)
		{
			File.AppendAllText(logFile, msg + "\n");
		}

		public static void Log_Error(string msg)
		{
			Verse.Log.Error($"[Life Lessons: Patches] {msg}");
			if (LLPatchesMod.settings.patchCEAmmo_Logging)
				File.AppendAllText(logFile, msg + "\n");
		}
	}
}
