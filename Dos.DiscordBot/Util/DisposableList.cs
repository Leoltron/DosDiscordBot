using System;
using System.Collections.Generic;

namespace Dos.DiscordBot.Util
{
    public class DisposableList : List<IDisposable>, IDisposable
    {
        public void Dispose()
        {
            this.DisposeAll();
            Clear();
        }
    }
}
