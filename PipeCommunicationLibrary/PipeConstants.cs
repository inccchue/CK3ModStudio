using System;

namespace PipeCommunicationLibrary
{
    public static class PipeConstants
    {
        public const string PIPE_NAME = "WpfAppCommunicationPipe";
    }

    public enum PipeMessageType
    {
        AppBRunning,
        CheckStatus,
        ClientRunning,
        GetClientInfo,
        ShutdownRequest,
        ShutdownConfirmed,
        SendFamilyInfo
    }

    public class PipeMessage
    {
        public PipeMessageType MessageType { get; set; }
        public string Content { get; set; }
    }

    public class CommonFamily
    {
        public string FamilyName;
        public string FamilyName_CN;
    }
}
