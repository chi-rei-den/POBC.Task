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
                ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
                File();
            }

            private void OnInitialize(EventArgs args)
            {
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystem, "/task","/任务","任务系统说明")
                {
                    HelpText = " POBC 任务系统"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemList, "/tasklist","/任务列表", "任务列表")
                {
                    HelpText = " POBC 任务系统 所有任务"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemPick, "/tasklist", "/接收任务")
                {
                    HelpText = "接受任务时 随机5项任务选择1项 可以接受单个任务，为了提高任务的难度放弃任务和刷新可接受任务有CD时间 /n 主线任务默认接取"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemf5, "/taskf5", "/刷新任务")
                {
                    HelpText = "刷新随机支线任务可接取列表，注意刷新时间冷却 .主线任务不可刷新"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemGive, "/taskgive", "/放弃任务")
                {
                    HelpText = "放弃任务时 ，为了提高任务的难度放弃任务和刷新可接受任务有CD时间 /n 主线任务不可放弃"
                });
                Commands.ChatCommands.Add(new Command("TaskSystem.user", _TaskSystemOver, "/taskover", "/完成任务")
                {
                    HelpText = "完成任务 ， 并获取奖励"
                });
            }

            private void _TaskSystemOver(CommandArgs args)
            {
                //完成任务
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("/完成任务 主线 （/完成任务 支线） 完成任务");
                    return;
                }
                switch (args.Parameters[0])
                {
                    case "主线":
                        if (Model.MainTaskLists.Count == 0)
                        {
                            args.Player.SendErrorMessage("没有主线任务");
                            return;
                        }
                        if (!Db.Queryuser(args.Player.Name))
                        {
                            args.Player.SendErrorMessage("没有接受的主线任务");
                        }
                        if (Db.QueryData(args.Player.Name).MianTaskUser == null)
                        {
                            args.Player.SendErrorMessage("没有接受的主线任务");
                        }
                        //获取主线任务
                        var _MainTask = Model.MainTaskLists.Where(x => x.Name == Db.QueryData(args.Player.Name).MianTaskUser).FirstOrDefault();
                        //检测是否完成任务
                        foreach (var item in _MainTask.Conditions)
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
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                args.Player.SendErrorMessage("您的背包中没有任务物品：" + Main.item[int.Parse(item.Condition)].Name + " " + item.Condition);
                                                args.Player.SendInfoMessage("任务物品请不要放在背包最上排中");
                                                return;
                                            }
                                        }

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
                                                return;
                                            }
                                        }

                                    }
                                    break;
                                //任务类型2 是否击杀指定NPC
                                case "2":
                                    {
                                        if (int.Parse(Db.QueryData(args.Player.Name).MianTaskData) >= int.Parse(item.Condition))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            args.Player.SendErrorMessage("您没有击杀指定NPC或击杀数量不足：任务数量" + item.Condition + " 实际数量" + Db.QueryData(args.Player.Name).MianTaskData);
                                            return;
                                        }
                                    }
                                //任务类型3 到达指定地图区域
                                case "3":
                                    {
                                        var Coordinate = item.Condition.Split(',');
                                        int x = int.Parse(Coordinate[0]);
                                        int y = int.Parse(Coordinate[1]);
                                        int deviation = int.Parse(Coordinate[2]);
                                        if (args.Player.X >= (x - deviation) && args.Player.X <= (x + deviation) && args.Player.Y >= (y - deviation) && args.Player.Y <= (y + deviation))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            args.Player.SendErrorMessage("您没有到达指定地图区域：您的坐标" + args.Player.X + "," + args.Player.Y + " 指定区域" + x + "," + y + " 允许偏差" + deviation);
                                            return;
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
                                                return;
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
                                            return;
                                        }
                                    }
                                default:
                                    {
                                        args.Player.SendErrorMessage("请确认配置文件中 TaskType 值是否正确");
                                        return;

                                    }
                            }
                        }
                        //消耗任务物品
                        var _itemTask = _MainTask.Conditions.Where(x => x.TaskType == "0");
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
                                        return;
                                    }
                                }
                            }
                        }

                        //给与奖励物品
                        foreach (var item in _MainTask.Reward)
                        {
                            Commands.HandleCommand(TSPlayer.Server, item.Replace("{name}", args.Player.Name));
                        }

                        break;        
                }
                

            } 


            private void _TaskSystemGive(CommandArgs args)
            {
                //放弃支线任务
                if (!Db.Queryuser(args.Player.Name))
                {
                    args.Player.SendErrorMessage("您没有接受支线任务");
                }
                if (Db.QueryData(args.Player.Name).RegionalTaskUser == null)
                {
                    args.Player.SendErrorMessage("您没有接受支线任务");
                }
                DateTime now = DateTime.Now;
                if(Model.RandomTasks.Find(t=>t.Name==args.Player.Name).GiveTime.AddSeconds(10)<now)
                {
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime = now;
                    DBData dBData = Db.QueryData(args.Player.Name);
                    dBData.RegionalTaskUser = null;
                    dBData.RegionalTaskData = null;
                    Db.UPData(dBData);
                    args.Player.SendWarningMessage("您已放弃任务，CD时间已更新");

                }
                else
                {
                    args.Player.SendWarningMessage("放弃任务冷却中 上次放弃时间"+ Model.RandomTasks.Find(t => t.Name == args.Player.Name).GiveTime + "CD时间 10S");
                }

            }

            private void _TaskSystemf5(CommandArgs args)
            {
                if (!Model.RandomTasks.Exists(t => t.Name == args.Player.Name)) //刷新任务
                {
                    var Random = getRadom(5, 0, Model.RegionalTaskLists.Count());
                    Model.RandomTasks.Add(new RandomTask() { Name = args.Player.Name, Reward = Random, F5Time = DateTime.Now});
                    args.Player.SendErrorMessage("任务已刷新");
                    foreach (int item in Random)
                    {
                        args.Player.SendErrorMessage("任务ID" + item + " 任务名称" + Model.RegionalTaskLists.Find(t => t.ID == item).Name + " 任务信息" + Model.RegionalTaskLists.Find(t => t.ID == item).Info );
                    }
                }
                DateTime Date = DateTime.Now;
                if (Model.RandomTasks.Find(t => t.Name == args.Player.Name).F5Time.AddSeconds(10) < Date)
                {
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).F5Time = Date;
                    var Random = getRadom(5, 0, Model.RegionalTaskLists.Count());
                    Model.RandomTasks.Find(t => t.Name == args.Player.Name).Reward = Random;
                    args.Player.SendErrorMessage("任务已刷新");
                    foreach (int item in Random)
                    {
                        args.Player.SendErrorMessage("任务ID" + item + " 任务名称" + Model.RegionalTaskLists.Find(t => t.ID == item).Name + " 任务信息" + Model.RegionalTaskLists.Find(t => t.ID == item).Info);
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("任务刷新时间冷却中");
                }
                
                
            }

            private void _TaskSystemPick(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("/接收任务 主线 接受主线任务");
                    args.Player.SendErrorMessage("/接收任务 支线 [任务ID] 接受支线任务");
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
                                        Db.Adduser(new DBData() { UserName = args.Player.Name, MianTaskUser = Model.MainTaskLists[0].Name, MianTaskData = null, MianTaskCompleted = 0, RegionalTaskUser = null, RegionalTaskData = null, RegionalCompleted = 0 });
                                        args.Player.SendErrorMessage("当前接受主线任务为" + Model.MainTaskLists[0].Name);
                                        args.Player.SendErrorMessage("任务信息:" + Model.MainTaskLists[0].Info);
                                    }                                    
                                    DBData dBData = Db.QueryData(args.Player.Name);
                                    if (dBData.UserName != null)
                                    {
                                        args.Player.SendErrorMessage("您已接受任务" + dBData.MianTaskUser + "不能再次领取任务");
                                        args.Player.SendErrorMessage("任务信息:" + Model.MainTaskLists[dBData.RegionalCompleted].Info);
                                        return;
                                    }


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

                            //接受支线任务
                            if (args.Parameters.Count == 2)
                            {
                                if (!int.TryParse(args.Parameters[2], out int tmp))
                                {
                                    args.Player.SendErrorMessage("请输入正确的任务ID");
                                    return;
                                }
                                if (Model.MainTaskLists.Count() == 0)
                                {
                                    args.Player.SendErrorMessage("没有支线任务");
                                    return;
                                }
                                //接受任务
                                try
                                {
                                    if (!Model.RandomTasks.Exists(t => t.Name == args.Player.Name)) //刷新任务
                                    {
                                        var Random = getRadom(5, 0, Model.RegionalTaskLists.Count());
                                        Model.RandomTasks.Add(new RandomTask() { Name = args.Player.Name, Reward = Random, F5Time = DateTime.Now });
                                    }
                                    //查询数据库没有用户时添加用户
                                    if (!Db.Queryuser(args.Player.Name))
                                    {
                                        Db.Adduser(new DBData() { UserName = args.Player.Name, MianTaskUser = null, MianTaskData = null, MianTaskCompleted = 0, RegionalTaskUser = Model.RegionalTaskLists.Find(t => t.ID==int.Parse(args.Parameters[2])).Name, RegionalTaskData = null, RegionalCompleted = 0 });
                                        args.Player.SendErrorMessage("当前接受支线任务为" + Model.RegionalTaskLists.Find(t => t.ID == int.Parse(args.Parameters[2])).Name);
                                        args.Player.SendErrorMessage("任务信息:" + Model.RegionalTaskLists.Find(t => t.ID == int.Parse(args.Parameters[2])).Info);
                                        return;
                                    }
                                    DBData dBData = Db.QueryData(args.Player.Name);
                                    if (dBData.RegionalTaskUser != null)
                                    {
                                        args.Player.SendErrorMessage("您已接受任务" + dBData.RegionalTaskUser + "不能再次领取任务");

                                         args.Player.SendErrorMessage("任务信息:" + Model.RegionalTaskLists.Find(t => t.ID == int.Parse(args.Parameters[2])).Info);
   
                                        
                                        return;
                                    }


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
                    args.Player.SendErrorMessage("/任务列表 主线 [page] 或者 /task list main [page]显示主线任务 /n /任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
                    return;
                }
                switch (args.Parameters[0])
                {
                    case "主线":
                        {
                            // 5项一页显示 Model.MainTaskLists
                            int page = 1;
                            if (args.Parameters.Count > 1)
                            {
                                if (!int.TryParse(args.Parameters[1], out page))
                                {
                                    args.Player.SendErrorMessage("/任务列表 主线 [page] 或者 /task list main [page]显示主线任务 /n /任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
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
                                args.Player.SendInfoMessage("任务" + Model.MainTaskLists[i].Name + " 奖励" + Model.MainTaskLists[i].Reward);
                            }
                            break;
                        }
                    case "支线":
                        {
                            //5项一页显示 Model.RegionalTaskLists
                            int page = 1;
                            if (args.Parameters.Count > 1)
                            {
                                if (!int.TryParse(args.Parameters[1], out page))
                                {
                                    args.Player.SendErrorMessage("/任务列表 主线 [page] 或者 /task list main [page]显示主线任务 /n /任务列表 支线 [页数]或者 /task list Regional [page]显示支线任务");
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
                                args.Player.SendInfoMessage("任务" + Model.RegionalTaskLists[i].Name + " 奖励" + Model.RegionalTaskLists[i].Reward);
                            }
                            break;
                            


                        }
                    default:
                        { args.Player.SendErrorMessage("/任务列表 主线 [page] 显示主线任务 /n /任务列表 支线 [页数] 显示支线任务"); }
                        break;
                }
   



                }

            private void _TaskSystem(CommandArgs args)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("命令如下\r\n/任务列表(10项一页).\r\n /接受任务 -接受任务 [任务ID]\r\n /task re  -刷新任务列表（1小时1次）\r\n/task end  -完成任务\r\n /task over -放弃任务（CD 1小时）\r\n");
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
