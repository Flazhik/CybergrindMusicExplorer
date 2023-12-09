using System;
using System.Threading;
using System.Threading.Tasks;

namespace CybergrindMusicExplorer.Downloader
{
    public class QueueManager
    {
        private readonly SemaphoreSlim semaphore;
        public QueueManager()
        {
            semaphore = new SemaphoreSlim(4);
        }

        public async Task Enqueue(Func<Task> taskGenerator)
        {
            await semaphore.WaitAsync();
            try
            {
                await taskGenerator();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}