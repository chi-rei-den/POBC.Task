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
		public DateTime F5Time;
		public DateTime GiveTime;
	}
	public class DBData
	{
		//public int ID;
		public string UserName;
		public string MianTaskUser;
		public int MianTaskData;
		public int MianTaskCompleted;
		public string RegionalTaskUser;
		public int RegionalTaskData;
		public int RegionalCompleted;        
	}
}
