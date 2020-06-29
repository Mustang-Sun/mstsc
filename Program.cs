using Microsoft.Win32;
using NetFwTypeLib;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace App{
    class Program
    {
        static void Main(string[] args)
        {
            ServicesCheck();
        }

        /// <summary>
        /// 服务检查
        /// </summary>
        public static void ServicesCheck()
        {
            Console.WriteLine("按任意键执行...");
            Console.ReadKey();
            Console.WriteLine("开始进行本地远程桌面服务检查...");
            StartAndEnable("SessionEnv");
            Console.WriteLine("Remote Desktop Configuration Start And Enable Success!");
            StartAndEnable("TermService");
            Console.WriteLine("Remote Desktop Services Start And Enable Success!");
            StartAndEnable("UmRdpService");
            Console.WriteLine("Remote Desktop Services UserMode Port Redirector Start And Enable Success!");
            ChangeReg();
            Console.WriteLine("本地用户模式修改为经典用户模式成功!");
            //NetFwAddPorts();
            //Console.WriteLine("本机防火墙放行成功!");
            StartRdpService();
            Console.WriteLine("本地RDP服务开启成功! \r\n---------------------\r\n远程桌面所有相关服务已启用，请确认当前计算机已设置登录密码 \r\n按任意键关闭...");
            Console.ReadKey();
        }

        /// <summary>
        /// 启用远程桌面服务
        /// </summary>
        private static void StartRdpService()
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Terminal Server", true);
            if (myKey != null)
            {
                myKey.SetValue("fDenyTSConnections", 0);
                myKey.Close();
            }
        }

        /// <summary>
        /// 启动并开机启用
        /// </summary>
        /// <param name="servicename">服务名称</param>
        public static void StartAndEnable(string servicename)
        {
            //netsh firewall set portopening TCP 3389 ENABLE
            CmdRun("net start "+ servicename);
            CmdRun("sc config "+servicename+" start= auto");
        }

        /// <summary>
        /// 执行任务并设置为开机启动
        /// </summary>
        /// <param name="servicesname">服务名称</param>
        public static void CmdRun(string cmdstring)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            //反斜杠/c 是用于告知系统，运行的是命令而不是文字
            startInfo.Arguments = "/c "+cmdstring;
            process.StartInfo = startInfo;
            process.Start();
        }

        /// <summary>
        /// 修改注册表
        /// </summary>
        public static void ChangeReg()
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa", true);
            if (myKey != null)
            {
                myKey.SetValue("forceguest", 0);
                myKey.Close();
            }
        }

        /// <summary>
        /// 防火墙放行
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        public static void NetFwAddPorts()
        {
            try
            {
                CmdRun("netsh firewall set portopening TCP 3389 ENABLE");
            }catch (Exception)
            {
                INetFwRule2 inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                inboundRule.Enabled = true;
                inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                inboundRule.Protocol = 6;
                inboundRule.LocalPorts = "3389";
                inboundRule.Name = "Access 3389";
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(inboundRule);
            }
        }
    }
}
