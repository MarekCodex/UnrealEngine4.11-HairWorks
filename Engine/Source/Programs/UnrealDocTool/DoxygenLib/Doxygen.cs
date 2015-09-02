﻿// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenLib
{
	public class DoxygenConfig
	{
		public string Name;
		public string[] InputPaths;
		public string OutputPath;
		public string OutputTagfile;

		public List<string> Definitions = new List<string>();
		public List<string> IncludePaths = new List<string>();
		public List<string> ExcludePatterns = new List<string>();
		public List<string> InputTagfiles = new List<string>();
		public List<string> ExpandAsDefined = new List<string>();

		public DoxygenConfig(string InName, string[] InInputPaths, string InOutputPath)
		{
			Name = InName;
			InputPaths = InInputPaths;
			OutputPath = InOutputPath;
		}

		public void Write(string ConfigPath, string ExecutePath)
		{
			using (StreamWriter Output = new StreamWriter(ConfigPath))
			{
				Output.WriteLine("# Automatically generated; do not edit by hand");
				Output.WriteLine();

				// Write all the settings
				FormatSetting(Output, "PROJECT_NAME", "\"" + Name + "\"");
				FormatSetting(Output, "PROJECT_NUMBER", "");
				FormatSetting(Output, "LOOKUP_CACHE_SIZE", "4");
				FormatSetting(Output, "OUTPUT_DIRECTORY", QuoteValue(OutputPath));
				FormatSetting(Output, "OUTPUT_LANGUAGE", "English");
				FormatSetting(Output, "GENERATE_HTML", "NO");
				FormatSetting(Output, "GENERATE_LATEX", "NO");
				FormatSetting(Output, "GENERATE_XML", "YES");
				FormatSetting(Output, "WARN_IF_UNDOCUMENTED", "NO");
//				FormatSetting(Output, "QUIET", "YES");
				FormatSetting(Output, "EXTRACT_ALL", "YES");
				FormatSetting(Output, "WARNINGS", "YES");
				FormatSetting(Output, "RECURSIVE", "NO");
				FormatSetting(Output, "WARN_IF_UNDOCUMENTED", "NO");
				FormatSetting(Output, "WARN_IF_DOC_ERROR", "NO");
				FormatSetting(Output, "WARN_NO_PARAMDOC", "NO");
				FormatSetting(Output, "ENABLE_PREPROCESSING", "YES");
				FormatSetting(Output, "MACRO_EXPANSION", "YES");
				FormatSetting(Output, "SKIP_FUNCTION_MACROS", "NO");

				// Write the conditional settings
				if (OutputTagfile != null)
				{
					FormatSetting(Output, "GENERATE_TAGFILE", QuoteValue(OutputTagfile));
				}

				// List of input paths
				Output.WriteLine();
				FormatSetting(Output, "INPUT", new List<string>(InputPaths.Select(x => QuoteValue(x))));

				// Input filter program
				Output.WriteLine();
				string FilterPath = Path.GetDirectoryName(ExecutePath).Replace("\\", "/");
				for (int PathRemovalCoiunt = 0; PathRemovalCoiunt < 5; ++PathRemovalCoiunt)
				{
					// Back up five levels.
					FilterPath = FilterPath.Remove(FilterPath.LastIndexOf('/'));
				}
				FilterPath = "\"" + FilterPath + "/Source/Programs/UnrealDocTool/DoxygenInputFilter/perl.exe " + FilterPath + "/Source/Programs/UnrealDocTool/DoxygenInputFilter/DoxygenInputFilter.pl\" ";
				FormatSetting(Output, "INPUT_FILTER", FilterPath);

				// List of predefined macros
				Output.WriteLine();
				FormatSetting(Output, "PREDEFINED", new List<string>(Definitions.Select(x => QuoteValue(x))));

				// List of expanded macros
				Output.WriteLine();
				FormatSetting(Output, "EXPAND_AS_DEFINED", ExpandAsDefined);

				// List of valid file patterns
				Output.WriteLine();
				FormatSetting(Output, "FILE_PATTERNS", new List<string> { "*.h", "*.c", "*.hpp", "*.cpp", "*.inl", "*.inc" });

				// List of tag files
				Output.WriteLine();
				FormatSetting(Output, "TAGFILES", new List<string>(InputTagfiles.Select(x => QuoteValue(x))));

				// Only output the include paths which actually exist; UHT paths won't be created (but will be added to the include list) if a module does not have 
				// any UObjects, but Doxygen warns about it.
				List<string> ExistingIncludePaths = new List<string>();
				foreach (string IncludePath in IncludePaths)
				{
					if (Directory.Exists(IncludePath))
					{
						ExistingIncludePaths.Add(QuoteValue(IncludePath));
					}
					else
					{
						// Hidden for now; UBT generates a lot of default but invalid include paths for directories that may or may not have autogenerated object headers
						// Console.WriteLine("  Missing include path: {0}", IncludePath);
					}
				}
				Output.WriteLine();
				FormatSetting(Output, "INCLUDE_PATH", ExistingIncludePaths);

				// Exclude directory patterns
				Output.WriteLine();
				FormatSetting(Output, "EXCLUDE_PATTERNS", ExcludePatterns);
			}
		}

		static string QuoteValue(string Value)
		{
			return Value.Contains(' ') ? ("\"" + Value + "\"") : Value;
		}

		static void FormatSetting(StreamWriter Output, string Key, string Value)
		{
			Output.WriteLine("{0,-22} = {1}", Key, Value);
		}

		static void FormatSetting(StreamWriter Output, string Key, List<string> Values)
		{
			if (Values.Count == 0)
			{
				FormatSetting(Output, Key, "");
			}
			else if (Values.Count == 1)
			{
				FormatSetting(Output, Key, Values[0]);
			}
			else
			{
				Output.Write("{0,-22} = {1}", Key, Values[0]);
				for (int Idx = 1; Idx < Values.Count; Idx++)
				{
					Output.WriteLine(" \\");
					Output.Write("{0,-25}{1}", "", Values[Idx]);
				}
				Output.WriteLine();
			}
		}
	}

	public static class Doxygen
	{
		public static bool Run(string DoxygenPath, string WorkingDir, DoxygenConfig Config, bool bVerbose)
		{
			// Create the output directory
			if (!Directory.Exists(Config.OutputPath))
			{
				Directory.CreateDirectory(Config.OutputPath);
			}

			// Write the doxygen config file
			string ConfigFilePath = Path.Combine(Config.OutputPath, "Doxyfile");
			Config.Write(ConfigFilePath, DoxygenPath);

			// Spawn doxygen
			using (Process DoxygenProcess = new Process())
			{
				DoxygenProcess.StartInfo.WorkingDirectory = WorkingDir;
				DoxygenProcess.StartInfo.FileName = DoxygenPath;
				DoxygenProcess.StartInfo.Arguments = "\"" + ConfigFilePath + "\" -b";
				DoxygenProcess.StartInfo.UseShellExecute = false;
				DoxygenProcess.StartInfo.RedirectStandardOutput = true;
				DoxygenProcess.StartInfo.RedirectStandardError = true;

				using (StreamWriter DoxygenOutputWriter = new StreamWriter(Path.Combine(Config.OutputPath, "Doxygen.log")))
				{
					using (TextWriter SynchronizedDoxygenOutputWriter = TextWriter.Synchronized(DoxygenOutputWriter))
					{
						DoxygenProcess.OutputDataReceived += new DataReceivedEventHandler((x, y) => OutputReceived(x, y, SynchronizedDoxygenOutputWriter, bVerbose));
						DoxygenProcess.ErrorDataReceived += new DataReceivedEventHandler((x, y) => OutputReceived(x, y, SynchronizedDoxygenOutputWriter, bVerbose));
						try
						{
							DoxygenProcess.Start();
							DoxygenProcess.BeginOutputReadLine();
							DoxygenProcess.BeginErrorReadLine();
							DoxygenProcess.WaitForExit();
							return DoxygenProcess.ExitCode == 0;
						}
						catch (Exception Ex)
						{
							Console.WriteLine(Ex.ToString() + "\n" + Ex.StackTrace);
							return false;
						}
					}
				}
			}
		}

		static private void OutputReceived(Object Sender, DataReceivedEventArgs Line, TextWriter LogWriter, bool bVerbose)
		{
			if (Line.Data != null)
			{
				// Replace the 'warning:' tag with 'pedantic:' in missing include path messages, so as not to trip the build machines.
				// Most missing includes are by design because they're filtered out, and Doxygen is not being smart.
				string OutputLine = Line.Data;
				if(OutputLine.Contains("perhaps you forgot to add its directory to INCLUDE_PATH?"))
				{
					OutputLine = OutputLine.Replace("warning:", "pedantic:");
				}

				// Write it to the log
				LogWriter.WriteLine(OutputLine);
				if (bVerbose) Console.WriteLine("  doxygen> " + OutputLine);
			}
		}
	}
}
