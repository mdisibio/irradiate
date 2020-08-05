using System;
using System.Threading.Tasks;

namespace Irradiate.Tests
{
    public interface IThing
    {
        void Void();
        void VoidParams(int x, int y);
        int FuncParams(int x, int y);
        Task<int> FuncParamsAsync(int x, int y);
        Task<int> FuncParamsAsyncDelayed(int x, int y);
        void ThrowsException();
    }

    public class Thing : IThing
    {
        public void Void() { }
        public void VoidParams(int x, int y) { }
        public int FuncParams(int x, int y) => x * y;
        public Task<int> FuncParamsAsync(int x, int y) => Task.FromResult<int>(x * y);
        public async Task<int> FuncParamsAsyncDelayed(int x, int y)
        {
            await Task.Delay(100);
            return x * y;
        }
        public void ThrowsException() => throw new NotImplementedException();
    }

}
