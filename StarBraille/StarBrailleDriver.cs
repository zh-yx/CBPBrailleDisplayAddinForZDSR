using System;
using System.Runtime.InteropServices;

namespace StarBraille
{
    /// <summary>
    /// 文星盲文显示器驱动库导出函数的包装静态类。
    /// </summary>
    internal static class StarBrailleDriver
    {
        /// <summary>
        /// 文星盲文显示器驱动动态链接库文件名的常量。
        /// </summary>
        private const string starDriverDll = "StarLibDriver.dll";

        /// <summary>
        /// 打开连接到本机的文星盲文显示器。
        /// </summary>
        /// <returns>如果成功返回 1，否则返回 0。</returns>
        [DllImport(starDriverDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenBrailleDisplay();

        /// <summary>
        /// 显示盲文内容。
        /// 以一个字节的0~7位是否为1代表第1~8点的抬起。
        /// </summary>
        /// <param name="brailleData">要显示的盲文内容数据。</param>
        [DllImport(starDriverDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowBraille(byte[] brailleData);

        /// <summary>
        /// 获取盲文显示器按钮信息。
        /// 按键 Id 规则如下：
        /// 未连接到设备为 -1；
        /// 路由光标键为对应序号 Id；
        /// 路由光标键随后为其他功能按键；
        /// 没有按键被按下为 255。
        /// </summary>
        /// <returns>按键 Id。</returns>
        [DllImport(starDriverDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetBtn();

        /// <summary>
        /// 关闭已经打开的文星盲文显示器。
        /// </summary>
        [DllImport(starDriverDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseBrailleDisplay();
    }
}
