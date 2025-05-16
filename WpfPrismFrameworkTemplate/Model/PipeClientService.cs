using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeCommunicationLibrary;

namespace WpfPrismFrameworkTemplate.Model
{
    public class PipeClientService
    {
        // 检查 B 程序是否运行
        public async Task<bool> IsAppBRunningAsync()
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeConstants.PIPE_NAME, PipeDirection.InOut))
                {
                    // 尝试连接到服务器，设置超时时间为 1 秒
                    // 如果无法连接，说明 B 程序未运行
                    await pipeClient.ConnectAsync(1000);

                    if (pipeClient.IsConnected)
                    {
                        using (StreamReader reader = new StreamReader(pipeClient))
                        using (StreamWriter writer = new StreamWriter(pipeClient))
                        {
                            // 发送检查状态消息
                            await writer.WriteLineAsync(PipeConstants.CHECK_STATUS_MESSAGE);
                            await writer.FlushAsync();

                            // 读取回复
                            string response = await reader.ReadLineAsync();

                            // 检查回复是否表明 B 程序正在运行
                            return response == PipeConstants.APP_B_RUNNING_MESSAGE;
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                // 连接超时，B 程序未运行
                return false;
            }
            catch (Exception ex)
            {
                // 其他异常，假定 B 程序未运行
                Console.WriteLine($"Pipe client error: {ex.Message}");
                return false;
            }

            return false;
        }

        public async Task<bool> SendFamilyNameAsync(string familyName)
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeConstants.PIPE_NAME, PipeDirection.InOut))
                {
                    // 尝试连接到服务器，设置超时时间为 1 秒
                    // 如果无法连接，说明 B 程序未运行
                    await pipeClient.ConnectAsync(1000);

                    if (pipeClient.IsConnected)
                    {
                        using (StreamReader reader = new StreamReader(pipeClient))
                        using (StreamWriter writer = new StreamWriter(pipeClient))
                        {
                            await writer.WriteLineAsync(familyName);
                            await writer.FlushAsync();

                            // 读取回复
                            string response = await reader.ReadLineAsync();

                            // 检查回复是否表明 B 程序正在运行
                            return response == PipeConstants.APP_B_RUNNING_MESSAGE;
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                // 连接超时，B 程序未运行
                return false;
            }
            catch (Exception ex)
            {
                // 其他异常，假定 B 程序未运行
                Console.WriteLine($"Pipe client error: {ex.Message}");
                return false;
            }

            return false;
        }
    }


}
