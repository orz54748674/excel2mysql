using System;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Excel2Mysql.util
{
    public class Process
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]

        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        public static void Kill(Application excel)
        {
            try
            {
                IntPtr t = new IntPtr(excel.Hwnd);

                int k = 0;

                GetWindowThreadProcessId(t, out k);

                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);

                p.Kill();
            }
            catch
            { }
        }
    }  
}
