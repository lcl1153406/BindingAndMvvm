﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace LoongEgg.MouseKeyHook
{
  
    /// <summary>
    ///     全局监听器的基类
    /// </summary>
    public abstract class BaseGlobalListener : IDisposable
    {
        /*-------------------------  Public Property  -------------------------*/
        /// <summary>
        ///     <see cref="IdHooks"/>
        /// </summary>
        [Required] 
        public IdHooks IdHook { get; private set; }

        /*--------------------------  Private Fields  -------------------------*/
        /// <summary>
        ///     回调函数委托, 指向钩子函数完成时的方法
        /// </summary>
        protected LowLevelHookProc _HookProc;

        /// <summary>
        ///     钩子句柄
        /// </summary>
        protected IntPtr _HookId = IntPtr.Zero;

        /// <summary>
        ///     已经销毁资源?
        /// </summary>
        protected bool disposed;

        /*---------------------------  Constructor  ---------------------------*/
        /// <summary>
        ///     <see cref="BaseGlobalListener"/>构造器
        /// </summary>
        public BaseGlobalListener(IdHooks idHook)
        {
            IdHook = idHook;
            _HookProc = HookCallBack;
        }

        /// <summary>
        ///     解构器
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        ///     清理占用的资源
        /// </summary>
        /// <param name="disposing">
        ///     垃圾回收器正在释放资源?
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (disposed || disposing)
                return;

            if (_HookId != IntPtr.Zero)
            {
                User32Dll.UnhookWindowsHookEx(
                    _HookId);
            }

            disposed = true;
        }

        /// <summary>
        ///     如果忘记手动Dispose(), 垃圾回收器会自动调用
        /// </summary>
        ~BaseGlobalListener() => Dispose(false);

        /*--------------------------  Public Methods  -------------------------*/
        /// <summary>
        ///     装载键盘/鼠标钩子函数
        /// </summary>
        public void SetHook()
        {
            _HookId = SetHook(_HookProc);
        }

        /// <summary>
        ///     卸载键盘/鼠标钩子函数
        /// </summary>
        public void UnHook()
        {
            User32Dll.UnhookWindowsHookEx(_HookId);
        }

        /*--------------------------  Private Methods  -------------------------*/
        /// <summary>
        ///     设置钩子函数
        /// </summary>
        ///     <param name="proc">
        ///         钩子的地址?
        ///     </param>
        /// <returns>
        /// </returns>
        /// <remarks>
        ///     https://docs.microsoft.com/zh-cn/dotnet/api/system.diagnostics.process.mainmodule?view=netframework-4.8
        /// </remarks>
        protected IntPtr SetHook(LowLevelHookProc proc)
        {
            //  [Process] 提供对本地和远程进程的访问权限并使你能够启动和停止本地系统进程。 
            using (Process curProcess = Process.GetCurrentProcess()) // 获取新的 Process 组件并将其与当前活动的进程关联。
            {
                using (ProcessModule curModule = curProcess.MainModule) // 获取关联进程的主模块。
                { 
                       var ret = User32Dll.SetWindowsHookEx(
                            (int)IdHook,
                            proc,
                            User32Dll.GetModuleHandle(curModule.ModuleName),
                            0
                        );
                    return ret;
                }
            }
        }

        /// <summary>
        ///     键盘/鼠标监控器钩子的回调方法, 钩子函数执行完会调用此方法 
        ///     实现这个方法后，不要忘记return <see cref="CallNextHookEx(int, IntPtr, IntPtr)"/>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected abstract IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///     调用下一个钩子
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected IntPtr CallNextHookEx(int nCode, IntPtr wParam, IntPtr lParam)
            => User32Dll.CallNextHookEx(_HookId, nCode, wParam, lParam);
    }
}
