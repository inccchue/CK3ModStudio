using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class FileHeartbeatChecker
    {
        private readonly string _heartbeatFile;
        private const int HEARTBEAT_TIMEOUT_SECONDS = 10; // 10秒没有心跳就认为服务器停止

        public FileHeartbeatChecker()
        {
            _heartbeatFile = Path.Combine(Path.GetTempPath(), "appb_heartbeat.txt");
        }

        public async Task<bool> IsAppBRunningAsync()
        {
            return await Task.Run(() => IsAppBRunning()); // 异步执行文件操作
        }

        private bool IsAppBRunning()
        {
            try
            {
                if (!File.Exists(_heartbeatFile))
                {
                    Console.WriteLine("心跳文件不存在");
                    return false;
                }

                string content = File.ReadAllText(_heartbeatFile);

                if (string.IsNullOrEmpty(content))
                {
                    Console.WriteLine("心跳文件为空");
                    return false;
                }

                // 解析心跳信息: "时间戳|进程ID|状态"
                string[] parts = content.Split('|');

                if (parts.Length >= 3)
                {
                    string timestampStr = parts[0];
                    string processIdStr = parts[1];
                    string status = parts[2];

                    if (DateTime.TryParse(timestampStr, out DateTime lastHeartbeat))
                    {
                        double secondsSinceLastHeartbeat = (DateTime.Now - lastHeartbeat).TotalSeconds;
                        bool isRunning = secondsSinceLastHeartbeat < HEARTBEAT_TIMEOUT_SECONDS && status == "running";

                        // 可选：验证进程ID是否仍然存在
                        if (isRunning && int.TryParse(processIdStr, out int processId))
                        {
                            try
                            {
                                Process process = Process.GetProcessById(processId);
                                isRunning = !process.HasExited;
                            }
                            catch (ArgumentException)
                            {
                                // 进程不存在
                                isRunning = false;
                            }
                        }

                        Console.WriteLine($"心跳检测: 上次心跳 {secondsSinceLastHeartbeat:F1} 秒前, 状态: {(isRunning ? "运行中" : "已停止")}");
                        return isRunning;
                    }
                }

                Console.WriteLine("心跳文件格式错误");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"心跳检测异常: {ex.Message}");
                return false;
            }
        }
    }
}
