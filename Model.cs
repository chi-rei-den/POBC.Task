using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using TShockAPI;

namespace POBC.TaskSystem
{
    public class  Model
    {
		public static List<RandomTask> RandomTasks = new List<RandomTask> { };
		public static List<_TaskList> MainTaskLists = new List<_TaskList> { };
		public static List<_TaskList> RegionalTaskLists = new List<_TaskList> { };
	}
	public class RandomTask
	{
		public string Name;
		public ArrayList Reward;
		public DateTime Time;
	}
	public class DBData
	{
		//public int ID;
		public string UserName;
		public string MianTaskUser;
		public string MianTaskData;
		public int MianTaskCompleted;
		public string RegionalTaskUser;
		public string RegionalTaskData;
		public int RegionalCompleted;        
	}
}
