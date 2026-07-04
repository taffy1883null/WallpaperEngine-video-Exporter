using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace WallpaperEngineExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            // 强制使用 UTF-8 防止乱码，并支持显示 √ 符号
            Console.OutputEncoding = Encoding.UTF8;

            string sourcePath = @"D:\SteamLibrary\steamapps\workshop\content\431960";
            // 优化 3：将导出的文件统一放入新建的 WallpaperEngineExport 文件夹
            string exportPath = @"C:\Users\Administrator\Downloads\WallpaperEngineExport";

            // 1. 扫描壁纸
            var wallpapers = ScanWallpapers(sourcePath);
            if (wallpapers.Count == 0)
            {
                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey(intercept: true);
                return;
            }

            // 2. 询问是否进入交互导出模式
            Console.WriteLine();
            Console.Write("是否要进入交互模式选择并导出壁纸？(Y = 进入交互导出 / 其他任意键 = 仅查看列表): ");
            var key = Console.ReadKey(intercept: true).Key;

            if (key != ConsoleKey.Y)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("您选择了仅查看列表，以下是扫描到的所有视频壁纸：\n");
                Console.ResetColor();

                PrintWallpaperList(wallpapers);

                Console.WriteLine("\n按任意键退出程序...");
                Console.ReadKey(intercept: true);
                return;
            }

            // 3. 交互式菜单选择
            var selectedWallpapers = MultiSelectMenu(wallpapers);

            // 4. 执行复制并重命名
            if (selectedWallpapers.Count > 0)
            {
                ExportFiles(selectedWallpapers, exportPath);
            }
            else
            {
                Console.WriteLine("\n未选择任何壁纸，已退出。");
            }

            Console.WriteLine("\n按任意键退出程序...");
            Console.ReadKey(intercept: true);
        }

        static List<WallpaperInfo> ScanWallpapers(string targetPath)
        {
            var results = new List<WallpaperInfo>();
            if (!Directory.Exists(targetPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"找不到指定的目录: {targetPath}");
                Console.ResetColor();
                return results;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("正在扫描视频壁纸，请稍候...\n");
            Console.ResetColor();

            string[] mp4Files = Directory.GetFiles(targetPath, "*.mp4", SearchOption.AllDirectories);

            foreach (string mp4 in mp4Files)
            {
                FileInfo fileInfo = new FileInfo(mp4);
                string jsonPath = Path.Combine(fileInfo.DirectoryName, "project.json");
                string title = $"未命名壁纸 (ID {fileInfo.Directory.Name})";

                if (File.Exists(jsonPath))
                {
                    try
                    {
                        string jsonString = File.ReadAllText(jsonPath, Encoding.UTF8);
                        using (JsonDocument doc = JsonDocument.Parse(jsonString))
                        {
                            if (doc.RootElement.TryGetProperty("title", out JsonElement titleElement))
                            {
                                string parsedTitle = titleElement.GetString();
                                if (!string.IsNullOrWhiteSpace(parsedTitle)) title = parsedTitle;
                            }
                        }
                    }
                    catch { /* 忽略解析错误 */ }
                }

                results.Add(new WallpaperInfo
                {
                    FilePath = fileInfo.FullName,
                    Title = title,
                    Time = fileInfo.LastWriteTime
                });
            }

            results = results.OrderByDescending(r => r.Time).ToList();
            Console.WriteLine($"扫描完成！共找到 {results.Count} 个视频壁纸。");
            return results;
        }

        static void PrintWallpaperList(List<WallpaperInfo> wallpapers)
        {
            Console.WriteLine($"{"修改时间",-20} | 标题");
            Console.WriteLine(new string('-', 60));

            foreach (var wp in wallpapers)
            {
                Console.WriteLine($"{wp.Time:yyyy-MM-dd HH:mm:ss} | {wp.Title}");
            }
        }

        static List<WallpaperInfo> MultiSelectMenu(List<WallpaperInfo> options)
        {
            int currentIndex = 0;
            bool[] selected = new bool[options.Count];
            int topIndex = 0;

            // 优化 1：列表加长到 30 行
            int maxVisible = 30;

            Console.CursorVisible = false;

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" 💡 [↑/↓] 移动光标 | [Space/Tab] 选中/取消 | [A] 全选/取消 | [Enter] 导出 | [Esc] 退出");
                Console.WriteLine(new string('-', 95));
                Console.ResetColor();

                if (currentIndex < topIndex) topIndex = currentIndex;
                if (currentIndex >= topIndex + maxVisible) topIndex = currentIndex - maxVisible + 1;

                // 优化 1：顶部省略号
                if (topIndex > 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("   ...");
                    Console.ResetColor();
                }

                for (int i = topIndex; i < Math.Min(topIndex + maxVisible, options.Count); i++)
                {
                    bool isHover = (i == currentIndex);
                    bool isSelected = selected[i];

                    Console.Write(isHover ? " > " : "   ");
                    Console.ForegroundColor = isSelected ? ConsoleColor.Green : ConsoleColor.DarkGray;

                    // 优化 2：选中标志改为 √
                    Console.Write(isSelected ? "[√] " : "[ ] ");

                    if (isHover) Console.ForegroundColor = ConsoleColor.Cyan;
                    else Console.ForegroundColor = ConsoleColor.White;

                    var wp = options[i];
                    Console.WriteLine($"{wp.Time:yyyy-MM-dd} | {wp.Title}");

                    Console.ResetColor();
                }

                // 优化 1：底部省略号
                if (topIndex + maxVisible < options.Count)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("   ...");
                    Console.ResetColor();
                }

                var keyInfo = Console.ReadKey(intercept: true);

                // 优化 1：取消循环滚动，改成触底/触顶停止
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (currentIndex > 0) currentIndex--;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (currentIndex < options.Count - 1) currentIndex++;
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar || keyInfo.Key == ConsoleKey.Tab)
                {
                    selected[currentIndex] = !selected[currentIndex];
                }
                // 优化 1：按 CapsLock 触发全选 / 全不选
                else if (keyInfo.Key == ConsoleKey.A)
                {
                    // 判断当前是否已经全部选中，如果是，则全部取消；否则全部选中
                    bool allSelected = selected.All(x => x);
                    for (int i = 0; i < selected.Length; i++)
                    {
                        selected[i] = !allSelected;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    return new List<WallpaperInfo>();
                }
            }

            Console.CursorVisible = true;
            Console.Clear();

            var resultList = new List<WallpaperInfo>();
            for (int i = 0; i < options.Count; i++)
            {
                if (selected[i]) resultList.Add(options[i]);
            }
            return resultList;
        }

        static void ExportFiles(List<WallpaperInfo> wallpapers, string exportDir)
        {
            Console.WriteLine($"\n准备导出 {wallpapers.Count} 个文件到:\n{exportDir}");
            Console.WriteLine(new string('-', 60));

            // 优化 3：如果没有这个文件夹，会自动创建
            if (!Directory.Exists(exportDir)) Directory.CreateDirectory(exportDir);

            int successCount = 0;

            foreach (var wp in wallpapers)
            {
                string safeTitle = string.Join("_", wp.Title.Split(Path.GetInvalidFileNameChars()));
                string destFileName = $"{safeTitle}.mp4";
                string destPath = Path.Combine(exportDir, destFileName);

                int counter = 1;
                while (File.Exists(destPath))
                {
                    destFileName = $"{safeTitle}({counter}).mp4";
                    destPath = Path.Combine(exportDir, destFileName);
                    counter++;
                }

                try
                {
                    File.Copy(wp.FilePath, destPath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[成功] {destFileName}");
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[失败] {destFileName} -> {ex.Message}");
                }
                Console.ResetColor();
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"🎉 导出完毕！成功: {successCount}/{wallpapers.Count}");
        }
    }

    class WallpaperInfo
    {
        public string FilePath { get; set; }
        public string Title { get; set; }
        public DateTime Time { get; set; }
    }
}