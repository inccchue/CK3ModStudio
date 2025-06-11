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
        private bool _isConnected = false;
        private bool _isPolling = false;

        public event Action<string> MessageReceived;
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

        public bool IsConnected
        {
            get => _isConnected && _pipeClient?.IsConnected == true;
            set 
            {
                if (_isConnected == value) return; // 状态未改变则不处理
                SetProperty(ref _isConnected, value);
                ServerStatusChanged?.Invoke();
            }
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
            _isPolling = false;
            _serverCheckTimer?.Dispose();
            _serverCheckTimer = null;
        }

        // 检查服务器并尝试连接
        private async Task CheckServerAndConnect()
        {
            try
            {
                bool serverRunning = await _heartbeatChecker.IsAppBRunningAsync();

                if (serverRunning && !IsConnected)
                {
                    // 服务器运行且未连接，尝试建立连接
                    IsConnected = await ConnectAndStartListeningAsync();
                }
                else if (!serverRunning && IsConnected)
                {
                    // 服务器停止但仍显示连接，断开连接
                    await DisconnectAsync();
                    
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
            if (IsConnected) return true;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _pipeClient = new NamedPipeClientStream(".", PipeConstants.PIPE_NAME, PipeDirection.InOut);

                await _pipeClient.ConnectAsync(5000);

                if (_pipeClient.IsConnected)
                {
                    _reader = new StreamReader(_pipeClient);
                    _writer = new StreamWriter(_pipeClient) { AutoFlush = true };
                    IsConnected = true;

                    Connected?.Invoke();

                    // 启动监听任务
                    _ = Task.Run(ListenForMessagesAsync, _cancellationTokenSource.Token);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
                await DisconnectAsync();
            }

            return false;
        }

        // 持续监听服务器消息
        private async Task ListenForMessagesAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && _pipeClient.IsConnected)
                {
                    string message = await _reader.ReadLineAsync();

                    if (message != null)
                    {
                        MessageReceived?.Invoke(message);
                        await HandleMessageAsync(message);
                    }
                    else
                    {
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

        // 处理接收到的消息并发送回复
        private async Task HandleMessageAsync(string message)
        {
            try
            {
                string response = "";

                switch (message)
                {
                    case PipeConstants.CHECK_STATUS_MESSAGE:
                        response = PipeConstants.CLIENT_RUNNING_MESSAGE;
                        break;

                    case PipeConstants.GET_CLIENT_INFO_MESSAGE:
                        response = GetClientInfo();
                        break;

                    case PipeConstants.SHUTDOWN_REQUEST_MESSAGE:
                        response = PipeConstants.SHUTDOWN_CONFIRMED_MESSAGE;
                        break;

                    default:
                        response = ProcessCustomMessage(message);
                        break;
                }

                if (!string.IsNullOrEmpty(response))
                {
                    await SendMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
        }

        protected virtual string ProcessCustomMessage(string message)
        {
            return $"已收到消息: {message}";
        }

        private string GetClientInfo()
        {
            return $"客户端进程ID: {System.Diagnostics.Process.GetCurrentProcess().Id}, 时间: {DateTime.Now}";
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
                ErrorOccurred?.Invoke(ex);
            }

            return false;
        }

        public async Task DisconnectAsync()
        {
            IsConnected = false;

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

        

        public void Dispose()
        {
            StopPolling();
            DisconnectAsync().Wait();
        }
    }
}
