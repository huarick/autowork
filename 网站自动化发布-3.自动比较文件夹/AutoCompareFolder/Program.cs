using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutoCompareFolder {
    internal class Program {
        static void Main(string[] args) {
            try {
                string beyondComparePath = @"C:\Program Files\Beyond Compare 4\BComp.exe";

                // 让用户选择要处理的网站
                Console.WriteLine("请选择要处理的网站：");
                Console.WriteLine("1. Qxy.PdmPortalWeb");
                Console.WriteLine("2. Qxy.PlatformProductPortal");
                Console.Write("请输入选项 (1 , 2 , 3)：");
                string choice = Console.ReadLine();

                // 根据用户选择实例化不同的项目
                PublishProject project = null;
                if (choice == "1") {
                    project = new PublishProject {
                        ProjectName = "Qxy.PdmPortalWeb",
                        ProjectPath = @"D:\QXYSVN\trunk\Qxy.PdmPortalWeb",
                        PublishConfigPath = @"D:\QXYSVN\trunk\Qxy.PdmPortalWeb\Properties\PublishProfiles",
                        PublishFolderPath = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy",
                        IsWebProject = true,
                        RemoteFolderPath = @"D:\RemoteFB\FBpdm"
                    };
                } else if (choice == "2") {
                    project = new PublishProject {
                        ProjectName = "Qxy.PlatformProductPortal",
                        ProjectPath = @"D:\QXYSVN\trunk\Qxy.PlatformProductPortal",
                        PublishConfigPath = @"D:\QXYSVN\trunk\Qxy.PlatformProductPortal\Properties\PublishProfiles",
                        PublishFolderPath = @"D:\QXYSVN\trunk\deployment\Qxy.PlatformProductPortal",
                        IsWebProject = true,
                        RemoteFolderPath = @"D:\RemoteFB\FBPlatfrmProduct"
                    };
                } else if (choice == "3") {
                    project = new PublishProject {
                        ProjectName = "PlatformSyn",
                        ProjectPath = @"D:\QXYSVN\trunk\Qxy.SchedulingServer.PlatformSyn",
                        PublishConfigPath = string.Empty,
                        PublishFolderPath = @"D:\QXYSVN\trunk\Qxy.SchedulingServer.PlatformSyn\Qxy.SchedulingServer.PlatformSyn\bin\Debug",
                        IsWebProject = true,
                        RemoteFolderPath = @"D:\RemoteFB\FBPlatformSyn"
                    };
                } else {
                    Console.WriteLine("错误：无效的选择，请输入 1 或 2 或3");
                    return;
                }

                Console.WriteLine($"\n您选择了处理网站：{project.ProjectName}");
                Console.WriteLine($"项目路径：{project.ProjectPath}");
                Console.WriteLine($"发布文件夹路径：{project.PublishFolderPath}");
                Console.WriteLine($"远程文件夹路径：{project.RemoteFolderPath}");
                Console.WriteLine();

                string sourceBasePath = project.PublishFolderPath;
                string targetBasePath = project.RemoteFolderPath;

                if (!Directory.Exists(sourceBasePath)) {
                    Console.WriteLine($"错误：源目录不存在 - {sourceBasePath}");
                    return;
                }

                if (!Directory.Exists(targetBasePath)) {
                    Console.WriteLine($"错误：目标目录不存在 - {targetBasePath}");
                    return;
                }

                if (!File.Exists(beyondComparePath)) {
                    Console.WriteLine($"错误：Beyond Compare 4 未找到 - {beyondComparePath}");
                    return;
                }

                var sourceFolders = Directory.GetDirectories(sourceBasePath)
                    .OrderByDescending(d => Directory.GetCreationTime(d))
                    .ToList();

                string latestDeployFolder = null;
                string deployFolderName = null;
                string targetDeployFolder = null;

                if (sourceFolders.Count > 0) {
                    // 源目录中有发布文件夹，执行移动操作
                    latestDeployFolder = sourceFolders[0];
                    deployFolderName = Path.GetFileName(latestDeployFolder);
                    targetDeployFolder = Path.Combine(targetBasePath, deployFolderName);

                    if (Directory.Exists(targetDeployFolder)) {
                        Directory.Delete(targetDeployFolder, true);
                    }

                    Console.WriteLine($"开始移动文件夹：{latestDeployFolder} -> {targetDeployFolder}");
                    Console.WriteLine($"源文件夹是否存在：{Directory.Exists(latestDeployFolder)}");
                    if (Directory.Exists(latestDeployFolder)) {
                        try {
                            var sourceFiles = Directory.GetFiles(latestDeployFolder, "*", SearchOption.AllDirectories);
                            Console.WriteLine($"源文件夹中包含 {sourceFiles.Length} 个文件");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"无法访问源文件夹内容：{ex.Message}");
                        }
                    }
                    
                    try {
                        Directory.Move(latestDeployFolder, targetDeployFolder);
                        Console.WriteLine($"已将最新发布文件夹 {deployFolderName} 剪切到目标目录");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"移动文件夹时发生错误：{ex.Message}");
                        return;
                    }
                    
                    Console.WriteLine($"目标文件夹是否存在：{Directory.Exists(targetDeployFolder)}");
                    if (Directory.Exists(targetDeployFolder)) {
                        try {
                            var targetFiles = Directory.GetFiles(targetDeployFolder, "*", SearchOption.AllDirectories);
                            Console.WriteLine($"目标文件夹中包含 {targetFiles.Length} 个文件");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"无法访问目标文件夹内容：{ex.Message}");
                        }
                    }
                } else {
                    // 源目录为空，直接从目标目录中找出最新的发布文件夹
                    Console.WriteLine("警告：源目录中没有发布文件夹，直接从目标目录中查找最新发布文件夹");
                    
                    var targetFolders = Directory.GetDirectories(targetBasePath)
                        .Where(d => !Path.GetFileName(d).ToLower().Contains("_fb"))
                        .OrderByDescending(d => Directory.GetCreationTime(d))
                        .ToList();
                    
                    if (targetFolders.Count == 0) {
                        Console.WriteLine("错误：目标目录中也没有发布文件夹");
                        return;
                    }
                    
                    targetDeployFolder = targetFolders[0];
                    deployFolderName = Path.GetFileName(targetDeployFolder);
                    Console.WriteLine($"从目标目录中找到最新发布文件夹：{deployFolderName}");
                    
                    Console.WriteLine($"目标文件夹是否存在：{Directory.Exists(targetDeployFolder)}");
                    if (Directory.Exists(targetDeployFolder)) {
                        try {
                            var targetFiles = Directory.GetFiles(targetDeployFolder, "*", SearchOption.AllDirectories);
                            Console.WriteLine($"目标文件夹中包含 {targetFiles.Length} 个文件");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"无法访问目标文件夹内容：{ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"开始扫描目标目录：{targetBasePath}");
                var allFolders = Directory.GetDirectories(targetBasePath);
                Console.WriteLine($"目标目录中总共有 {allFolders.Length} 个文件夹");
                
                var existingFolders = allFolders
                    .Where(d => !Path.GetFileName(d).ToLower().Contains("_fb"))
                    .OrderByDescending(d => Directory.GetCreationTime(d))
                    .ToList();

                Console.WriteLine($"过滤后（排除_fb文件夹）剩余 {existingFolders.Count} 个文件夹");
                foreach (var folder in existingFolders) {
                    Console.WriteLine($"  - {Path.GetFileName(folder)} (创建时间: {Directory.GetCreationTime(folder)})");
                }

                if (existingFolders.Count < 2) {
                    Console.WriteLine("警告：目标目录中没有足够的历史发布文件夹进行比较");
                    return;
                }

                Console.WriteLine("\n开始执行BeyondCompare比较操作...");
                string previousDeployFolder = existingFolders[1];
                Console.WriteLine($"选择的历史发布文件夹：{Path.GetFileName(previousDeployFolder)}");
                
                string resultFolderName = deployFolderName + "_fb";
                string resultFolder = Path.Combine(targetBasePath, resultFolderName);
                Console.WriteLine($"结果文件夹将保存到：{resultFolderName}");

                if (Directory.Exists(resultFolder)) {
                    Console.WriteLine("结果文件夹已存在，正在删除...");
                    Directory.Delete(resultFolder, true);
                }

                Console.WriteLine("创建结果文件夹...");
                Directory.CreateDirectory(resultFolder);

                Console.WriteLine($"BeyondCompare路径：{beyondComparePath}");
                
                // 检查目标文件夹和历史文件夹的访问权限
                Console.WriteLine("检查文件夹访问权限...");
                try {
                    Console.WriteLine($"检查目标文件夹权限：{targetDeployFolder}");
                    var testFiles1 = Directory.GetFiles(targetDeployFolder, "*", SearchOption.TopDirectoryOnly);
                    Console.WriteLine($"目标文件夹顶层文件数：{testFiles1.Length}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"无法访问目标文件夹：{ex.Message}");
                    return;
                }
                
                try {
                    Console.WriteLine($"检查历史文件夹权限：{previousDeployFolder}");
                    var testFiles2 = Directory.GetFiles(previousDeployFolder, "*", SearchOption.TopDirectoryOnly);
                    Console.WriteLine($"历史文件夹顶层文件数：{testFiles2.Length}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"无法访问历史文件夹：{ex.Message}");
                    return;
                }
                
                // 直接使用C#代码实现文件夹比较和复制，这是最可靠的方法
                Console.WriteLine("开始使用C#代码实现文件夹比较和复制...");
                
                try {
                    // 实现文件比较和复制逻辑
                    CompareAndCopyFiles(targetDeployFolder, previousDeployFolder, resultFolder);

                    // 检查结果文件夹中的文件数量
                    if (Directory.Exists(resultFolder)) {
                        var resultFiles = Directory.GetFiles(resultFolder, "*", SearchOption.AllDirectories);
                        Console.WriteLine($"结果文件夹中包含 {resultFiles.Length} 个文件");
                    }
                    
                    Console.WriteLine("操作完成！");
                }
                catch (Exception ex) {
                    Console.WriteLine($"执行文件比较和复制时发生错误：{ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    return;
                }
            }
            catch (Exception ex) {
                Console.WriteLine("==============================================");
                Console.WriteLine("发生严重错误：");
                Console.WriteLine($"错误消息：{ex.Message}");
                Console.WriteLine($"错误类型：{ex.GetType().FullName}");
                Console.WriteLine($"错误堆栈：{ex.StackTrace}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"内部错误：{ex.InnerException.Message}");
                    Console.WriteLine($"内部错误堆栈：{ex.InnerException.StackTrace}");
                }
                Console.WriteLine("==============================================");
            }
            finally {
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        // 比较两个文件夹中的文件并复制不同的文件到结果文件夹
        static void CompareAndCopyFiles(string targetFolder, string previousFolder, string resultFolder) {
            Console.WriteLine("开始比较文件...");
            int copiedCount = 0;
            int totalFiles = 0;

            // 遍历目标文件夹中的所有文件
            foreach (string targetFile in Directory.GetFiles(targetFolder, "*", SearchOption.AllDirectories)) {
                totalFiles++;
                // 计算相对路径
                string relativePath = targetFile.Substring(targetFolder.Length).TrimStart('\\');
                string previousFile = Path.Combine(previousFolder, relativePath);

                // 检查文件是否在历史文件夹中存在
                bool fileExistsInPrevious = File.Exists(previousFile);
                bool filesAreDifferent = false;

                if (fileExistsInPrevious) {
                    // 文件存在，比较内容
                    filesAreDifferent = !FilesAreEqual(targetFile, previousFile);

                    // 对于App_global.asax.compiled文件，添加详细的日志输出
                    if (relativePath.Contains("App_global.asax.compiled")) {
                        Console.WriteLine($"详细比较：{relativePath}");
                        Console.WriteLine($"  目标文件：{targetFile}");
                        Console.WriteLine($"  历史文件：{previousFile}");
                        Console.WriteLine($"  文件大小相同：{new FileInfo(targetFile).Length == new FileInfo(previousFile).Length}");
                        Console.WriteLine($"  文件内容不同：{filesAreDifferent}");
                    }
                }

                // 只有当文件不存在于历史文件夹或文件内容不同时，才复制到结果文件夹
                if (!fileExistsInPrevious || filesAreDifferent) {
                    string resultFile = Path.Combine(resultFolder, relativePath);

                    // 确保结果文件夹的目录结构存在
                    string resultDir = Path.GetDirectoryName(resultFile);
                    if (!Directory.Exists(resultDir)) {
                        Directory.CreateDirectory(resultDir);
                    }

                    // 复制文件到结果文件夹
                    File.Copy(targetFile, resultFile, true);
                    copiedCount++;

                    if (!fileExistsInPrevious) {
                        Console.WriteLine($"复制新文件：{relativePath}");
                    } else {
                        Console.WriteLine($"复制修改的文件：{relativePath}");
                    }
                }
            }

            Console.WriteLine($"文件比较完成，共检查 {totalFiles} 个文件，复制 {copiedCount} 个文件到结果文件夹");
        }

        // 比较两个文件的内容是否相同（使用快速二进制比较）
        static bool FilesAreEqual(string file1, string file2) {
            // 检查文件是否存在
            if (!File.Exists(file1) || !File.Exists(file2)) {
                return false;
            }

            using (FileStream fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read)) {
                using (FileStream fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read)) {
                    if (fs1.Length != fs2.Length) {
                        return false;
                    }

                    byte[] buffer1 = new byte[4096];
                    byte[] buffer2 = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = fs1.Read(buffer1, 0, buffer1.Length)) > 0) {
                        int bytesRead2 = fs2.Read(buffer2, 0, bytesRead);
                        if (bytesRead != bytesRead2) {
                            return false;
                        }

                        for (int i = 0; i < bytesRead; i++) {
                            if (buffer1[i] != buffer2[i]) {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }
        }
    }

    // 发布项目类
    public class PublishProject {
        // 发布的项目名
        public string ProjectName { get; set; }
        
        // 发布的项目路径
        public string ProjectPath { get; set; }
        
        // 发布配置路径
        public string PublishConfigPath { get; set; }
        
        // 发布的文件夹路径
        public string PublishFolderPath { get; set; }
        
        // 是否网站项目（默认为是）
        public bool IsWebProject { get; set; } = true;
        
        // 远程的文件夹路径
        public string RemoteFolderPath { get; set; }
    }

}