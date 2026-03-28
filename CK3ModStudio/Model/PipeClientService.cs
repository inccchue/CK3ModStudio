using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeCommunicationLibrary;
using System.Threading;
using Prism.Mvvm;
using GongSolutions.Wpf.DragDrop;
using System.Diagnostics;
using WpfPrismFrameworkTemplate.Helper;
using Newtonsoft.Json;

namespace WpfPrismFrameworkTemplate.Model
{
    // 改进的管道客户端服务
    public class PipeClientService : BindableBase,IDisposable
    {
        private NamedPipeClientStream _pipeClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cancellationTokenSource;
        private Timer _serverCheckTimer;
        private bool _isPolling = false;

        public bool _IsShowDebugInfo = false;

        //public event Action<PipeMessage> MessageReceived;
        public event Action<Exception> ErrorOccurred;
        public event Action Connected;
        public event Action Disconnected;
        public event Action ServerStatusChanged; // 服务器状态变化事件

        private bool _isChecking = false;
        private int _consecutiveFailures = 0;
        private const int MAX_FAILURES_BEFORE_DISCONNECT = 3; // 连续失败3次才认为服务器断开
        private readonly FileHeartbeatChecker _heartbeatChecker=new FileHeartbeatChecker();

        // 轮询间隔（毫秒）
        public int PollingInterval { get; set; } = 3000; // 默认3秒

        public bool IsShowDebugInfo
        {
            get => _IsShowDebugInfo;
            set => SetProperty(ref _IsShowDebugInfo, value);
        }

        public bool IsConnected
        {
            get => _pipeClient!=null && _pipeClient.IsConnected == true;
        }
      

        // 开始轮询服务器状态
        public void StartPolling()
        {
            if (_isPolling) return;
            _isPolling = true;
            _serverCheckTimer = new Timer(async _ => await CheckServerAndConnect(), null, 0, PollingInterval);
        }

        // 停止轮询
        public void StopPolling()
        {
            _serverCheckTimer?.Dispose();
            _serverCheckTimer = null;
            _isPolling = false;
            
        }

        // 检查服务器并尝试连接
        private async Task CheckServerAndConnect()
        {
            try
            {
                if (!IsConnected)
                {
                    bool serverRunning = await _heartbeatChecker.IsAppBRunningAsync();
                    if (serverRunning)
                    {
                        // 服务器运行且未连接，尝试建立连接
                        bool isSuccessConnect = await ConnectAndStartListeningAsync();
                        if (isSuccessConnect)
                        {
                            ShowDebugInfo("已连接到管道服务器");
                        }
                    }
                }
                

               
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
        }

        // 连接到服务器并开始持续监听
        public async Task<bool> ConnectAndStartListeningAsync()
        {
            NamedPipeClientStream tempPipeClient = null;
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                tempPipeClient = new NamedPipeClientStream(
                    ".",
                    PipeConstants.PIPE_NAME,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous); // 添加异步选项

                await tempPipeClient.ConnectAsync(_cancellationTokenSource.Token);

                if (tempPipeClient !=null&& tempPipeClient.IsConnected)
                {
                    // 只有在成功连接后才赋值给字段
                    _pipeClient = tempPipeClient;

                    _reader = new StreamReader(tempPipeClient);
                    _writer = new StreamWriter(tempPipeClient) { AutoFlush = true };

                    Connected?.Invoke();                   


                    // 启动监听任务（在后台监听服务器返回的消息）
                    _ = Task.Run(async () => await ListenForMessagesAsync(), _cancellationTokenSource.Token);

                    return true;
                }
                else
                {
                    ShowDebugInfo("连接失败：管道未连接");
                    tempPipeClient?.Dispose();
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                ShowDebugInfo("连接被取消");
                tempPipeClient?.Dispose();
                return false;
            }
            catch (TimeoutException ex)
            {
                ShowDebugInfo($"连接超时: {ex.Message}");
                tempPipeClient?.Dispose();
                ErrorOccurred?.Invoke(ex);
                return false;
            }
            catch (Exception ex)
            {
                ShowDebugInfo($"连接失败: {ex.Message}");
                tempPipeClient?.Dispose();
                ErrorOccurred?.Invoke(ex);
                await DisconnectAsync();
                return false;
            }
        }

        // 持续监听服务器消息
        private async Task ListenForMessagesAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested&& _pipeClient!=null && _pipeClient.IsConnected)
                {
                    try
                    {
                        string message = await _reader.ReadLineAsync();
                        ShowDebugInfo($"收到服务器响应: {message}");
                        if (message != null)
                        {                           
                            await HandleMessageAsync(message);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (IOException)
                    {
                        ShowDebugInfo("管道连接中断");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        ShowDebugInfo("管道已被释放");
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        private Task HandleMessageAsync(string message)
        {
            try
            {
                JsonConvert.DeserializeObject<PipeMessage>(message);
            }
            catch (JsonException)
            {
                ShowDebugInfo($"Invalid message format: {message}");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                if (IsConnected && _writer != null)
                {
                    await _writer.WriteLineAsync(message);
                    await _writer.FlushAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowDebugInfo($@"发送消息失败:{ex.Message}");
                ErrorOccurred?.Invoke(ex);
            }

            return false;
        }

        public async Task SendFamilyInfoAsync(Family selectFamily)
        {
            try
            {
                CommonFamily commonFamily = new CommonFamily
                {
                    FamilyName = selectFamily.FamilyName,
                    FamilyName_CN = selectFamily.FamilyName_CN
                };
                string jsonMessage = JsonConvert.SerializeObject(commonFamily);
                var message = new PipeMessage
                {
                    MessageType = PipeMessageType.SendFamilyInfo,
                    Content = jsonMessage
                };

                jsonMessage = JsonConvert.SerializeObject(message);
                await SendMessageAsync(jsonMessage);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
        }

        public async Task DisconnectAsync()
        {
            _cancellationTokenSource?.Cancel();

            try
            {
                _writer?.Close();
                _reader?.Close();
                _pipeClient?.Close();
            }
            catch { }
            finally
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _pipeClient?.Dispose();
                _cancellationTokenSource?.Dispose();

                _writer = null;
                _reader = null;
                _pipeClient = null;
                _cancellationTokenSource = null;

                Disconnected?.Invoke();
            }
        }

        public void ShowDebugInfo(string info)
        {
            if (IsShowDebugInfo)
            {
                Console.WriteLine(info);
                HandyControl.Controls.Growl.Info(info);
            }
        }

        public void Dispose()
        {
            StopPolling();
            DisconnectAsync().Wait();
        }
    }
}
