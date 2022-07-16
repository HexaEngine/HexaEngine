//-----------------------------------------------------------------------
// <copyright file="FxCompile.cs" company="Microsoft">
// Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace ShaderCompiler
{
    /// <summary>
    /// Task to support Fxc.exe
    /// </summary>
    public class FxCompile : ToolTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FxCompile()
        {
            // Because FxCop wants it this way.
        }

        #region Inputs

        /// <summary>
        /// Sources to be compiled.
        /// </summary>
        /// <remarks>Required for task to run.</remarks>
        [Required]
        public virtual ITaskItem[] Source
        {
            get { return (ITaskItem[])Bag["Sources"]; }
            set { Bag["Sources"] = value; }
        }

        /// <summary>
        /// Gets the collection of parameters used by the derived task class.
        /// </summary>
        /// <value>Parameter bag.</value>
        protected internal Hashtable Bag
        {
            get
            {
                return bag;
            }
        }

        private Hashtable bag = new Hashtable();

        /// <summary>
        /// ShaderType, requires ShaderModel
        /// Specifies the type of shader.  (/T [type]_[model])
        /// </summary>
        /// <remarks>Consider using one of these: "NotSet", "Effect", "Vertex", "Pixel", "Geometry", "Hull", "Domain", "Compute", or "Texture".</remarks>
        public virtual string ShaderType
        {
            get
            {
                return (string)Bag["ShaderType"];
            }

            set
            {
                string result = string.Empty;
                switch (value.ToLowerInvariant())
                {
                    case "notset":
                        result = "";
                        break;

                    case "effect":
                        result = "/T fx";
                        break;

                    case "vertex":
                        result = "/T vs";
                        break;

                    case "pixel":
                        result = "/T ps";
                        break;

                    case "geometry":
                        result = "/T gs";
                        break;

                    case "hull":
                        result = "/T hs";
                        break;

                    case "domain":
                        result = "/T ds";
                        break;

                    case "compute":
                        result = "/T cs";
                        break;

                    case "texture":
                        result = "/T tx";
                        break;

                    default:
                        throw new ArgumentException("ShaderType of " + value + @" is an invalid.  Consider using one of these: ""NotSet"", ""Effect"", ""Vertex"", ""Pixel"", ""Geometry"", ""Hull"", ""Domain"", ""Compute"", or ""Texture"".");
                }

                Bag["ShaderType"] = result;
            }
        }

        /// <summary>
        /// ShaderModel, requires ShaderType
        /// Specifies the shader model. Some shader types can only be used with recent shader models (/T [type]_[model])
        /// </summary>
        public virtual string ShaderModel
        {
            get { return (string)Bag["ShaderModel"]; }
            set { Bag["ShaderModel"] = value; }
        }

        /// <summary>
        /// AssemblerOutput, requires AssemblerOutputFile
        /// Specifies the contents of assembly language output file. (/Fc, /Fx)
        /// </summary>
        /// <remarks>Consider using one of these: "Assembly Code" or "Assembly Code and Hex".</remarks>
        public virtual string AssemblerOutput
        {
            get
            {
                return (string)Bag["AssemblerOutput"];
            }

            set
            {
                string result = string.Empty;
                switch (value.ToLowerInvariant())
                {
                    case "Assembly Code":
                        result = "/Fc";
                        break;

                    case "Assembly Code and Hex":
                        result = "/Fx";
                        break;

                    default:
                        throw new ArgumentException("AssemblerOutput of " + value + @" is an invalid.  Consider using one of these: ""Assembly Code"" or ""Assembly Code and Hex"".");
                }

                Bag["AssemblerOutput"] = value;
            }
        }

        /// <summary>
        /// AssemblerOutputFile, requires AssemblerOutput
        /// Specifies file name for assembly code listing file
        /// </summary>
        public virtual string AssemblerOutputFile
        {
            get { return (string)Bag["AssemblerOutputFile"]; }
            set { Bag["AssemblerOutputFile"] = value; }
        }

        /// <summary>
        /// Specifies a name for the variable name in the header file (/Vn [name])
        /// </summary>
        public virtual string VariableName
        {
            get { return (string)Bag["VariableName"]; }
            set { Bag["VariableName"] = value; }
        }

        /// <summary>
        /// Specifies a name for header file containing object code. (/Fh [name])
        /// </summary>
        public virtual string HeaderFileOutput
        {
            get { return (string)Bag["HeaderFileOutput"]; }
            set { Bag["HeaderFileOutput"] = value; }
        }

        /// <summary>
        /// Specifies a name for object file. (/Fo [name])
        /// </summary>
        public virtual string ObjectFileOutput
        {
            get { return (string)Bag["ObjectFileOutput"]; }
            set { Bag["ObjectFileOutput"] = value; }
        }

        /// <summary>
        /// Defines preprocessing symbols for your source file.
        /// </summary>
        public virtual string[] PreprocessorDefinitions
        {
            get { return (string[])Bag["PreprocessorDefinitions"]; }
            set { Bag["PreprocessorDefinitions"] = value; }
        }

        /// <summary>
        /// Specifies one or more directories to add to the include path; separate with semi-colons if more than one. (/I[path])
        /// </summary>
        public virtual string[] AdditionalIncludeDirectories
        {
            get { return (string[])Bag["AdditionalIncludeDirectories"]; }
            set { Bag["AdditionalIncludeDirectories"] = value; }
        }

        /// <summary>
        /// Suppresses the display of the startup banner and information message. (/nologo)
        /// </summary>
        public virtual bool SuppressStartupBanner
        {
            get { return GetBoolParameterWithDefault("SuppressStartupBanner", false); }
            set { Bag["SuppressStartupBanner"] = value; }
        }

        /// <summary>
        /// Specifies the name of the entry point for the shader (/E[name])
        /// </summary>
        public virtual string EntryPointName
        {
            get { return (string)Bag["EntryPointName"]; }
            set { Bag["EntryPointName"] = value; }
        }

        /// <summary>
        /// Treats all compiler warnings as errors. For a new project, it may be best to use /WX in all compilations; resolving all warnings will ensure the fewest possible hard-to-find code defects.
        /// </summary>
        public virtual bool TreatWarningAsError
        {
            get { return GetBoolParameterWithDefault("TreatWarningAsError", false); }
            set { Bag["TreatWarningAsError"] = value; }
        }

        /// <summary>
        /// Disable optimizations. /Od implies /Gfp though output may not be identical to /Od /Gfp.
        /// </summary>
        public virtual bool DisableOptimizations
        {
            get { return GetBoolParameterWithDefault("DisableOptimizations", false); }
            set { Bag["DisableOptimizations"] = value; }
        }

        /// <summary>
        /// Enable debugging information.
        /// </summary>
        public virtual bool EnableDebuggingInfo
        {
            get { return GetBoolParameterWithDefault("EnableDebuggingInfo", false); }
            set { Bag["EnableDebuggingInfo"] = value; }
        }

        /// <summary>
        /// Compiler will assume that all resources that a shader may reference are bound and are in good
        /// state for the duration of shader execution (/all_resources_bound). Available for Shader Model
        /// 5.1 and above.
        /// </summary>
        public virtual bool AllResourcesBound
        {
            get { return GetBoolParameterWithDefault("AllResourcesBound", false); }
            set { Bag["AllResourcesBound"] = value; }
        }

        /// <summary>
        /// Inform the compiler that a shader may contain a declaration of a resource array with unbounded range
        /// (/enable_unbounded_descriptor_tables). Available for Shader Model 5.1 and above.
        /// </summary>
        public virtual bool EnableUnboundedDescriptorTable
        {
            get { return GetBoolParameterWithDefault("EnableUnboundedDescriptorTable", false); }
            set { Bag["EnableUnboundedDescriptorTable"] = value; }
        }

        /// <summary>
        /// Attach root signature to shader bytecode (/setrootsignature). Available for Shader Model 5.0 and above.
        /// </summary>
        public virtual bool SetRootSignature
        {
            get { return GetBoolParameterWithDefault("SetRootSignature", false); }
            set { Bag["SetRootSignature"] = value; }
        }

        public virtual string ConsumeExportFile
        {
            get { return (string)Bag["ConsumeExportFile"]; }
            set { Bag["ConsumeExportFile"] = value; }
        }

        public virtual string GenerateExportFile
        {
            get { return (string)Bag["GenerateExportFile"]; }
            set { Bag["GenerateExportFile"] = value; }
        }

        public virtual string GenerateExportShaderProfile
        {
            get { return (string)Bag["GenerateExportShaderProfile"]; }
            set
            {
                string profile = null;

                string[] inputParts = value.Split(';');
                if (inputParts.Length == 2 && inputParts[0].Equals("Pixel", StringComparison.OrdinalIgnoreCase))
                {
                    if (inputParts[1].Equals("4_0", StringComparison.OrdinalIgnoreCase))
                    {
                        profile = "lib_4_0";
                    }
                    else if (inputParts[1].Equals("4_0_level_9_1", StringComparison.OrdinalIgnoreCase))
                    {
                        profile = "lib_4_0_level_9_1_ps_only";
                    }
                    else if (inputParts[1].Equals("4_0_level_9_3", StringComparison.OrdinalIgnoreCase))
                    {
                        profile = "lib_4_0_level_9_3_ps_only";
                    }
                    else if (inputParts[1].Equals("4_1", StringComparison.OrdinalIgnoreCase))
                    {
                        profile = "lib_4_1";
                    }
                    else if (inputParts[1].Equals("5_0", StringComparison.OrdinalIgnoreCase))
                    {
                        profile = "lib_5_0";
                    }
                }

                Bag["GenerateExportShaderProfile"] = profile;
            }
        }

        /// <summary>
        /// Name to Fxc.exe
        /// </summary>
        protected override string ToolName
        {
            get { return "Fxc.exe"; }
        }

        #endregion Inputs

        /// <summary>
        /// Returns a string with those switches and other information that can't go into a response file and
        /// must go directly onto the command line.
        /// Called after ValidateParameters and SkipTaskExecution
        /// </summary>
        /// <returns></returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension();
            AddCommandLineCommands(commandLineBuilder);
            return commandLineBuilder.ToString();
        }

        /// <summary>
        /// Returns the command line switch used by the tool executable to specify the response file
        /// Will only be called if the task returned a non empty string from GetResponseFileCommands
        /// Called after ValidateParameters, SkipTaskExecution and GetResponseFileCommands
        /// </summary>
        /// <param name="responseFilePath">full path to the temporarily created response file</param>
        /// <returns></returns>
        protected override string GenerateResponseFileCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension();
            AddResponseFileCommands(commandLineBuilder);
            return commandLineBuilder.ToString();
        }

        /// <summary>
        /// Fills the provided CommandLineBuilderExtension with those switches and other information that can go into a response file.
        /// </summary>
        /// <param name="commandLine"></param>
        protected internal virtual void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
        }

        /// <summary>
        /// Add Command Line Commands
        /// </summary>
        /// <param name="commandLine">CommandLineBuilderExtension</param>
        protected internal void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            //// Order of these affect the order of the command line

            if (ConsumeExportFile != null || GenerateExportFile != null || GenerateExportShaderProfile != null)
            {
                // Custom effects for D2D are enabled. We need to add the path to the D2D1EffectHelpers.hlsli file.
                if (AdditionalIncludeDirectories == null || AdditionalIncludeDirectories.Length == 0)
                {
                    AdditionalIncludeDirectories = new string[] { GetPathToD2D1EffectHelpersHlsli() };
                }
                else
                {
                    List<string> includeDirs = new List<string>(AdditionalIncludeDirectories.Length + 1);
                    includeDirs.AddRange(AdditionalIncludeDirectories);
                    includeDirs.Add(GetPathToD2D1EffectHelpersHlsli());

                    AdditionalIncludeDirectories = includeDirs.ToArray();
                }
            }

            commandLine.AppendSwitchIfNotNull("/I ", AdditionalIncludeDirectories, " /I ");
            commandLine.AppendSwitch(SuppressStartupBanner ? "/nologo" : string.Empty);
            commandLine.AppendSwitchIfNotNull("/E", EntryPointName);
            commandLine.AppendSwitch(TreatWarningAsError ? "/WX" : string.Empty);

            // Switch cannot be null
            if (ShaderType != null && ShaderModel != null)
            {
                // shader Model and Type are one switch
                commandLine.AppendSwitch(ShaderType + "_" + ShaderModel);
            }

            commandLine.AppendSwitchIfNotNull("/D ", PreprocessorDefinitions, " /D ");
            commandLine.AppendSwitchIfNotNull("/Fh ", HeaderFileOutput);
            commandLine.AppendSwitchIfNotNull("/Fo ", ObjectFileOutput);

            // Switch cannot be null
            if (AssemblerOutput != null)
            {
                commandLine.AppendSwitchIfNotNull(AssemblerOutput, AssemblerOutputFile);
            }

            commandLine.AppendSwitchIfNotNull("/Vn ", VariableName);
            commandLine.AppendSwitch(DisableOptimizations ? "/Od" : string.Empty);
            commandLine.AppendSwitch(EnableDebuggingInfo ? "/Zi" : string.Empty);
            commandLine.AppendSwitch(AllResourcesBound ? "/all_resources_bound" : string.Empty);
            commandLine.AppendSwitch(EnableUnboundedDescriptorTable ? "/enable_unbounded_descriptor_tables" : string.Empty);
            commandLine.AppendSwitch(SetRootSignature ? "/setrootsignature" : string.Empty);

            commandLine.AppendSwitchIfNotNull("/setprivate ", ConsumeExportFile);
            commandLine.AppendSwitchIfNotNull("/Fl ", GenerateExportFile);
            commandLine.AppendSwitchIfNotNull("/T ", GenerateExportShaderProfile);

            commandLine.AppendSwitchIfNotNull("", Source, " ");
        }

        /// <summary>
        /// Fullpath to the fxc.exe
        /// </summary>
        /// <returns>Fullpath to fxc.exe, if found.  Otherwise empty or null.</returns>
        protected override string GenerateFullPathToTool()
        {
            // Find the path to the most recent compiler.

            string fxcFullPath = null;
            Version fxcVersion = null;

            foreach (string kitRoot in GetWindowsKitRoots())
            {
                string binPath = Path.Combine(kitRoot, "bin");
                if (Directory.Exists(binPath))
                {
                    DirectoryInfo info = new DirectoryInfo(binPath);
                    FileInfo[] files = info.GetFiles(ToolName, SearchOption.AllDirectories);
                    if (files != null && files.Length > 0)
                    {
                        Version version;
                        foreach (var file in files)
                        {
                            if (!file.FullName.Contains("\\x86\\"))
                                continue;

                            FileVersionInfo tmpVer = FileVersionInfo.GetVersionInfo(file.FullName);
                            version = new Version(tmpVer.FileMajorPart, tmpVer.FileMinorPart, tmpVer.FileBuildPart, tmpVer.FilePrivatePart);
                            if (fxcVersion == null || version > fxcVersion)
                            {
                                fxcFullPath = file.FullName;
                                fxcVersion = version;
                            }
                        }
                    }
                }
            }

            Debug.Assert(fxcFullPath != null, "No fxc.exe found.");
            return fxcFullPath;
        }

        /// <summary>
        /// Get a bool parameter and return a default if its not present
        /// in the hash table.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <owner>JomoF</owner>
        protected internal bool GetBoolParameterWithDefault(string parameterName, bool defaultValue)
        {
            object obj = bag[parameterName];
            return obj == null ? defaultValue : (bool)obj;
        }

        private IList<string> GetWindowsKitRoots()
        {
            IList<string> kitRootsList = new List<string>();

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows Kits\Installed Roots", writable: false);
            if (key != null)
            {
                using (key)
                {
                    var kitRoots = from v in key.GetValueNames()
                                   where !string.IsNullOrEmpty(v) && v.StartsWith("KitsRoot", StringComparison.OrdinalIgnoreCase)
                                   orderby v descending
                                   select v;

                    foreach (string kitRoot in kitRoots)
                    {
                        string path = key.GetValue(kitRoot, string.Empty).ToString();

                        if (Directory.Exists(path))
                        {
                            kitRootsList.Add(path);
                        }
                    }
                }
            }

            return kitRootsList;
        }

        private string GetPathToD2D1EffectHelpersHlsli()
        {
            const string kFileName = "d2d1effecthelpers.hlsli";

            string fullPath = null;

            foreach (string kitRoot in GetWindowsKitRoots())
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(kitRoot);
                FileInfo[] files = directoryInfo.GetFiles(kFileName, SearchOption.AllDirectories);
                if (files != null && files.Length > 0)
                {
                    // Just take the first directory we find.
                    fullPath = Path.GetDirectoryName(files[0].FullName);
                    break;
                }
            }

            Debug.Assert(fullPath != null, string.Format("No {0} found.", kFileName));
            return fullPath;
        }
    }
}