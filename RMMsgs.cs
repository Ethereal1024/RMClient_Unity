using ProtoBuf;

[ProtoContract]
public class RMMsgs {
    // 1. RemoteControl - 自定义客户端->服务器 (2.2.1)
    [ProtoContract]
    public class RemoteControlData {
        [ProtoMember(1)] public int mouse_x;                    // 鼠标x轴移动速度
        [ProtoMember(2)] public int mouse_y;                    // 鼠标y轴移动速度  
        [ProtoMember(3)] public int mouse_z;                    // 鼠标滚轮移动速度
        [ProtoMember(4)] public bool left_button_down;          // 左键是否按下
        [ProtoMember(5)] public bool right_button_down;         // 右键是否按下
        [ProtoMember(6)] public uint keyboard_value;            // 键盘按键位掩码
        [ProtoMember(7)] public bool mid_button_down;           // 中键是否按下
        [ProtoMember(8)] public byte[] data;                    // 自定义数据(最大30字节)
    }

    // 2. GameStatus - 服务器->自定义客户端 (2.2.2)
    [ProtoContract]
    public class GameStatusData {
        [ProtoMember(1)] public uint current_round;             // 当前局次(从1开始)
        [ProtoMember(2)] public uint total_rounds;              // 总局数
        [ProtoMember(3)] public uint red_score;                 // 红方得分
        [ProtoMember(4)] public uint blue_score;                // 蓝方得分
        [ProtoMember(5)] public uint current_stage;             // 当前阶段
        [ProtoMember(6)] public int stage_countdown_sec;        // 当前阶段剩余时间(秒)
        [ProtoMember(7)] public int stage_elapsed_sec;          // 当前阶段已过时间(秒)
        [ProtoMember(8)] public bool is_paused;                 // 是否暂停
    }

    // 3. GlobalUnitStatus - 服务器->自定义客户端 (2.2.3)
    [ProtoContract]
    public class GlobalUnitStatusData {
        [ProtoMember(1)] public uint base_health;               // 基地当前血量
        [ProtoMember(2)] public uint base_status;               // 基地状态
        [ProtoMember(3)] public uint base_shield;               // 基地当前护盾值
        [ProtoMember(4)] public uint outpost_health;            // 前哨站当前血量
        [ProtoMember(5)] public uint outpost_status;            // 前哨站状态
        [ProtoMember(6)] public uint[] robot_health;            // 所有机器人血量数组
        [ProtoMember(7)] public int[] robot_bullets;            // 己方机器人剩余累计发弹量
        [ProtoMember(8)] public uint total_damage_red;          // 己方累计总伤害
        [ProtoMember(9)] public uint total_damage_blue;         // 对方累计总伤害
    }

    // 4. GlobalLogisticsStatus - 服务器->自定义客户端 (2.2.4)
    [ProtoContract]
    public class GlobalLogisticsStatusData {
        [ProtoMember(1)] public uint remaining_economy;         // 己方当前经济
        [ProtoMember(2)] public ulong total_economy_obtained;   // 己方累计总经济
        [ProtoMember(3)] public uint tech_level;                // 己方科技等级
        [ProtoMember(4)] public uint encryption_level;          // 己方加密等级
    }

    // 5. GlobalSpecialMechanism - 服务器->自定义客户端 (2.2.5)
    [ProtoContract]
    public class GlobalSpecialMechanismData {
        [ProtoMember(1)] public uint[] mechanism_id;            // 正在生效的机制ID列表
        [ProtoMember(2)] public int[] mechanism_time_sec;       // 对应的时间参数(秒)
    }

    // 6. Event - 服务器->自定义客户端 (2.2.6)
    [ProtoContract]
    public class EventData {
        [ProtoMember(1)] public int event_id;                   // 事件编号
        [ProtoMember(2)] public string param;                   // 事件参数
    }

    // 7. RobotInjuryStat - 服务器->自定义客户端 (2.2.7)
    [ProtoContract]
    public class RobotInjuryStatData {
        [ProtoMember(1)] public uint total_damage;              // 累计受伤总计
        [ProtoMember(2)] public uint collision_damage;          // 撞击伤害
        [ProtoMember(3)] public uint small_projectile_damage;   // 17mm弹丸伤害
        [ProtoMember(4)] public uint large_projectile_damage;   // 42mm弹丸伤害
        [ProtoMember(5)] public uint dart_splash_damage;        // 飞镖溅射伤害
        [ProtoMember(6)] public uint module_offline_damage;     // 模块离线扣血
        [ProtoMember(7)] public uint wifi_offline_damage;       // WiFi离线扣血
        [ProtoMember(8)] public uint penalty_damage;            // 判罚扣血
        [ProtoMember(9)] public uint server_kill_damage;        // 服务器强制战亡扣血
        [ProtoMember(10)] public uint killer_id;                // 击杀者ID
    }

    // 8. RobotRespawnStatus - 服务器->自定义客户端 (2.2.8)
    [ProtoContract]
    public class RobotRespawnStatusData {
        [ProtoMember(1)] public bool is_pending_respawn;        // 是否处于待复活状态
        [ProtoMember(2)] public uint total_respawn_progress;    // 复活所需总读条
        [ProtoMember(3)] public uint current_respawn_progress;  // 当前复活读条进度
        [ProtoMember(4)] public bool can_free_respawn;          // 是否可以免费复活
        [ProtoMember(5)] public uint gold_cost_for_respawn;     // 花费金币复活所需金币数
        [ProtoMember(6)] public bool can_pay_for_respawn;       // 是否允许花费金币复活
    }

    // 9. RobotStaticStatus - 服务器->自定义客户端 (2.2.9)
    [ProtoContract]
    public class RobotStaticStatusData {
        [ProtoMember(1)] public uint connection_state;              // 连接状态
        [ProtoMember(2)] public uint field_state;                   // 上场状态
        [ProtoMember(3)] public uint alive_state;                   // 存活状态
        [ProtoMember(4)] public uint robot_id;                      // 机器人编号
        [ProtoMember(5)] public uint robot_type;                    // 机器人类型
        [ProtoMember(6)] public uint performance_system_shooter;    // 发射机构性能体系
        [ProtoMember(7)] public uint performance_system_chassis;    // 底盘性能体系
        [ProtoMember(8)] public uint level;                         // 当前等级
        [ProtoMember(9)] public uint max_health;                    // 最大血量
        [ProtoMember(10)] public uint max_heat;                     // 最大热量
        [ProtoMember(11)] public float heat_cooldown_rate;          // 热量冷却速率
        [ProtoMember(12)] public uint max_power;                    // 最大功率
        [ProtoMember(13)] public uint max_buffer_energy;            // 最大缓冲能量
        [ProtoMember(14)] public uint max_chassis_energy;           // 最大底盘能量
    }

    // 10. RobotDynamicStatus - 服务器->自定义客户端 (2.2.10)
    [ProtoContract]
    public class RobotDynamicStatusData {
        [ProtoMember(1)] public uint current_health;                // 当前血量
        [ProtoMember(2)] public float current_heat;                 // 当前热量
        [ProtoMember(3)] public float last_projectile_fire_rate;    // 上一次弹丸射速
        [ProtoMember(4)] public uint current_chassis_energy;        // 当前底盘能量
        [ProtoMember(5)] public uint current_buffer_energy;         // 当前缓冲能量
        [ProtoMember(6)] public uint current_experience;            // 当前经验值
        [ProtoMember(7)] public uint experience_for_upgrade;        // 升级所需经验
        [ProtoMember(8)] public uint total_projectiles_fired;       // 累计已发弹量
        [ProtoMember(9)] public uint remaining_ammo;                // 剩余允许发弹量
        [ProtoMember(10)] public bool is_out_of_combat;             // 是否脱战
        [ProtoMember(11)] public uint out_of_combat_countdown;      // 脱战倒计时
        [ProtoMember(12)] public bool can_remote_heal;              // 是否可以远程补血
        [ProtoMember(13)] public bool can_remote_ammo;              // 是否可以远程补弹
    }

    // 11. RobotModuleStatus - 服务器->自定义客户端 (2.2.11)
    [ProtoContract]
    public class RobotModuleStatusData {
        [ProtoMember(1)] public uint power_manager;             // 电源管理模块状态
        [ProtoMember(2)] public uint rfid;                      // RFID模块状态
        [ProtoMember(3)] public uint light_strip;               // 灯条模块状态
        [ProtoMember(4)] public uint small_shooter;             // 17mm发射机构状态
        [ProtoMember(5)] public uint big_shooter;               // 42mm发射机构状态
        [ProtoMember(6)] public uint uwb;                       // 定位模块状态
        [ProtoMember(7)] public uint armor;                     // 装甲模块状态
        [ProtoMember(8)] public uint video_transmission;        // 图传模块状态
        [ProtoMember(9)] public uint capacitor;                 // 电容模块状态
        [ProtoMember(10)] public uint main_controller;          // 主控状态
    }

    // 12. RobotPosition - 服务器->自定义客户端 (2.2.12)
    [ProtoContract]
    public class RobotPositionData {
        [ProtoMember(1)] public float x;                        // 世界坐标X轴
        [ProtoMember(2)] public float y;                        // 世界坐标Y轴
        [ProtoMember(3)] public float z;                        // 世界坐标Z轴
        [ProtoMember(4)] public float yaw;                      // 朝向角度
    }

    // 13. Buff - 服务器->自定义客户端 (2.2.13)
    [ProtoContract]
    public class BuffData {
        [ProtoMember(1)] public uint robot_id;                  // 机器人ID
        [ProtoMember(2)] public uint buff_type;                 // Buff类型
        [ProtoMember(3)] public int buff_level;                 // Buff增益值
        [ProtoMember(4)] public uint buff_max_time;             // Buff最大剩余时间
        [ProtoMember(5)] public uint buff_left_time;            // Buff剩余时间
        [ProtoMember(6)] public string msg_params;              // 额外文字参数
    }

    // 14. PenaltyInfo - 服务器->自定义客户端 (2.2.14)
    [ProtoContract]
    public class PenaltyInfoData {
        [ProtoMember(1)] public uint penalty_type;          // 当前受罚类型
        [ProtoMember(2)] public uint penalty_effect_sec;    // 当前受罚效果时长
        [ProtoMember(3)] public uint total_penalty_num;     // 当前判罚数量
    }

    // 15. RobotPathPlanInfo - 服务器->自定义客户端 (2.2.15)
    [ProtoContract]
    public class RobotPathPlanInfoData {
        [ProtoMember(1)] public uint intention;             // 哨兵意图
        [ProtoMember(2)] public uint start_pos_x;           // 起始点X坐标
        [ProtoMember(3)] public uint start_pos_y;           // 起始点Y坐标
        [ProtoMember(4)] public int[] offset_x;             // 相对起始点X增量数组
        [ProtoMember(5)] public int[] offset_y;             // 相对起始点Y增量数组
        [ProtoMember(6)] public uint sender_id;             // 发送者ID
    }

    // 16. MapClickInfoNotify - 自定义客户端->服务器 (2.2.16)
    [ProtoContract]
    public class MapClickInfoNotifyData {
        [ProtoMember(1)] public uint is_send_all;           // 发送范围
        [ProtoMember(2)] public byte[] robot_id;            // 目标机器人ID列表
        [ProtoMember(3)] public uint mode;                  // 标记类型
        [ProtoMember(4)] public uint enemy_id;              // 标定的对方ID
        [ProtoMember(5)] public uint ascii;                 // 自定义图标ASCII码
        [ProtoMember(6)] public uint type;                  // 标记模式
        [ProtoMember(7)] public uint screen_x;              // 屏幕坐标X
        [ProtoMember(8)] public uint screen_y;              // 屏幕坐标Y
        [ProtoMember(9)] public float map_x;                // 地图坐标X
        [ProtoMember(10)] public float map_y;               // 地图坐标Y
    }

    // 17. RaderInfoToClient - 服务器->自定义客户端 (2.2.17)
    [ProtoContract]
    public class RaderInfoToClientData {
        [ProtoMember(1)] public uint target_robot_id;       // 目标机器人ID
        [ProtoMember(2)] public float target_pos_x;         // 目标位置X
        [ProtoMember(3)] public float target_pos_y;         // 目标位置Y
        [ProtoMember(4)] public float torward_angle;        // 朝向角度
        [ProtoMember(5)] public uint is_high_light;         // 是否特殊标识
    }

    // 18. CustomByteBlock - 机器人->自定义客户端 (2.2.18)
    [ProtoContract]
    public class CustomByteBlockData {
        [ProtoMember(1)] public byte[] data;               // 自定义数据包
    }

    // 19. AssemblyCommand - 自定义客户端->服务器 (2.2.19)
    [ProtoContract]
    public class AssemblyCommandData {
        [ProtoMember(1)] public uint operation;            // 装配操作类型
        [ProtoMember(2)] public uint difficulty;           // 选中的装配难度
    }

    // 20. TechCoreMotionStateSync - 服务器->自定义客户端 (2.2.20)
    [ProtoContract]
    public class TechCoreMotionStateSyncData {
        [ProtoMember(1)] public uint maximum_difficulty_level;  // 最高装配难度等级
        [ProtoMember(2)] public uint status;                    // 科技核心状态
    }

    // 21. RobotPerformanceSelectionCommand - 自定义客户端->服务器 (2.2.21)
    [ProtoContract]
    public class RobotPerformanceSelectionCommandData {
        [ProtoMember(1)] public uint shooter;              // 发射机构性能体系
        [ProtoMember(2)] public uint chassis;              // 底盘性能体系
    }

    // 22. RobotPerformanceSelectionSync - 服务器->自定义客户端 (2.2.22)
    [ProtoContract]
    public class RobotPerformanceSelectionSyncData {
        [ProtoMember(1)] public uint shooter;              // 发射机构性能体系
        [ProtoMember(2)] public uint chassis;              // 底盘性能体系
    }

    // 23. HeroDeployModeEventCommand - 自定义客户端->服务器 (2.2.23)
    [ProtoContract]
    public class HeroDeployModeEventCommandData {
        [ProtoMember(1)] public uint mode;                 // 部署模式
    }

    // 24. DeployModeStatusSync - 服务器->自定义客户端 (2.2.24)
    [ProtoContract]
    public class DeployModeStatusSyncData {
        [ProtoMember(1)] public uint status;               // 当前部署模式状态
    }

    // 25. RuneActivateCommand - 自定义客户端->服务器 (2.2.25)
    [ProtoContract]
    public class RuneActivateCommandData {
        [ProtoMember(1)] public uint activate;             // 激活指令
    }

    // 26. RuneStatusSync - 服务器->自定义客户端 (2.2.26)
    [ProtoContract]
    public class RuneStatusSyncData {
        [ProtoMember(1)] public uint rune_status;          // 能量机关状态
        [ProtoMember(2)] public uint activated_arms;       // 已激活灯臂数量
        [ProtoMember(3)] public uint average_rings;        // 总环数
    }

    // 27. SentinelStatusSync - 服务器->自定义客户端 (2.2.27)
    [ProtoContract]
    public class SentinelStatusSyncData {
        [ProtoMember(1)] public uint posture_id;           // 姿态ID
        [ProtoMember(2)] public bool is_weakened;          // 是否弱化
    }

    // 28. DartCommand - 自定义客户端->服务器 (2.2.28)
    [ProtoContract]
    public class DartCommandData {
        [ProtoMember(1)] public uint target_id;            // 目标ID
        [ProtoMember(2)] public bool open;                 // 闸门开关
    }

    // 29. DartSelectTargetStatusSync - 服务器->自定义客户端 (2.2.29)
    [ProtoContract]
    public class DartSelectTargetStatusSyncData {
        [ProtoMember(1)] public uint target_id;            // 目标ID
        [ProtoMember(2)] public bool open;                 // 闸门状态
    }

    // 30. GuardCtrlCommand - 自定义客户端->服务器 (2.2.30)
    [ProtoContract]
    public class GuardCtrlCommandData {
        [ProtoMember(1)] public uint command_id;           // 指令编号
    }

    // 31. GuardCtrlResult - 服务器->自定义客户端 (2.2.31)
    [ProtoContract]
    public class GuardCtrlResultData {
        [ProtoMember(1)] public uint command_id;           // 对应的指令编号
        [ProtoMember(2)] public uint result_code;          // 执行结果码
    }

    // 32. AirSupportCommand - 自定义客户端->服务器 (2.2.32)
    [ProtoContract]
    public class AirSupportCommandData {
        [ProtoMember(1)] public uint command_id;           // 指令类型
    }

    // 33. AirSupportStatusSync - 服务器->自定义客户端 (2.2.33)
    [ProtoContract]
    public class AirSupportStatusSyncData {
        [ProtoMember(1)] public uint airsupport_status;    // 空中支援状态
        [ProtoMember(2)] public uint left_time;            // 免费支援剩余时间
        [ProtoMember(3)] public uint cost_coins;           // 付费支援已花费金币
    }
}