using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ImbaXIV
{
    class ProcessReader
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int pHandle, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        
        public const int PROCESS_VM_READ = 0x10;

        private Process ffxivProcess;
        private int hdl;

        public IntPtr ModuleBase;

        public bool AttachProcess()
        {
            Process[] processArr = Process.GetProcessesByName("ffxiv_dx11");
            if (processArr.Length == 0)
            {
                return false;
            }
            ffxivProcess = processArr[0];
            int pid = ffxivProcess.Id;
            hdl = OpenProcess(PROCESS_VM_READ, false, ffxivProcess.Id);
            if (hdl == 0)
            {
                return false;
            }
            ModuleBase = ffxivProcess.MainModule.BaseAddress;
            return true;
        }

        public bool CheckAlive()
        {
            return !(ffxivProcess is null) && !ffxivProcess.HasExited;
        }

        public byte[] ReadBytes(long addr, uint size)
        {
            byte[] buf = new byte[size];
            ReadProcessMemory(hdl, (IntPtr)addr, buf, size, IntPtr.Zero);
            return buf;
        }

        public String ReadString(long addr)
        {
            return ReadString(addr, 64);
        }

        public String ReadString(long addr, uint size)
        {
            byte[] buf = ReadBytes(addr, size);
            String tmp = Encoding.UTF8.GetString(buf);
            int nullIdx = tmp.IndexOf('\0');
            if (nullIdx == -1)
                return tmp;
            return tmp.Substring(0, nullIdx);
        }

        public String ReadString(long offset, IntPtr baseAddr, uint size)
        {
            long addr = (long)baseAddr + offset;
            return ReadString(addr, size);
        }

        public float ReadFloat(long addr)
        {
            byte[] buf = ReadBytes(addr, 4);
            return BitConverter.ToSingle(buf, 0);
        }

        public float ReadFloat(long offset, IntPtr baseAddr)
        {
            long addr = (long)baseAddr + offset;
            return ReadFloat(addr);
        }

        public int ReadInt32(long addr)
        {
            byte[] buf = ReadBytes(addr, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public int ReadInt32(long offset, IntPtr baseAddr)
        {
            long addr = (long)baseAddr + offset;
            return ReadInt32(addr);
        }

        public long ReadInt64(long addr)
        {
            byte[] buf = ReadBytes(addr, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public long ReadInt64(long offset, IntPtr baseAddr)
        {
            long addr = (long)baseAddr + offset;
            return ReadInt64(addr);
        }
    }
}
