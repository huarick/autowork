using System;
using System.IO;

class TestCopyDirectory {
    static void Main() {
        string sourceDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy\Deploy";
        string targetDir = @"D:\QXYSVN\trunk\deployment\Qxy.PdmPortalWeb_deploy\TestCopy";
        
        Console.WriteLine($"源目录: {sourceDir}");
        Console.WriteLine($"目标目录: {targetDir}");
        Console.WriteLine($"源目录是否存在: {Directory.Exists(sourceDir)}");
        
        if (Directory.Exists(sourceDir)) {
            int sourceFileCount = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).Length;
            Console.WriteLine($"源目录中的文件数量: {sourceFileCount}");
            
            // 确保目标目录不存在
            if (Directory.Exists(targetDir)) {
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
        } else {
            Console.WriteLine("源目录不存在");
        }
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