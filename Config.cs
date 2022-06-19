using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace POBC.TaskSystem
{
	public class TaskConfig
	{
		public _TaskList[] TaskList = new _TaskList[0];

        public TaskConfig Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static TaskConfig Read(string file)
		{
			if (!File.Exists(file))
			{
				WriteExample(file);
			}
			return JsonConvert.DeserializeObject<TaskConfig>(File.ReadAllText(file));
		}

		public static void WriteExample(string file)
		{
			var Ex = new _TaskList()
			{
				ID = 1,
				Name = "任务名称",
				Type = "主线",
				Info = "任务信息",
                DetailedInfo = "任务详细信息",
                Reward = new string[]
				{
					"/BC 这是服务器公告",
					"/BC 这是服务器公告"
				}
			};
			var Conf = new TaskConfig()
			{
				TaskList = new _TaskList[] { Ex }
			};
			Conf.Write(file);
		}
	}

	public class _TaskList
	{
		public int ID = 0;
		public string Type;
		public string Name;
		public string Info;
        public string DetailedInfo;
        public string[] Reward;
	}
}

