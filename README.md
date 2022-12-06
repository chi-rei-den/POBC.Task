# POBC.Task
#POBC配套任务系统  
##进度： 

接受任务 完成  
刷新任务 完成  
放弃任务 完成  
完成任务 完成  
管理权限 ing...  
重载任务系统. ing...  
配置文件 格式检查ing.. 
冷却时间及执行随机任务个数配置文件添加设定ing..  

  
任务总类添加5种  
补充NPC击杀代码...完成  
  
- 更新了任务逻辑核心  
- 部分BUG修复（支线获取后删除在随机列表 物品获取错误修复 任务判断条件修复）  
- 任务核心插件代码测试完成  
- 支线任务获取后去重复  

  
  
# 权限及命令  

TaskSystem.user (/任务 /任务列表 /查询任务 /接受任务 /刷新任务 /完成任务 /放弃任务)  
  
  
#配置文件  
~~~Json
{  
  "TaskList": [  
    {  
      "ID": 1,  //注意ID 不能重复  
      "Type": "主线",   //区分主线及支线任务   主线任务不能放弃 单进程进行  
      "Name": "消耗物品 xxx 1个",  //任务名字  
      "Info": "消耗物品1个 获取经验",   //任务简要  
      "DetailedInfo": "消耗物品 xxx 1个 获得经验xxx",  //任务详细说明  
      "Conditions": [                //任务条件  
        {  
          "TaskType": "0",          // 0：消耗物品完成任务 1：背包中是否有指定物品  2：是否击杀指定NPC  3：到达指定地图区域 4：穿戴或拿起指定装备 5：拥有指定buff（第五个让任务条件暂时未测试 其他测试完成）  
          "Condition": "3507,1"  //任务条件 详细看下面列子 0条件时 3507代表物品id 1代表消耗几个 注意装备等不能堆叠时填写 1  
        }  
      ],  
      "Reward": [          //任务完成后执行命令  
        "/BC 这是服务器公告",  
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "  
      ]  
    },  
    {  
      "ID": 2,  
      "Type": "主线",  
      "Name": "拥有物品 xxx",  
      "Info": "任务条件拥有xxx物品1个",  
      "DetailedInfo": "拥有 xxx 完成任务获得经验xxx",  
      "Conditions": [  
        {  
          "TaskType": "1",                  //1条件时 3507代表物品id 此条件不判定数量  
          "Condition": "3507"  
        }  
      ],  
      "Reward": [  
        "/BC 这是服务器公告",  
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "  
      ]  
    },  
    {  
      "ID": 3,  
      "Type": "主线",  
      "Name": "击杀 xxx 1次",  
      "Info": "击杀xxx 1次 奖励 xxxx",  
      "DetailedInfo": "击杀xxx 1次后 完成任务获得经验xxx",  
      "Conditions": [  
        {  
          "TaskType": "2",         //2条件时  -3 代表NPC id   3 代表击杀数量    //其他条件能在单任务多次复用 单次条件单任务中 只能出现一次  
          "Condition": "-3,3"  
        }  
      ],  
      "Reward": [
        "/BC 这是服务器公告",
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "
      ]
    },
    {
      "ID": 4,
      "Type": "主线",
      "Name": "地图坐标到达",
      "Info": "到达地图坐标 xxx，yyy ",
      "DetailedInfo": "到达地图坐标 xxx，yyy  完成任务获得经验xxx",
      "Conditions": [
        {
          "TaskType": "3",                  //3条件时 500，500 代表地图X Y坐标  50代表允许便宜坐标值
          "Condition": "500,500,50"
        }
      ],
      "Reward": [
        "/BC 这是服务器公告",
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "
      ]
    },
    {
    ......
    },
    {
      "ID": 11,
      "Type": "支线",
      "Name": "拿起同志短剑",
      "Info": "拿起同志短剑在鼠标中 ",
      "DetailedInfo": "拿起同志短剑在鼠标中 完成任务获得经验xxx",
      "Conditions": [
        {
          "TaskType": "4",                   //4条件时 3507 代表装备或者鼠标拿起的物品id
          "Condition": "3507"
        }
      ],
      "Reward": [
        "/BC 这是服务器公告",
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "
      ]
    },
    {
      "ID": 12,
      "Type": "支线",
      "Name": "拿起同志短剑",
      "Info": "拿起同志短剑在鼠标中 ",
      "DetailedInfo": "拿起同志短剑在鼠标中 完成任务获得经验xxx",
      "Conditions": [
        {
          "TaskType": "4",
          "Condition": "3507"
        }
      ],
      "Reward": [
        "/BC 这是服务器公告",
        "/BC /xxx {name}  - 获取经验命令{name}不用更改将会自动替换为角色名 "
      ]
    }
  ]
}
~~~

![image](https://user-images.githubusercontent.com/19232925/175821708-7ac1d946-5715-4cd9-9551-c9897189f335.png)  
![image](https://user-images.githubusercontent.com/19232925/175821730-f6e871ad-3ebd-45ce-8454-8ee856bc4908.png)  
![image](https://user-images.githubusercontent.com/19232925/175821772-6f8a6057-3ad6-417c-9fcc-6962008f2855.png)  
![image](https://user-images.githubusercontent.com/19232925/175821799-86029f8a-f62e-473a-9703-439ac427175f.png)  
![image](https://user-images.githubusercontent.com/19232925/175821829-c43b94d2-dfd6-4731-95f8-28fe9dcc6110.png)  







