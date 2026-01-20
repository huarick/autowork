using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPublishProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 检查是否是测试模式
            bool testMode = args.Length > 0 && args[0] == "test";

            if (testMode)
            {
                Console.WriteLine("开始执行测试模式...");
                TestFileCopy();
                return;
            }

            try
            {
                Console.WriteLine("开始执行自动化发布流程...");

                // 1. 检查目录结构
                CheckDirectories();

                // 2. 获取SVN版本号
                string svnVersion = GetSvnVersion();
                Console.WriteLine($"SVN版本号: {svnVersion}");

                // 3. 获取最大脚本编号
                string maxScriptNumber = GetMaxScriptNumber();
                Console.WriteLine($"最大脚本编号: {maxScriptNumber}");

                Console.WriteLine("=======================================");
                Console.WriteLine("开始执行第1步: 发布网站");
                Console.WriteLine("=======================================");
                // 4. 发布网站
                PublishWebsite();

                Console.WriteLine("=======================================");
                Console.WriteLine("开始执行第2步: 重命名文件夹");
                Console.WriteLine("=======================================");
                // 5. 重命名文件夹
                RenameDeploymentFolder(svnVersion, maxScriptNumber);

                Console.WriteLine("自动化发布流程执行完成！");
                Console.WriteLine("请按任意键继续...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行过程中出现错误: {ex.Message}");
            }
        }

        static void TestFileCopy()
        {
            string sourceDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy\Deploy";
            string targetDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy\TestCopy";

            Console.WriteLine($"源目录: {sourceDir}");
            Console.WriteLine($"目标目录: {targetDir}");
            Console.WriteLine($"源目录是否存在: {Directory.Exists(sourceDir)}");

            if (Directory.Exists(sourceDir))
            {
                int sourceFileCount = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).Length;
                Console.WriteLine($"源目录中的文件数量: {sourceFileCount}");

                // 确保目标目录不存在
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                    Console.WriteLine("已删除已存在的目标目录");
                }

                // 创建目标目录
                Directory.CreateDirectory(targetDir);
                Console.WriteLine("目标目录创建完成");

                // 复制文件
                Console.WriteLine("开始复制文件...");
                CopyDirectory(sourceDir, targetDir);

                // 检查目标目录中的文件数量
                int targetFileCount = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories).Length;
                Console.WriteLine($"目标目录中的文件数量: {targetFileCount}");
                Console.WriteLine($"复制成功: {sourceFileCount == targetFileCount}");
            }
            else
            {
                Console.WriteLine("源目录不存在");
            }
        }

        static void CopyDirectory(string sourceDir, string targetDir)
        {
            // 确保目标目录存在
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // 复制文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFilePath = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFilePath, true);
            }

            // 递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                string targetSubDirPath = Path.Combine(targetDir, subDirName);
                CopyDirectory(subDir, targetSubDirPath);
            }
        }

        static void CheckDirectories()
        {
            string solutionDir = @"D:\QXYSVN\trunk\Qxy.PdmPortal";
            string webProjectDir = @"D:\QXYSVN\trunk\Qxy.PdmPortal\Qxy.Pdm.PortalWeb";
            string deploymentDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy";
            string scriptsDir = @"D:\QXYSVN\database scripts\19buy";

            if (!Directory.Exists(solutionDir))
            {
                throw new DirectoryNotFoundException($"解决方案目录不存在: {solutionDir}");
            }
            if (!Directory.Exists(webProjectDir))
            {
                throw new DirectoryNotFoundException($"网站项目目录不存在: {webProjectDir}");
            }
            if (!Directory.Exists(deploymentDir))
            {
                throw new DirectoryNotFoundException($"发布目录不存在: {deploymentDir}");
            }
            if (!Directory.Exists(scriptsDir))
            {
                throw new DirectoryNotFoundException($"数据库脚本目录不存在: {scriptsDir}");
            }

            Console.WriteLine("目录结构检查完成");
        }

        static string GetSvnVersion()
        {
            string svnVersion = string.Empty;
            string trunkPath = @"D:\QXYSVN\trunk";
            string subWCRevPath = string.Empty;
            bool hasError = false;

            // 检查Tortoise SVN的SubWCRev.exe工具位置
            if (File.Exists(@"C:\Program Files\TortoiseSVN\bin\SubWCRev.exe"))
            {
                subWCRevPath = @"C:\Program Files\TortoiseSVN\bin\SubWCRev.exe";
            }
            else if (File.Exists(@"C:\Program Files (x86)\TortoiseSVN\bin\SubWCRev.exe"))
            {
                subWCRevPath = @"C:\Program Files (x86)\TortoiseSVN\bin\SubWCRev.exe";
            }

            if (!string.IsNullOrEmpty(subWCRevPath) && Directory.Exists(trunkPath))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = subWCRevPath,
                        Arguments = $"{trunkPath}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(trunkPath)
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            // 从输出中提取版本号
                            // 输出格式类似: "Last committed at revision 102858"
                            if (output.Contains("Last committed at revision"))
                            {
                                string[] lines = output.Split('\n');
                                foreach (string line in lines)
                                {
                                    if (line.Contains("Last committed at revision"))
                                    {
                                        string[] parts = line.Split(' ');
                                        svnVersion = parts[parts.Length - 1].Trim();
                                        break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(svnVersion))
                                {
                                    Console.WriteLine($"成功获取SVN版本号: {svnVersion}");
                                }
                                else
                                {
                                    Console.WriteLine("无法从SubWCRev输出中提取版本号");
                                    hasError = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("SubWCRev输出格式不符合预期");
                                hasError = true;
                            }
                        }
                        else
                        {
                            string errorOutput = process.StandardError.ReadToEnd();
                            Console.WriteLine($"获取SVN版本号失败: {errorOutput}");
                            hasError = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"获取SVN版本号时出现异常: {ex.Message}");
                    hasError = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(subWCRevPath))
                {
                    Console.WriteLine("未找到Tortoise SVN命令行工具SubWCRev.exe");
                    hasError = true;
                }
                if (!Directory.Exists(trunkPath))
                {
                    Console.WriteLine($"Trunk目录不存在: {trunkPath}");
                    hasError = true;
                }
            }

            // 如果无法获取版本号，尝试从其他途径获取
            if (string.IsNullOrEmpty(svnVersion))
            {
                // 尝试使用svn.exe
                string svnExePath = string.Empty;
                if (File.Exists(@"C:\Program Files\TortoiseSVN\bin\svn.exe"))
                {
                    svnExePath = @"C:\Program Files\TortoiseSVN\bin\svn.exe";
                }
                else if (File.Exists(@"C:\Program Files (x86)\TortoiseSVN\bin\svn.exe"))
                {
                    svnExePath = @"C:\Program Files (x86)\TortoiseSVN\bin\svn.exe";
                }

                if (!string.IsNullOrEmpty(svnExePath))
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = svnExePath,
                            Arguments = $"info --show-item revision {trunkPath}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = Path.GetDirectoryName(trunkPath)
                        };

                        using (Process process = Process.Start(psi))
                        {
                            process.WaitForExit();
                            if (process.ExitCode == 0)
                            {
                                svnVersion = process.StandardOutput.ReadToEnd().Trim();
                                Console.WriteLine($"成功通过svn.exe获取SVN版本号: {svnVersion}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"使用svn.exe获取SVN版本号时出现异常: {ex.Message}");
                    }
                }
            }

            // 如果仍然无法获取版本号，抛出异常
            if (string.IsNullOrEmpty(svnVersion))
            {
                hasError = true;
                throw new Exception("无法获取SVN版本号，请确保Tortoise SVN已正确安装");
            }

            // 如果有错误，等待用户确认
            if (hasError)
            {
                Console.WriteLine("\n请确认以上错误信息，按任意键继续...");
                // 检查是否在交互式环境中运行，并添加超时机制
                if (Environment.UserInteractive)
                {
                    // 设置5秒超时
                    Console.WriteLine("(5秒后自动继续...)");
                    Task.Delay(5000).Wait();
                    Console.WriteLine("超时，自动继续...");
                }
                else
                {
                    Console.WriteLine("非交互式环境，自动继续...");
                }
            }

            return svnVersion;
        }

        static string GetMaxScriptNumber()
        {
            string scriptsDir = @"D:\QXYSVN\database scripts\19buy";
            int maxNumber = 0;
            bool hasError = false;

            if (Directory.Exists(scriptsDir))
            {
                try
                {
                    var files = Directory.GetFiles(scriptsDir, "*.sql")
                        .Concat(Directory.GetFiles(scriptsDir, "*.txt"))
                        .Where(f => !Path.GetFileName(f).Contains("SQL函数") && !Path.GetFileName(f).Contains("触发器") && !Path.GetFileName(f).Contains("存储过程") && !Path.GetFileName(f).Contains("视图") && !Path.GetFileName(f).Contains("数据表迁移") && !Path.GetFileName(f).Contains("子系统初始化脚本"));

                    if (files.Any())
                    {
                        foreach (var file in files)
                        {
                            string fileName = Path.GetFileName(file);
                            if (fileName.Contains("_"))
                            {
                                string numberPart = fileName.Split('_')[0];
                                if (int.TryParse(numberPart, out int number))
                                {
                                    if (number > maxNumber)
                                    {
                                        maxNumber = number;
                                    }
                                }
                            }
                        }
                        Console.WriteLine($"成功获取最大脚本编号: {maxNumber}");
                    }
                    else
                    {
                        Console.WriteLine("未找到符合条件的脚本文件");
                        hasError = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"获取最大脚本编号时出现异常: {ex.Message}");
                    hasError = true;
                }
            }
            else
            {
                Console.WriteLine($"数据库脚本目录不存在: {scriptsDir}");
                hasError = true;
            }

            // 如果有错误，等待用户确认
            if (hasError)
            {
                Console.WriteLine("\n请确认以上错误信息，按任意键继续...");
                // 检查是否在交互式环境中运行，并添加超时机制
                if (Environment.UserInteractive)
                {
                    // 设置5秒超时
                    Console.WriteLine("(5秒后自动继续...)");
                    Task.Delay(5000).Wait();
                    Console.WriteLine("超时，自动继续...");
                }
                else
                {
                    Console.WriteLine("非交互式环境，自动继续...");
                }
            }

            return maxNumber.ToString();
        }

        static void PublishWebsite()
        {
            Console.WriteLine("开始构建和发布网站...");

            string projectPath = @"D:\QXYSVN\trunk\Qxy.PdmPortal\Qxy.Pdm.PortalWeb\Qxy.Pdm.PortalWeb.csproj";
            string publishProfilePath = @"d:\Rick\自动化项目\网站自动化发布-2.自动发布网站\TestPublishProfile.pubxml";
            string msbuildPath = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe";
            bool hasError = false;

            // 检查MSBuild是否存在
            if (!File.Exists(msbuildPath))
            {
                msbuildPath = @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe";
                if (!File.Exists(msbuildPath))
                {
                    msbuildPath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe";
                    if (!File.Exists(msbuildPath))
                    {
                        // 尝试使用.NET Framework自带的MSBuild
                        msbuildPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";
                        if (!File.Exists(msbuildPath))
                        {
                            Console.WriteLine("错误: 未找到MSBuild");
                            hasError = true;
                        }
                    }
                }
            }

            if (!File.Exists(projectPath))
            {
                Console.WriteLine($"项目文件不存在: {projectPath}");
                hasError = true;
            }

            if (!File.Exists(publishProfilePath))
            {
                Console.WriteLine($"发布配置文件不存在: {publishProfilePath}");
                hasError = true;
            }

            if (hasError)
            {
                Console.WriteLine("\n请确认以上错误信息，按任意键继续...");
                // 检查是否在交互式环境中运行，并添加超时机制
                if (Environment.UserInteractive)
                {
                    // 设置5秒超时
                    Console.WriteLine("发布网站有错，请确认...");
                    Console.ReadLine();
                    Task.Delay(5000).Wait();
                    Console.WriteLine("超时，自动继续...");
                }
                else
                {
                    Console.WriteLine("非交互式环境，自动继续...");
                }
                return;
            }

            try
            {
                // 检查发布目录是否存在，如果不存在则创建
                string publishDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy\Deploy";
                if (!Directory.Exists(publishDir))
                {
                    try
                    {
                        Directory.CreateDirectory(publishDir);
                        Console.WriteLine($"创建发布目录: {publishDir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"创建发布目录失败: {ex.Message}");
                        hasError = true;
                    }
                }

                if (!hasError)
                {
                    // 使用MSBuild执行发布操作
                    Console.WriteLine($"使用MSBuild执行发布操作...");
                    Console.WriteLine($"MSBuild路径: {msbuildPath}");
                    Console.WriteLine($"项目路径: {projectPath}");
                    Console.WriteLine($"发布配置: {publishProfilePath}");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = msbuildPath,
                        Arguments = $"\"{projectPath}\" /p:DeployOnBuild=true /p:PublishProfile=\"{publishProfilePath}\" /p:Configuration=Release",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    Console.WriteLine("开始执行MSBuild命令...");
                    Console.WriteLine($"MSBuild命令: {psi.FileName} {psi.Arguments}");
                    
                    try {
                        using (Process process = Process.Start(psi)) {
                            Console.WriteLine("MSBuild命令已启动，等待执行完成...");
                            
                            // 异步读取输出流，避免死锁
                            var outputTask = process.StandardOutput.ReadToEndAsync();
                            var errorTask = process.StandardError.ReadToEndAsync();
                            
                            // 设置超时时间为10分钟
                            if (!process.WaitForExit(600000)) {
                                Console.WriteLine("MSBuild命令执行超时，已终止");
                                process.Kill();
                                hasError = true;
                            } else {
                                Console.WriteLine($"MSBuild命令执行完成，退出代码: {process.ExitCode}");
                                
                                // 等待输出读取完成
                                string output = outputTask.Result;
                                string errorOutput = errorTask.Result;
                                
                                if (process.ExitCode != 0) {
                                    Console.WriteLine($"发布失败: {errorOutput}");
                                    Console.WriteLine($"输出: {output}");
                                    hasError = true;
                                } else {
                                    Console.WriteLine($"MSBuild输出: {output.Substring(0, Math.Min(500, output.Length))}...");
                                    Console.WriteLine("网站发布成功");
                                    
                                    // 检查发布目录中是否有文件
                                    if (Directory.Exists(publishDir)) {
                                        int fileCount = Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories).Length;
                                        Console.WriteLine($"发布目录中的文件数量: {fileCount}");
                                        if (fileCount == 0) {
                                            Console.WriteLine("警告: 发布目录中没有文件");
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"执行MSBuild命令时出现异常: {ex.Message}");
                        hasError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发布网站时出现异常: {ex.Message}");
                hasError = true;
            }

            // 如果有错误，等待用户确认
            if (hasError)
            {
                Console.WriteLine("\n请确认以上错误信息，按任意键继续...");
                // 检查是否在交互式环境中运行，并添加超时机制
                if (Environment.UserInteractive)
                {
                    // 设置5秒超时
                    Console.WriteLine("(5秒后自动继续...)");
                    Task.Delay(5000).Wait();
                    Console.WriteLine("超时，自动继续...");
                } else {
                    Console.WriteLine("非交互式环境，自动继续...");
                }
            }
            
            Console.WriteLine("=======================================");
            Console.WriteLine("PublishWebsite方法执行完成...");
            Console.WriteLine("=======================================");
        }

        static void RenameDeploymentFolder(string svnVersion, string maxScriptNumber)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("开始执行RenameDeploymentFolder方法...");
            Console.WriteLine("========================================");

            string deploymentDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy";
            string deployDir = Path.Combine(deploymentDir, "Deploy");
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            string solutionName = "Qxy.PdmPortal";
            string newFolderName = $"{currentDate}_{solutionName}_{svnVersion}_{maxScriptNumber}";
            string newFolderPath = Path.Combine(deploymentDir, newFolderName);
            bool hasError = false;

            Console.WriteLine($"开始重命名文件夹...");
            Console.WriteLine($"基础目录: {deploymentDir}");
            Console.WriteLine($"Deploy目录: {deployDir}");
            Console.WriteLine($"新文件夹: {newFolderPath}");

            // 检查基础文件夹和Deploy文件夹是否存在
            if (!Directory.Exists(deploymentDir))
            {
                Console.WriteLine($"基础文件夹不存在: {deploymentDir}");
                hasError = true;
            }
            else if (!Directory.Exists(deployDir))
            {
                Console.WriteLine($"Deploy目录不存在: {deployDir}");
                hasError = true;
            }

            if (!hasError)
            {
                try
                {
                    // 检查新文件夹是否已存在
                    if (Directory.Exists(newFolderPath))
                    {
                        try
                        {
                            Directory.Delete(newFolderPath, true);
                            Console.WriteLine($"已删除已存在的新文件夹: {newFolderPath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"删除已存在的新文件夹失败: {ex.Message}");
                            hasError = true;
                        }
                    }

                    if (!hasError)
                    {
                        // 直接重命名Deploy文件夹为新的文件夹名称
                        try
                        {
                            Console.WriteLine($"开始重命名Deploy文件夹为: {newFolderName}");
                            
                            // 尝试释放可能的文件锁定
                            Console.WriteLine("尝试释放文件锁定...");
                            System.Threading.Thread.Sleep(2000); // 等待2秒，让可能的进程释放锁定
                            
                            // 尝试重命名，最多重试3次
                            int retryCount = 0;
                            const int maxRetries = 3;
                            bool renameSuccess = false;
                            
                            while (retryCount < maxRetries && !renameSuccess)
                            {
                                try
                                {
                                    Directory.Move(deployDir, newFolderPath);
                                    renameSuccess = true;
                                    Console.WriteLine("文件夹重命名完成");
                                }
                                catch (IOException ex)
                                {
                                    retryCount++;
                                    Console.WriteLine($"重命名失败 (尝试 {retryCount}/{maxRetries}): {ex.Message}");
                                    if (retryCount < maxRetries)
                                    {
                                        Console.WriteLine("等待3秒后重试...");
                                        System.Threading.Thread.Sleep(3000);
                                    }
                                }
                            }
                            
                            if (!renameSuccess)
                            {
                                throw new Exception("多次尝试后仍然无法重命名文件夹");
                            }
                            
                            // 检查新文件夹中的文件数量
                            int newFileCount = Directory.GetFiles(newFolderPath, "*", SearchOption.AllDirectories).Length;
                            Console.WriteLine($"新文件夹中的文件数量: {newFileCount}");

                            // 不需要创建新的Deploy文件夹，因为MSBuild会在发布时自动创建
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"重命名文件夹失败: {ex.Message}");
                            hasError = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重命名文件夹时出现异常: {ex.Message}");
                    hasError = true;
                }
            }

            // 如果有错误，等待用户确认
            if (hasError)
            {
                Console.WriteLine("\n请确认以上错误信息，按任意键继续...");
                // 检查是否在交互式环境中运行，并添加超时机制
                if (Environment.UserInteractive)
                {
                    // 设置5秒超时
                    Console.WriteLine("重命名发布目录有错，请确认...");
                    Console.ReadLine();
                    Console.WriteLine("超时，自动继续...");
                }
                else
                {
                    Console.WriteLine("非交互式环境，自动继续...");
                }
            }
            
            Console.WriteLine("========================================");
            Console.WriteLine("RenameDeploymentFolder方法执行完成...");
            Console.WriteLine("========================================");
        }
    }
}
