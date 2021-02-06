using UniLua;
using System;
using Modding;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Modding.Blocks;

namespace LuaScripting
{
	public class Mod : ModEntryPoint
	{
		public static List<string> defaultLuaFiles = new List<string>()
		{
            {
				"LuaRoot/main.lua"
			}
		};

		public static List<string> defaultLuaData => new List<string>()
		{
            {
				ModIO.ReadAllText("main.lua.default")
            }
		};

		public static MessageType setSliderValueMsg, setEmulateStateMsg;

		public override void OnLoad()
		{	
			Libs.MachineLib.Init();
			Libs.GUILib.Init();
			Libs.InputLib.Init();
			Libs.LinesLib.Init();
			LuaScripting.Init();

			setSliderValueMsg = ModNetworking.CreateMessageType(new DataType[]
			{
				DataType.Block,
				DataType.Integer,
				DataType.Single
			});

			setEmulateStateMsg = ModNetworking.CreateMessageType(new DataType[]
			{
				DataType.Block,
				DataType.Integer,
				DataType.Boolean,
			});

			ModNetworking.MessageReceived += (msg) =>
			{
				try
                {
					if (msg.Type == setSliderValueMsg)
					{
						Block block = (Block) msg.GetData(0);
						int fieldHashCode = (int) msg.GetData(1);
						float value = (float) msg.GetData(2);

						MapperType mapperType = null;

						if (block != null && block.InternalObject != null && block.InternalObject.SimBlock != null)
							for (int i = 0; i < block.InternalObject.SimBlock.MapperTypes.Count; i++)
								if (Utils.GetStableHashCode(block.InternalObject.SimBlock.MapperTypes[i].Key) == fieldHashCode)
								{
									mapperType = block.InternalObject.SimBlock.MapperTypes[i];
									break;
								}

						if (mapperType != null)
						{
							MSlider slider = mapperType as MSlider;
							slider.SetValue(value);
							slider.ApplyValue();
						}
					}

					if (msg.Type == setEmulateStateMsg)
					{
						Block block = (Block) msg.GetData(0);
						KeyCode key = (KeyCode) (int) msg.GetData(1);
						bool state = (bool) msg.GetData(2);

						if (block != null)
						{
							foreach (MapperType mapperType in block.SimBlock.InternalObject.MapperTypes)
								if (mapperType is MKey)
								{
									MKey mkey = mapperType as MKey;
									if (mkey.HasKey(key))
										mkey.UpdateEmulation(state);
								}
						}
					}
				} catch (NullReferenceException)
                {
					// wellp, nothing to do here oof
                }
			};

			UnityEngine.Object.DontDestroyOnLoad(SingleInstance<LuaScripting>.Instance);
		}


		public static void SaveLuaRootToMachine()
        {
			List<string> allFiles = new List<string>();
			List<string> allData = new List<string>();

			RecursivelyAddAllLuaFiles("LuaRoot", allFiles);

			foreach (string file in allFiles)
				allData.Add(ModIO.ReadAllText(file));

			Machine.Active().MachineData.Write("lua_files", allFiles.ToArray());
			Machine.Active().MachineData.Write("lua_data", allData.ToArray());
			Machine.Active().MachineData.Write("lua", true);

			Debug.Log("[LuaScripting] Saved LuaRoot to machine.");
		}

		public static void LoadLuaRootFromMachine(bool useDefault = false)
		{
			if (ModIO.ExistsDirectory("LuaRoot"))
				ModIO.DeleteDirectory("LuaRoot", true);
			ModIO.CreateDirectory("LuaRoot");

			if (!Machine.Active().MachineData.HasKey("lua"))
				useDefault = true;
			if (useDefault)
				Debug.Log("[LuaScripting] Loaded machinne does not have LuaRoot in. Loading default.");


			List<string> allFiles = useDefault ? defaultLuaFiles : Machine.Active().MachineData.ReadStringArray("lua_files").ToList();
			List<string> allData = useDefault ? defaultLuaData : Machine.Active().MachineData.ReadStringArray("lua_data").ToList();

			if (useDefault)
            {
				allData[0] = $"-- Created {DateTime.Now} \n\n" + allData[0]; 
            }

			for (int i = 0; i < allFiles.Count; i++)
            {
				string file = allFiles[i];
				string data = allData[i];

				string[] d = file.Split('/');
				string[] c = new string[d.Length - 1];
				for (int j = 0; j < d.Length - 1; j++)
					c[i] = d[i];

				string path = string.Join("/", c);

				ModIO.CreateDirectory(path);
				ModIO.WriteAllText(file, data);
			}

			Debug.Log("[LuaScripting] Loaded LuaRoot from machine.");
		}

		private static void RecursivelyAddAllLuaFiles(string dir, List<string> files)
        {
			string[] allFoundFiles = ModIO.GetFiles(dir);
			List<string> luaFiles = new List<string>();
			foreach (string file in allFoundFiles)
				if (file.EndsWith(".lua"))
					luaFiles.Add(file);
			files.AddRange(luaFiles);
			foreach (string dir2 in ModIO.GetDirectories(dir))
				RecursivelyAddAllLuaFiles(dir2, files);
        }
	}
}
