using System;
using System.Collections.Generic;
using System.Text;

namespace DevilBot.Models
{
    class UserSession
    {
        public VideoResponse Response { get; set; } = new();

        public int StepNumber { get; set; }

        public string ApiUrl { get; set; } = "";

        public CancellationTokenSource Cts { get; set; } = new();

        public SemaphoreSlim Semaphore { get; set; } = new(1, 1);

        public bool IsInitialized { get; set; }
    }

}
