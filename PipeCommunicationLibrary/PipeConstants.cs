using System;

namespace PipeCommunicationLibrary
{
    public static class PipeConstants
    {
        public const string PIPE_NAME = "WpfAppCommunicationPipe";
        public const string APP_B_RUNNING_MESSAGE = "APP_B_IS_RUNNING";
        public const string CHECK_STATUS_MESSAGE = "CHECK_STATUS";
        public const string CLIENT_RUNNING_MESSAGE = "CLIENT_RUNNING";
        public const string GET_CLIENT_INFO_MESSAGE = "GET_CLIENT_INFO";
        public const string SHUTDOWN_REQUEST_MESSAGE = "SHUTDOWN_REQUEST";
        public const string SHUTDOWN_CONFIRMED_MESSAGE = "SHUTDOWN_CONFIRMED";
    }
}
