using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace POBC.TaskSystem
{
    namespace TaskSystem
    {
        [ApiVersion(2, 1)]
        public class TaskSystem : TerrariaPlugin
        {
            /// <summary>
            /// Gets the author(s) of this plugin
            /// </summary>
            public override string Author => "欲情";

            /// <summary>
            /// Gets the description of this plugin.
            /// A short, one lined description that tells people what your plugin does.
            /// </summary>
            public override string Description => "POBC 任务系统";

            /// <summary>
            /// Gets the name of this plugin.
            /// </summary>
            public override string Name => "POBC.TaskSystem";

            /// <summary>
            /// Gets the version of this plugin.
            /// </summary>
            public override Version Version => new Version(1, 0, 0, 1);
            public string ConfigPath { get { return Path.Combine(TShock.SavePath, "POBCTask.json"); } }
            public TaskConfig MainTaskLists = new TaskConfig();

            /// <summary>
            /// Initializes a new instance of the TestPlugin class.
            /// This is where you set the plugin's order and perfrom other constructor logic
            /// </summary>
            public TaskSystem(Main game) : base(game)
            {

            }

            /// <summary>
            /// Handles plugin initialization. 
            /// Fired when the server is started and the plugin is being loaded.
            /// You may register hooks, perform loading procedures etc here.
            /// </summary>
            public override void Initialize()
            {
                File();
                Db.Connect();
                ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
                ServerApi.Hooks.NpcKilled.Register(this, NpcKill);

            }

            private void NpcKill(NpcKilledEventArgs args)
            {
                if (args.npc.lastInteraction == 255) return;
                if (Model.MainTaskLists.Count == 0) return;
                if (Model.RegionalTaskLists.Count == 0) return;
                var player = TShock.Players[args.npc.lastInteraction];
                DBData dBData = new DBData();
                if (Db.Queryuser(player.Name))
                {
                    dBData = Db.QueryData(player.Name);
                }
                else return;
                if (dBData.MianTaskUser == null & dBData.RegionalTaskUser ==null) return;
                if (dBData.MianTaskUser!=null)
                {
                    if (Model.MainTaskLists.Find(x => x.Name == dBData.MianTaskUser).Conditions.Any(x => x.TaskType == "2" ))
                    {
                        var task = Model.MainTaskLists.Find(x => x.Name == dBData.MianTaskUser).Conditions.Where(x => x.TaskType == "2").FirstOrDefault();
                        var Coordinate = task.Condition.Split(',');
                        if (Coordinate[0]==args.npc.netID.ToString())
                        {
                            dBData.MianTaskData += 1;
                            Db.UPData(dBData);
                            player.SendInfoMessage("[主线任务]你击杀了 " + args.npc.FullName + " ,已击杀" + dBData.MianTaskData + "个怪物,任务需要击杀" + Coordinate[1] + "个怪物");
                        }

                    }
                }
                if (dBData.RegionalTaskUser != null)
                {
                    if (Model.RegionalTaskLists.Find(x => x.Name == dBData.RegionalTaskUser).Conditions.Any(x => x.TaskType == "2"))
                    {
                        var task = Model.RegionalTaskLists.Find(x => x.Name == dBData.RegionalTaskUser).Conditions.Where(x => x.TaskType == "2").FirstOrDefault();
                        var Coordinate = task.Condition.Split(',');
                        if (Coordinate[0] == args.npc.netID.ToString())
                        {
                            dBData.RegionalTaskData += 1;
                            Db.UPData(dBData);
                            player.SendInfoMessage("[支线任务]你击杀了 " + args.npc.FullName + " ,已击杀" + dBData.RegionalTaskData + "个怪物,任务需要击杀" + Coordinate[1] + "个怪物");
                        }
                    }
                }
            }

            private void OnInitialize(EventArgs args)
            {
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystem, "task","任务") //测试完成
                {
                    HelpText = " POBC 任务系统"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemList, "tasklist","任务列表" ) //测试完成
                {
                    HelpText = " POBC 任务系统 所有任务"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemQuery, "taskquery", "查询任务") //测试完成
                {
                    HelpText = " POBC 任务系统 通过id或任务名称 查询任务信息"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemPick, "tasklist", "接受任务") //测试完成
                {
                    HelpText = "接受任务时 随机5项任务选择1项 可以接受单个任务，为了提高任务的难度放弃任务和刷新可接受任务有CD时间 /n 主线任务默认接取"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemf5, "taskf5", "刷新任务")  //测试完成
                {
                    HelpText = "刷新随机支线任务可接取列表，注意刷新时间冷却 .主线任务不可刷新"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemGive, "taskgive", "放弃任务")
                {
                    HelpText = "放弃任务时 ，为了提高任务的难度放弃任务和刷新可接受任务有CD时间 /n 主线任务不可放弃"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemOver, "taskover", "完成任务")
                {
                    HelpText = "完成任务 ， 并获取奖励"
                });
            }

            private void _TaskSystemQuery(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("语法错误,正确语法: /查询任务 id或任务名称");
                    return;
                }
                var AllTask = Model.MainTaskLists.Concat(Model.RegionalTaskLists).ToList<_TaskList>();
                var Task = AllTask.FirstOrDefault(x => x.Name == args.Parameters[0] || x.ID.ToString() == args.Parameters[0]);
                if (Task == null)
                {
                    args.Player.SendErrorMessage("没有找到任务");
                    return;
                }
                args.Player.SendInfoMessage("任务ID：" + Task.ID);
                args.Player.SendInfoMessage("任务类型：" + Task.Type);
                args.Player.SendInfoMessage("任务名称：" + Task.Name);
                args.Player.SendInfoMessage("任务描述：" + Task.Info);
                args.Player.SendInfoMessage("详细信息：" + Task.DetailedInfo);
               // args.Player.SendInfoMessage("任务条件：" + Task.Conditions.ToString());
                //args.Player.SendInfoMessage("任务奖励：" + Task.Reward);
                
            }

            private void _TaskSystemOver(CommandArgs args)
            {
                //完成任务
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("语法错误，正确语法：/完成任务 主线 （/完成任务 支线）");
                    return;
                }
                switch (args.Parameters[0])
                {
                    case "主线":
                        {
                            if (Model.MainTaskLists.Count == 0)
                            {
                                args.Player.SendErrorMessage("没有主线任务");
                                return;
                            }
                            if (!Db.Queryuser(args.Player.Name))
                            {
                                args.Player.SendErrorMessage("没有接受的主线任务");
                                return;
                            }
                            if (Db.QueryData(args.Player.Name).MianTaskUser == null)
                            {
                                args.Player.SendErrorMessage("没有接受的主线任务");
                                return;
                            }
                            //获取主线任务 
                            var _DBData = Db.QueryData(args.Player.Name);
                            var _MainTask = Model.MainTaskLists.Where(x => x.Name == _DBData.MianTaskUser).FirstOrDefault();
                            Tasklogic(args, _MainTask);

                            _DBData.MianTaskUser = null;
                            _DBData.MianTaskData = 0;
                            _DBData.MianTaskCompleted += 1;
                            Db.UPData(_DBData);
                            args.Player.SendErrorMessage("主线任务 完成 : 任务名" + _MainTask.Name + " ID:" + _MainTask.ID);
                        }
                        break;
                    case "支线":
                        {
                            if (Model.RegionalTaskLists.Count == 0)
                            {
                                args.Player.SendErrorMessage("没有支线任务");
                                return;
                            }
                            if (!Db.Queryuser(args.Player.Name))
                            {
                                args.Player.SendErrorMessage("没有接受的支线任务");
                                return;
                            }
                            if (Db.QueryData(args.Player.Name).RegionalTaskUser == null)
                            {
                                args.Player.SendErrorMessage("没有接受的支线任务");
                                return;
                            }
                            //获取支线任务
                            var _DBData = Db.QueryData(args.Player.Name);
                            var _RegionalTask = Model.RegionalTaskLists.Where(x => x.Name == Db.QueryData(args.Player.Name).RegionalTaskUser).FirstOrDefault();
                            if(Tasklogic(args, _RegionalTask))
                            {
                                _DBData.RegionalTaskUser = null;
                                _DBData.RegionalTaskData = 0;
                                _DBData.RegionalCompleted += 1;
                                Db.UPData(_DBData);
                                args.Player.SendErrorMessage("支线任务 完成 : 任务名" + _RegionalTask.Name + " ID:" + _RegionalTask.ID);

                            }

                        }
                        break;

                }
                
                
            }
            
            //任务判定与任务类型 奖励给予等逻辑核心
            private bool Tasklogic(CommandArgs args ,_TaskList Tasklist)
            {
                foreach (var item in Tasklist.Conditions)
                {
                    switch (item.TaskType)
                    {
                        //任务类型0 消耗物品完成任务
                        case "0":
                            {
                                var _item = item.Condition.Split(',');
                                for (int i = 10; i < 49; i++) //背包从第二排开始到最后一个背包位置
                                {
                                    if (args.TPlayer.inventory[i].netID == Convert.ToInt32(_item[0]))
                                    {
                                        if (args.TPlayer.inventory[i].stack >= Convert.ToInt32(_item[1]))
                                        {
                                            //  args.TPlayer.inventory[i].stack -= Convert.ToInt32(_item[1]);
                                            break;
                                        }
                                        else
                                        {
                                            args.Player.SendErrorMessage("没有足够的物品");
                                            return false;
                                        }
                                    }

                                }
                                Item _item1 = new Item();
                                _item1.netID = Convert.ToInt32(_item[0]);
                                args.Player.SendErrorMessage("您的背包中没有任务物品"+_item1.Name );
                                args.Player.SendInfoMessage("任务物品请不要放在背包最上排中");
                                return false;

                            }
                            break;
                        //任务类型1 背包中是否有指定物品
                        case "1":
                            {
                                for (int i = 0; i < 49; i++) //背包从第二排开始到最后一个背包位置
                                {
                                    if (args.TPlayer.inventory[i].netID.ToString() == item.Condition)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        args.Player.SendErrorMessage("您的背包中没有任务物品：" + Main.item[int.Parse(item.Condition)].Name + " " + item.Condition);
                                        return false;
                                    }
                                }

                            }
                            break;
                        //任务类型2 是否击杀指定NPC //测试完成
                        case "2":
                            {
                                var _item = item.Condition.Split(',');
                                if (Db.QueryData(args.Player.Name).MianTaskData >= int.Parse(_item[1]) | Db.QueryData(args.Player.Name).RegionalTaskData >= int.Parse(_item[1]))
                                {
                                    continue;
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("您没有击杀指定NPC或击杀数量不足(" + Main.npc[int.Parse(_item[0])].FullName + ") ：任务数量" + int.Parse(_item[1]) + " 实际数量" + Db.QueryData(args.Player.Name).MianTaskData);
                                    return false;
                                }
                            }
                        //任务类型3 到达指定地图区域   //测试完成
                        case "3":
                            {
                                var Coordinate = item.Condition.Split(',');
                                int x = int.Parse(Coordinate[0]);
                                int y = int.Parse(Coordinate[1]);
                                int deviation = int.Parse(Coordinate[2]);
                                if (args.Player.X/16 >= (x - deviation) && args.Player.X/16 <= (x + deviation) && args.Player.Y/16 >= (y - deviation) && args.Player.Y/16 <= (y + deviation))
                                {
                                    continue;
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("您没有到达指定地图区域：您的坐标" + args.Player.X/16 + "," + args.Player.Y/16 + " 指定区域" + x + "," + y + " 允许偏差" + deviation );
                                    return false;
                                }
                            }
                        //穿戴或拿起指定装备
                        case "4":
                            {
                                for (int i = 50; i < 99; i++)
                                {
                                    if (args.TPlayer.inventory[i].netID.ToString() == item.Condition)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        args.Player.SendErrorMessage("您未穿戴或拿起指定装备：" + Main.item[int.Parse(item.Condition)].Name + " " + item.Condition);
                                        return false;
                                    }
                                }
                            }
                            break;
                        //拥有指定buff
                        case "5":
                            {
                                if (args.TPlayer.buffType.Contains(int.Parse(item.Condition)))
                                {
                                    continue;
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("您没有拥有指定buff： BUFF ID:" + item.Condition);
                                    return false;
                                }
                            }


                        default:
                            {
                                args.Player.SendErrorMessage("请确认配置文件中 TaskType 值是否正确");
                                return false;

                            }
                    }
                }
                //消耗任务物品
                var _itemTask = Tasklist.Conditions.Where(x => x.TaskType == "0");
                foreach (var item in _itemTask)
                {
                    var _item = item.Condition.Split(',');
                    for (int i = 10; i < 49; i++) //背包从第二排开始到最后一个背包位置
                    {
                        if (args.TPlayer.inventory[i].netID == Convert.ToInt32(_item[0]))
                        {
                            if (args.TPlayer.inventory[i].stack >= Convert.ToInt32(_item[1]))
                            {
                                var stack = args.TPlayer.inventory[i].stack -= Convert.ToInt32(_item[1]);
                                PlayItemSet(args.Player.Index, i, Convert.ToInt32(_item[0]), stack);
                                break;
                            }
                            else
                            {
                                args.Player.SendErrorMessage("没有足够的物品");
                                return false;
                            }
                        }
                    }
                }
                //给与奖励

                //给与奖励物品
                foreach (var item in Tasklist.Reward)
                {
                    Commands.HandleCommand(TSPlayer.Server, item.Replace("{name}", args.Player.Name));
                }

                return true;

            }
           
            private void _TaskSystemGive(CommandArgs args)
            {
                //放弃支线任务
                if (!Db.Queryuser(args.Player.Name))
                {
                    args.Player.SendErrorMessage("您没有接受支线任务");
                    return;
                }
                if (Db.QueryData(args.Player.Name).RegionalTaskUser == null)
                {
                    args.Player.SendErrorMessage("您没有接受支线任务");
                    return;
                }
                if (!Model.RandomTasks.Exists(x=>x.Name==args.Player.Name))
                {
                    args.Player.SendErrorMessage("支线任务信息未刷新 请先刷新支线任务");
                    return;
                }
                DateTime now = DateTime.Now;
                if (!Model.RandomTasks.Exists(x=>x.GiveTime==null))
                {
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime = now;
                    DBData dBData = Db.QueryData(args.Player.Name);
                    dBData.RegionalTaskUser = null;
                    dBData.RegionalTaskData = 0;
                    Db.UPData(dBData);
                    args.Player.SendWarningMessage("您已放弃任务，CD时间已更新");
                    return;
                }
                if (Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime.AddSeconds(10) < now)
                {
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime = now;
                    DBData dBData = Db.QueryData(args.Player.Name);
                    dBData.RegionalTaskUser = null;
                    dBData.RegionalTaskData = 0;
                    Db.UPData(dBData);
                    args.Player.SendWarningMessage("您已放弃任务，CD时间已更新");
                    return;

                }
                else
                {
                    TimeSpan ts = Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime.AddSeconds(10) - now;
                    args.Player.SendWarningMessage("放弃任务冷却中 上次放弃时间" + Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime + "剩余 CD时间 " + ts.Seconds + "秒");
                    return;
                }

            }

            private void _TaskSystemf5(CommandArgs args)
            {
                if (!Model.RandomTasks.Exists(t => t.Name == args.Player.Name)) //刷新任务
                {
                    var Random = getRadom(5, 1, Model.RegionalTaskLists.Count());
                    Model.RandomTasks.Add(new RandomTask() { Name = args.Player.Name, Reward = Random, F5Time = DateTime.Now});
                    args.Player.SendErrorMessage("任务已刷新");
                    foreach (int item in Random)
                    {
                        args.Player.SendErrorMessage("任务ID" + item + " 任务名称" + Model.RegionalTaskLists.Find(t => t.ID == item).Name + " 任务信息" + Model.RegionalTaskLists.Find(t => t.ID == item).Info );
                       
                    }
                    return;
                }
                DateTime Date = DateTime.Now;
                if (Model.RandomTasks.Find(t => t.Name == args.Player.Name).F5Time.AddSeconds(10) < Date)
                {
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).F5Time = Date;
                    var Random = getRadom(5, 1, Model.RegionalTaskLists.Count());
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).Reward = Random;
                    args.Player.SendErrorMessage("任务已刷新");
                    foreach (int item in Random)
                    {
                        args.Player.SendErrorMessage("任务ID" + item + " 任务名称" + Model.RegionalTaskLists.Find(t => t.ID == item).Name + " 任务信息" + Model.RegionalTaskLists.Find(t => t.ID == item).Info);
                    }
                    return;
                }
                else
                {

                    //剩余时间
                    TimeSpan ts = Model.RandomTasks.Find(t => t.Name == args.Player.Name).F5Time.AddSeconds(10) - Date;
                    args.Player.SendErrorMessage("任务刷新时间冷却中,剩余时间" + ts.Seconds + "秒");
                    return;

                }
                
                
            }

            private void _TaskSystemPick(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("/接受任务 主线 接受主线任务");
                    args.Player.SendErrorMessage("/接受任务 支线 查看可接受支线任务");
                    args.Player.SendErrorMessage("/接受任务 支线 [任务ID] 接受支线任务");
                    return;
                }
                switch (args.Parameters[0])
                {
                    case "主线":
                        {
                            //接受主线任务
                            if (args.Parameters.Count == 1)
                            {
                                if (Model.MainTaskLists.Count() == 0)
                                {
                                    args.Player.SendErrorMessage("没有主线任务");
                                    return;
                                }
                                //接受任务
                                try
                                {
                                    if (!Db.Queryuser(args.Player.Name))
                                    {
                                        Db.Adduser(new DBData() { UserName = args.Player.Name, MianTaskUser = Model.MainTaskLists[0].Name, MianTaskData = 0, MianTaskCompleted = 0, RegionalTaskUser = null, RegionalTaskData = 0, RegionalCompleted = 0 });
                                        args.Player.SendErrorMessage("当前接受主线任务为" + Model.MainTaskLists[0].Name + " 任务ID:"+Model.MainTaskLists[0].ID);
                                        args.Player.SendErrorMessage("任务信息:" + Model.MainTaskLists[0].Info);
                                        return;
                                    }                                    
                                    DBData dBData = Db.QueryData(args.Player.Name);
                                    if (dBData.UserName != null)
                                    {
                                        args.Player.SendErrorMessage("您已接受任务" + dBData.MianTaskUser +" 任务ID: "+Model.MainTaskLists[0].ID + " 不能再次领取任务");
                                        args.Player.SendErrorMessage("任务信息:" + Model.MainTaskLists[dBData.RegionalCompleted].Info);
                                        return;
                                    }
                                    dBData.MianTaskUser = Model.MainTaskLists[dBData.MianTaskCompleted].Name;
                                    dBData.MianTaskData = 0;
                                    Db.UPData(dBData);
                                    args.Player.SendErrorMessage("当前接受主线任务为" + Model.MainTaskLists[dBData.MianTaskCompleted].Name +" 任务ID:"+ Model.MainTaskLists[dBData.MianTaskCompleted].ID);
                                    args.Player.SendErrorMessage("任务信息:" + Model.MainTaskLists[dBData.MianTaskCompleted].Info);

                                    return;

                                }
                                catch (Exception ex)
                                {
                                    args.Player.SendErrorMessage("任务接受失败");
                                    TShock.Log.ConsoleError("[POBCTask] 任务接受失败!\n{0}".SFormat(ex.ToString()));
                                    throw;
                                }
                            }
                            break;
                        }
                    case "支线":
                        {
                            if (Model.MainTaskLists.Count() == 0)
                            {
                                args.Player.SendErrorMessage("没有支线任务");
                                return;
                            }
                            if (args.Parameters.Count == 1)
                            {
                                if (!Model.RandomTasks.Exists(t => t.Name == args.Player.Name)) //刷新任务
                                {
                                    //var Random = getRadom(5, 0, Model.RegionalTaskLists.Count());
                                    //Model.RandomTasks.Add(new RandomTask() { Name = args.Player.Name, Reward = Random, F5Time = DateTime.Now });
                                    args.Player.SendErrorMessage("随机任务未刷新，请先刷新随机任务列表");
                                    return;
                                }
                                args.Player.SendErrorMessage("当前可接受支线任务为:");
                                foreach (int item in Model.RandomTasks.Find(x=>x.Name==args.Player.Name).Reward)
                                {
                                    args.Player.SendErrorMessage("任务ID" + item + " 任务名称" + Model.RegionalTaskLists.Find(t => t.ID == item).Name + " 任务信息" + Model.RegionalTaskLists.Find(t => t.ID == item).Info);
                                   
                                }
                                return;
                            }
                            //接受支线任务
                            if (args.Parameters.Count == 2)
                            {
                                if (!int.TryParse(args.Parameters[1], out int tmp))
                                {
                                    args.Player.SendErrorMessage("请输入正确的任务ID");
                                    return;
                                }

                                //接受任务
                                try
                                {
                                    int taskid = int.Parse(args.Parameters[1]);
                                    if (!Model.RandomTasks.Find(t => t.Name == args.Player.Name).Reward.Contains(taskid))
                                    {
                                        args.Player.SendErrorMessage("请输入正确的任务ID");
                                        return;
                                    }
                                    //查询数据库没有用户时添加用户
                                    if (!Db.Queryuser(args.Player.Name))
                                    {
                                        Db.Adduser(new DBData() { UserName = args.Player.Name, MianTaskUser = null, MianTaskData = 0, MianTaskCompleted = 0, RegionalTaskUser = Model.RegionalTaskLists.Find(t => t.ID == taskid).Name, RegionalTaskData = 0, RegionalCompleted = 0 });
                                        args.Player.SendInfoMessage("当前接受支线任务为" + Model.RegionalTaskLists.Find(t => t.ID == taskid).Name);
                                        args.Player.SendInfoMessage("任务信息:" + Model.RegionalTaskLists.Find(t => t.ID == taskid).Info);
                                        return;
                                    }
                                    DBData dBData = Db.QueryData(args.Player.Name);
                                    if (dBData.RegionalTaskUser != null)
                                    {
                                        args.Player.SendInfoMessage("您已接受任务" + dBData.RegionalTaskUser + "不能再次领取任务");

                                         args.Player.SendInfoMessage("任务信息:" + Model.RegionalTaskLists.Find(t => t.ID == taskid).Info);
   
                                        
                                        return;
                                    }
                                    dBData.RegionalTaskUser = Model.RegionalTaskLists.Find(t => t.ID == taskid).Name;
                                    dBData.RegionalTaskData = 0;
                                    Db.UPData(dBData);
                                    args.Player.SendInfoMessage("当前接受支线任务为" + Model.RegionalTaskLists.Find(t => t.ID == taskid).Name);
                                    args.Player.SendInfoMessage("任务信息:" + Model.RegionalTaskLists.Find(t => t.ID == taskid).Info);
                                    return;

                                }
                                catch (Exception ex)
                                {
                                    args.Player.SendErrorMessage("任务接受失败");
                                    TShock.Log.ConsoleError("[POBCTask] 任务接受失败!\n{0}".SFormat(ex.ToString()));
                                    throw;
                                }

                            }
                            else
                            {
                                args.Player.SendErrorMessage("语法错误 正确语法 /接受任务 支线 <任务ID>");
                            }
                            break;

                        }

                }
            }

            /// <summary>
            /// 产生随机数
            /// </summary>
            /// <param name="len">产生随机数的个数</param>
            /// <param name="min">随机数的下界</param>
            /// <param name="max">随机数的上界</param>
            /// <returns></returns>
            private ArrayList getRadom(int len, int min, int max)
            {
                Random r = new Random();
                ArrayList al = new ArrayList();
                for (int i = 0; i < len; i++)
                {
                    al.Add(r.Next(min, max));
                }
                return al;
            }




            private void _TaskSystemList(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("/任务列表 主线 [页数] 或者 /task list main [page]显示主线任务 \r\n/任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
                    return;
                }
                switch (args.Parameters[0])
                {
                    case "主线":
                        {
                            if (Model.MainTaskLists.Count() == 0)
                            {
                                args.Player.SendErrorMessage("没有主线任务");
                                return;
                            }
                            // 5项一页显示 Model.MainTaskLists
                            int page = 1;
                            if (args.Parameters.Count > 1)
                            {
                                if (!int.TryParse(args.Parameters[1], out page))
                                {
                                    args.Player.SendErrorMessage("/任务列表 主线 [页数] 或者 /task list main [page]显示主线任务 \r\n/任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
                                    return;
                                }
                            }
                            int maxPage = (int)Math.Ceiling(Model.MainTaskLists.Count() / 5.0);
                            if (page < 1)
                            {
                                page = 1;
                            }
                            if (page > maxPage)
                            {
                                page = maxPage;
                            }
                            args.Player.SendInfoMessage("主线任务列表 第" + page + "页 共" + maxPage + "页");
                            for (int i = (page - 1) * 5; i < page * 5; i++)
                            {
                                if (i >= Model.MainTaskLists.Count())
                                {
                                    break;
                                }
                                args.Player.SendInfoMessage("任务ID " + Model.MainTaskLists[i].ID +" 任务名: " + Model.MainTaskLists[i].Name + " 简略描述: " + Model.MainTaskLists[i].Info);
                            }
                            break;
                        }
                    case "支线":
                        {
                            if (Model.RegionalTaskLists.Count() == 0)
                            {
                                args.Player.SendErrorMessage("没有支线任务");
                                return;
                            }
                            //5项一页显示 Model.RegionalTaskLists
                            int page = 1;
                            if (args.Parameters.Count > 1)
                            {
                                if (!int.TryParse(args.Parameters[1], out page))
                                {
                                    args.Player.SendErrorMessage("/任务列表 主线 [页数] 或者 /task list main [page]显示主线任务 \r\n/任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
                                    return;
                                }
                            }
                            int maxPage = (int)Math.Ceiling(Model.RegionalTaskLists.Count() / 5.0);
                            if (page < 1)
                            {
                                page = 1;
                            }
                            if (page > maxPage)
                            {
                                page = maxPage;
                            }
                            args.Player.SendInfoMessage("支线任务列表 第" + page + "页 共" + maxPage + "页");
                            for (int i = (page - 1) * 5; i < page * 5; i++)
                            {
                                if (i >= Model.RegionalTaskLists.Count())
                                {
                                    break;
                                }
                                args.Player.SendInfoMessage("任务ID: " + Model.RegionalTaskLists[i].ID +" 任务名: " + Model.RegionalTaskLists[i].Name + " 简略描述: " + Model.RegionalTaskLists[i].Info );
                            }
                            break;
                            


                        }
                    default:
                        { args.Player.SendErrorMessage("语法错误，正确语法：/任务列表 主线 [页数] 显示主线任务 \r\n/任务列表 支线 [页数] 显示支线任务"); }
                        break;
                }
   



                }

            private void _TaskSystem(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("命令如下 /任务列表 /接受任务 /刷新任务 /完成任务 /放弃任务");
                    return;
                }

            }
 

            /// <summary>
            /// Handles plugin disposal logic.
            /// *Supposed* to fire when the server shuts down.
            /// You should deregister hooks and free all resources here.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                }
                base.Dispose(disposing);
            }
            public void File()
            {
                try
                {
                    MainTaskLists = TaskConfig.Read(ConfigPath).Write(ConfigPath);
                    foreach (var item in MainTaskLists.TaskList)
                    {
                        if (item.Type=="主线")
                        {
                            Model.MainTaskLists.Add(item);
                        }
                    }
                    foreach (var item in MainTaskLists.TaskList)
                    {
                        if (item.Type == "支线")
                        {
                            Model.RegionalTaskLists.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainTaskLists = new TaskConfig();
                    TShock.Log.ConsoleError("[POBCTask] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
                }


            }



                // 给与玩家物品
                public void PlayItemSet(int ID, int slot, int Item, int stack)//ID 玩家ID，slot 格子ID，Item 物品ID，stack 物品堆叠
            {
                TSPlayer player = new TSPlayer(ID);
                int index;
                Item item = TShock.Utils.GetItemById(Item);
                item.stack = stack;
                //Inventory slots
                if (slot < NetItem.InventorySlots)
                {
                    index = slot;
                    player.TPlayer.inventory[slot] = item;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.inventory[index].Name), player.Index, slot, player.TPlayer.inventory[index].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.inventory[index].Name), player.Index, slot, player.TPlayer.inventory[index].prefix);
                }

                //Armor & Accessory slots
                else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots)
                {
                    index = slot - NetItem.InventorySlots;
                    player.TPlayer.armor[index] = item;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.armor[index].Name), player.Index, slot, player.TPlayer.armor[index].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.armor[index].Name), player.Index, slot, player.TPlayer.armor[index].prefix);
                }

                //Dye slots
                else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
                {
                    index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots);
                    player.TPlayer.dye[index] = item;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.dye[index].Name), player.Index, slot, player.TPlayer.dye[index].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.dye[index].Name), player.Index, slot, player.TPlayer.dye[index].prefix);
                }

                //Misc Equipment slots
                else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
                {
                    index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                    player.TPlayer.miscEquips[index] = item;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.miscEquips[index].Name), player.Index, slot, player.TPlayer.miscEquips[index].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.miscEquips[index].Name), player.Index, slot, player.TPlayer.miscEquips[index].prefix);
                }

                //Misc Dyes slots
                else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
                {
                    index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                    player.TPlayer.miscDyes[index] = item;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.miscDyes[index].Name), player.Index, slot, player.TPlayer.miscDyes[index].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.miscDyes[index].Name), player.Index, slot, player.TPlayer.miscDyes[index].prefix);
                }
            }

            public int Stack(int userid)
            {
                int x = -1;
                for (int i = 0; i < 50; i++)
                {
                    if (TShock.Players[userid].TPlayer.inventory[i].netID == 0)
                    {
                        x = i;
                        break;
                    }
                    else
                    {
                        x = -1;
                    }
                }
                return x;
            }


        }
    }
}
