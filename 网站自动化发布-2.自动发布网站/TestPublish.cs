using System;
using System.IO;

class TestPublish {
    static void Main() {
        Console.WriteLine("开始执行测试发布流程...");
        
        // 模拟发布流程
        string deploymentDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy";
        string svnVersion = "102860";
        string maxScriptNumber = "8473";
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string solutionName = "Qxy.PdmPortal";
        string newFolderName = String.Format("{0}_{1}_{2}_{3}", currentDate, solutionName, svnVersion, maxScriptNumber);
        string newFolderPath = Path.Combine(deploymentDir, newFolderName);
        string deployDir = Path.Combine(deploymentDir, "Deploy");
        
        Console.WriteLine(String.Format("部署目录: {0}", deploymentDir));
        Console.WriteLine(String.Format("新文件夹路径: {0}", newFolderPath));
        Console.WriteLine(String.Format("Deploy目录路径: {0}", deployDir));
        Console.WriteLine(String.Format("Deploy目录是否存在: {0}", Directory.Exists(deployDir)));
        
        if (Directory.Exists(deployDir)) {
            int deployFileCount = Directory.GetFiles(deployDir, "*", SearchOption.AllDirectories).Length;
            Console.WriteLine(String.Format("Deploy目录中的文件数量: {0}", deployFileCount));
            
            // 确保新文件夹不存在
            if (Directory.Exists(newFolderPath)) {
                Directory.Delete(newFolderPath, true);
                Console.WriteLine("已删除已存在的新文件夹");
            }
            
            // 创建新文件夹
            Directory.CreateDirectory(newFolderPath);
            Console.WriteLine("新文件夹创建完成");
            
            // 复制文件
            Console.WriteLine("开始复制文件...");
            CopyDirectory(deployDir, newFolderPath);
            
            // 检查新文件夹中的文件数量
            int newFileCount = Directory.GetFiles(newFolderPath, "*", SearchOption.AllDirectories).Length;
            Console.WriteLine(String.Format("新文件夹中的文件数量: {0}", newFileCount));
            Console.WriteLine(String.Format("复制成功: {0}", deployFileCount == newFileCount));
        } else {
            Console.WriteLine("Deploy目录不存在");
        }
        
        Console.WriteLine("测试发布流程执行完成！");
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
    
    static void CopyDirectory(string sourceDir, string targetDir) {
        // 确保目标目录存在
        if (!Directory.Exists(targetDir)) {
            Directory.CreateDirectory(targetDir);
        }

        // 复制文件
        foreach (string file in Directory.GetFiles(sourceDir)) {
            string fileName = Path.GetFileName(file);
            string targetFilePath = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFilePath, true);
        }

        // 递归复制子目录
        foreach (string subDir in Directory.GetDirectories(sourceDir)) {
            string subDirName = Path.GetFileName(subDir);
            string targetSubDirPath = Path.Combine(targetDir, subDirName);
            CopyDirectory(subDir, targetSubDirPath);
        }
    }
}