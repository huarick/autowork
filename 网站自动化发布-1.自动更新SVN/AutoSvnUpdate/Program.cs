using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AutoSvnUpdate {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("=============开始进行SVN更新===============");
            
            try {
                // 更新第一个目录：D:\QXYSVN\trunk\
                Console.WriteLine("正在更新 D:\\QXYSVN\\trunk\\...");
                if (UpdateSvnDirectory("D:\\QXYSVN\\trunk\\")) {
                    Console.WriteLine("更新truck成功");
                } else {
                    Console.WriteLine("更新truck失败");
                    WaitForUserConfirmation();
                    return;
                }
                
                // 更新第二个目录：D:\QXYSVN\database scripts\19buy
                Console.WriteLine("正在更新 D:\\QXYSVN\\database scripts\\19buy...");
                if (UpdateSvnDirectory("D:\\QXYSVN\\database scripts\\19buy")) {
                    Console.WriteLine("更新scripts成功");
                } else {
                    Console.WriteLine("更新scripts失败");
                    WaitForUserConfirmation();
                    return;
                }
                
                Console.WriteLine("=============SVN更新结束==================");
            } catch (Exception ex) {
                Console.WriteLine($"错误：{ex.Message}");
                Console.ReadLine();
            } 
        }
        
        static bool UpdateSvnDirectory(string directory) {
            if (!Directory.Exists(directory)) {
                Console.WriteLine($"错误：目录不存在：{directory}");
                return false;
            }
            
            try {
                // 尝试使用TortoiseProc.exe执行SVN更新操作
                string tortoiseProcPath = "C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe";
                
                if (!File.Exists(tortoiseProcPath)) {
                    Console.WriteLine($"错误：找不到TortoiseProc.exe文件：{tortoiseProcPath}");
                    Console.WriteLine("请确保已安装TortoiseSVN客户端。");
                    return false;
                }
                
                ProcessStartInfo startInfo = new ProcessStartInfo {
                    FileName = tortoiseProcPath,
                    Arguments = $"/command:update /path:\"{directory}\" /closeonend:1",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                using (Process process = Process.Start(startInfo)) {
                    process.WaitForExit();
                    
                    // TortoiseProc.exe的退出码通常为0，即使更新失败
                    // 因为它是一个图形界面工具，错误处理在界面中完成
                    return true;
                }
            } catch (Exception ex) {
                Console.WriteLine($"执行SVN更新时出错：{ex.Message}");
                return false;
            }
        }
        
        static void WaitForUserConfirmation() {
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}
