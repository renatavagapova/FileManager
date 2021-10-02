using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace FileManager
{
    #region Задание
    /*
     * Требуется создать консольный файловый менеджер начального уровня, 
     * который покрывает минимальный набор функционала по работе с файлами.
     * Требования и функции
     * Просмотр файловой структуры
     * 1. Поддержка копирование файлов, каталогов
     * 2. Поддержка удаление файлов, каталогов
     * 3. Получение информации о размерах, системных атрибутов файла, каталога
     * 4. Вывод файловой структуры должен быть постраничным
     * 5. В конфигурационном файле должна быть настройка вывода количества элементов на страницу 
     * 6. При выходе должно сохраняться, последнее состояние
     * 7. Должны быть комментарии в коде
     * 8. Должна быть документация к проекту в формате md
     * 9. Приложение должно обрабатывать непредвиденные ситуации (не падать)
     * 10. При успешном выполнение предыдущих пунктов – реализовать сохранение ошибки в текстовом файле в каталоге errors/random_name_exception.txt
     * 11. При успешном выполнение предыдущих пунктов – реализовать движение по истории команд (стрелочки вверх, вниз)
     */
    #endregion

    class Program
    {
        private static string errorsLogFile = "Errors.json";
        static void Main(string[] args)
        {
            var settings = new Settings();
            StandCursorAtCommandline();
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), errorsLogFile)))
            {
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), errorsLogFile));
            }
            CheckSettingsFile(ref settings);
          
            List<string> userCommands;
            var currentDirectory = settings.lastPath;
            int pageNumber = settings.pageNumber;
            string pathFrom;
            string pathTo;

            Draw(settings);
            AttachedDirectories(currentDirectory, pageNumber, settings);
            AttachedFiles(currentDirectory, pageNumber, settings);
            DirectoryInfo(currentDirectory);

            while (true)
            {
                StandCursorAtCommandline();
                userCommands = ParseString(Console.ReadLine());

                if (userCommands.Count <= 1 && userCommands[0] != "exit")
                {
                    StandCursorAtCommandline();
                    PrintHelp(settings);
                    Console.Write("Неправильная команда. Набор команд в окне информации. 'exit' -  для выхода (Нажмите любую клавишу)");
                    Console.ReadKey();
                    continue;
                }

                var command = userCommands[0];

                switch (command)
                {
                    case "copy":
                        pathFrom = userCommands[1];
                        if (Directory.Exists(pathFrom) && userCommands.Count == 3)
                        {
                            pathTo = userCommands[2];
                            if (!Directory.Exists(pathTo))
                            {
                                CopyDirectory(pathFrom, pathTo);
                            }
                            else
                            {
                                StandCursorAtCommandline();
                                Console.Write($"{pathTo} уже существует (Нажмите любую клавишу)");
                                Console.ReadKey();
                                break;
                            }
                        }
                        else if (File.Exists(pathFrom) && userCommands.Count == 3)
                        {
                            pathTo = userCommands[2];
                            if (!File.Exists(pathTo))
                            {
                                CopyFile(pathFrom, pathTo);
                            }
                            else
                            {
                                StandCursorAtCommandline();
                                Console.Write($"{pathTo} уже существует (Нажмите любую клавишу)");
                                Console.ReadKey();
                                break;
                            }
                        }
                        else
                        {
                            StandCursorAtCommandline();
                            Console.Write($"{pathFrom} не существует (Нажмите любую клавишу)");
                            Console.ReadKey();
                        }
                        AttachedDirectories(currentDirectory, pageNumber, settings);
                        AttachedFiles(currentDirectory, pageNumber, settings);
                        break;

                    case "del":
                        pathFrom = userCommands[1];
                        if (Directory.Exists(pathFrom))
                        {
                            DeleteDirectory(pathFrom);
                        }
                        else if (File.Exists(pathFrom))
                        {
                            DeleteFile(pathFrom);
                        }
                        else
                        {
                            StandCursorAtCommandline();
                            Console.Write($"{pathFrom} не существует (Нажмите любую клавишу)");
                            Console.ReadKey();
                        }
                        AttachedDirectories(currentDirectory, pageNumber, settings);
                        AttachedFiles(currentDirectory, pageNumber, settings);
                        break;

                    case "cd":
                        var newDirectory = userCommands[1];
                        if (Directory.Exists(newDirectory))
                        {
                            if (userCommands.Count == 4)
                            {
                                if (userCommands[2] == "-p")
                                {
                                    try
                                    {
                                        pageNumber = Convert.ToInt32(userCommands[3]);
                                    }
                                    catch (Exception e)
                                    {
                                        StandCursorAtCommandline();
                                        Console.Write("Неверный формат номера страницы");
                                        string rootPath = Directory.GetCurrentDirectory();
                                        if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                                        {
                                            var jsonString = JsonSerializer.Serialize(e.Message);
                                            try
                                            {
                                                File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                                            }
                                            catch
                                            {
                                                Console.Write($"Ошибка записи в файл {errorsLogFile}");
                                            }
                                        }
                                        Console.ReadKey();
                                        break;
                                    }
                                    currentDirectory = newDirectory;
                                    AttachedDirectories(currentDirectory, pageNumber, settings);
                                    AttachedFiles(currentDirectory, pageNumber, settings);
                                    DirectoryInfo(currentDirectory);
                                    break;
                                }
                                else
                                {
                                    StandCursorAtCommandline();
                                    Console.Write("Для команды 'cd' можно использовать только -р аттрибут");
                                    Console.ReadKey();
                                    break;
                                }
                            }
                            currentDirectory = newDirectory;
                            AttachedDirectories(currentDirectory, pageNumber, settings);
                            AttachedFiles(currentDirectory, pageNumber, settings);
                            DirectoryInfo(currentDirectory);
                        }
                        else if (Directory.Exists(Path.Combine(currentDirectory, newDirectory)))
                        {
                            if (userCommands.Count == 4)
                            {
                                if (userCommands[2] == "-p")
                                {
                                    try
                                    {
                                        pageNumber = Convert.ToInt32(userCommands[3]);
                                    }
                                    catch (Exception e)
                                    {
                                        StandCursorAtCommandline();
                                        Console.Write("Неверный формат номера страницы");
                                        string rootPath = Directory.GetCurrentDirectory();
                                        if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                                        {
                                            var jsonString = JsonSerializer.Serialize(e.Message);
                                            try
                                            {
                                                File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                                            }
                                            catch
                                            {
                                                Console.Write($"Ошибка записи в файл {errorsLogFile}");
                                            }
                                        }
                                        Console.ReadKey();
                                        break;
                                    }
                                    currentDirectory = Path.Combine(currentDirectory, newDirectory);
                                    AttachedDirectories(currentDirectory, pageNumber, settings);
                                    AttachedFiles(currentDirectory, pageNumber, settings);
                                    DirectoryInfo(currentDirectory);
                                    break;
                                }
                                else
                                {
                                    StandCursorAtCommandline();
                                    Console.Write("Для команды 'cd' можно использовать только -р аттрибут");
                                    Console.ReadKey();
                                    break;
                                }
                            }
                            currentDirectory = Path.Combine(currentDirectory, newDirectory);
                            AttachedDirectories(currentDirectory, pageNumber, settings);
                            AttachedFiles(currentDirectory, pageNumber, settings);
                            DirectoryInfo(currentDirectory);
                        }
                        else
                        {
                            StandCursorAtCommandline();
                            Console.Write($"{newDirectory} не существует, попробуйте снова (Нажмите любую клавишу)");
                            Console.ReadKey();
                        }
                        break;

                    case "page":
                        if (userCommands.Count == 3)
                        {
                            try
                            {
                                pageNumber = Convert.ToInt32(userCommands[2]);
                            }
                            catch (Exception e)
                            {
                                StandCursorAtCommandline();
                                Console.Write("Неверный формат номера страницы (Нажмите любую клавишу)");
                                string rootPath = Directory.GetCurrentDirectory();
                                if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                                {
                                    var jsonString = JsonSerializer.Serialize(e.Message);
                                    try
                                    {
                                        File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                                    }
                                    catch
                                    {
                                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                                    }
                                }
                                Console.ReadKey();
                                break;
                            }
                            if (userCommands[1] == "-f")
                            {
                                AttachedFiles(currentDirectory, pageNumber, settings);
                            }
                            else if (userCommands[1] == "-d")
                            {
                                AttachedDirectories(currentDirectory, pageNumber, settings);
                            }
                        }
                        else
                        {
                            try
                            {
                                pageNumber = Convert.ToInt32(userCommands[1]);
                            }
                            catch (Exception e)
                            {
                                StandCursorAtCommandline();
                                Console.Write("Неверный формат номера страницы (Нажмите любую клавишу)");
                                string rootPath = Directory.GetCurrentDirectory();
                                if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                                {
                                    var jsonString = JsonSerializer.Serialize(e.Message);
                                    try
                                    {
                                        File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                                    }
                                    catch
                                    {
                                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                                    }
                                }
                                Console.ReadKey();
                                break;
                            }
                            AttachedDirectories(currentDirectory, pageNumber, settings);
                            AttachedFiles(currentDirectory, pageNumber, settings);
                        }
                        break;

                    case "info":
                        pathFrom = userCommands[1];
                        if (File.Exists(pathFrom))
                        {
                            FileInfo(pathFrom);
                        }
                        else
                        {
                            StandCursorAtCommandline();
                            Console.Write($"{pathFrom} не существует, попробуйте снова (Нажмите любую клавишу)");
                            Console.ReadKey();
                        }
                        break;

                    case "size":
                        var path = userCommands[1];
                        int cursorTop = Console.WindowHeight - (settings.infoWindowHeight + settings.commandLineHeight);
                        Console.SetCursorPosition(1, cursorTop + 3);
                        Console.Write($"Size of {path.ToUpper()}: {GetSize(path)} bytes".PadRight(Console.WindowWidth / 2 - 2));
                        break;
                }

                if (userCommands[0] == "exit")
                {
                    settings.lastPath = currentDirectory;
                    SaveSettingFile(settings);
                    break;
                }


            }

        }

        ////команды
        //static void Commands(string commands)
        //{

        //}

        //выставляет курсор на командную строку
        static void StandCursorAtCommandline()
        {
            var settings = new Settings();
            Console.SetCursorPosition(1, Console.WindowHeight - settings.commandLineHeight); //устанавливает положение курсора (позиция столбца/позици строки)
            Console.WriteLine(" ".PadRight(Console.WindowHeight-2));
            Console.SetCursorPosition(1, Console.WindowHeight - settings.commandLineHeight);
            Console.Write(">>");
        }

        //рисует рамку с окнами 
        static void Draw(Settings settings)
        {
            char topRight = '\u2555'; // ╕
            char topLeft = '\u2552'; // ╒
            char topMiddle = '\u2564'; // ╤
            char downRight = '\u2518'; // ┘
            char downLeft = '\u2514'; // └
            char downMiddle = '\u2534'; // ┴
            char verticalLine = '\u2502'; // │
            char horizontalSingleLine = '\u2500'; // ─
            char horizontalDoubleLine = '\u2550'; // ═
            char middleDoubleRight = '\u2561'; // ╡
            char middleDoubleLeft = '\u255E'; // ╞
            char middleDoubleBoth = '\u256A'; // ╪
            char middleSingleRight = '\u2524'; // ┤
            char middleSingleLeft = '\u251C'; // ├

            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Console.WindowHeight; i++) //проходим по всей высоте окна
            {
                Console.SetCursorPosition(Console.WindowWidth - 1, i); //рисуем боковые рамки в конце окна
                Console.Write(verticalLine);
                Console.SetCursorPosition(0, i); //рисуем боковые рамки в левой стороне окна
                Console.Write(verticalLine);
                if (Console.CursorTop < (Console.WindowHeight - settings.commandLineHeight) && Console.CursorTop != 0)
                {
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 1, Console.CursorTop);
                    Console.Write(verticalLine);
                }
                if (Console.CursorTop == 0)
                {
                    for (int j = 1; j < Console.WindowWidth - 1; j++)
                    {
                        Console.Write(horizontalDoubleLine);
                    }
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 1, Console.CursorTop);
                    Console.Write(topMiddle);
                    Console.SetCursorPosition(2, Console.CursorTop);
                    Console.Write("Directories");
                    Console.SetCursorPosition((Console.WindowWidth / 2) + 1, Console.CursorTop);
                    Console.Write("Files");
                }

                if ((Console.WindowHeight - Console.CursorTop - 1) == (settings.infoWindowHeight + settings.commandLineHeight)) //рисуем верхнюю границу окна информации
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(middleDoubleLeft);
                    for (int j = 1; j < Console.WindowWidth - 1; j++)
                    {
                        Console.Write(horizontalDoubleLine);
                    }
                    Console.Write(middleDoubleRight);
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 1, Console.CursorTop);
                    Console.Write(middleDoubleBoth);
                    Console.SetCursorPosition(2, Console.CursorTop);
                    Console.Write("Directory Info");
                    Console.SetCursorPosition((Console.WindowWidth / 2) + 1, Console.CursorTop);
                    Console.Write("Files Info");
                }

                if ((Console.WindowHeight - Console.CursorTop - 1) == settings.commandLineHeight) //рисуем верхнюю границу окна командной строки
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(middleSingleLeft);
                    for (int j = 1; j < Console.WindowWidth - 1; j++)
                    {
                        Console.Write(horizontalSingleLine);
                    }
                    Console.Write(middleSingleRight);
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 1, Console.CursorTop);
                    Console.Write(downMiddle);
                    Console.SetCursorPosition(2, Console.CursorTop);
                    Console.Write("Command line");
                }
                
                if (Console.CursorTop == Console.WindowHeight - 1)
                {
                    for (int j = 1; j < Console.WindowWidth - 1; j++)
                    {
                        Console.Write(horizontalSingleLine);
                    }
                }

            }
            Console.SetCursorPosition(0, 0);
            Console.Write(topLeft);
            Console.SetCursorPosition(Console.WindowWidth - 1, 0);
            Console.Write(topRight);
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(downLeft);
            Console.SetCursorPosition(Console.WindowWidth - 1, Console.WindowHeight - 1);
            Console.Write(downRight);
            return;
        }

        //проверяем наличие файла настроек. Если он есть - свчитываем; нет -используем стандартные
        static void CheckSettingsFile(ref Settings settings)
        {
            Console.Title = "File Manager";
            string path = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(path, settings.settingsFile)))
            {
                try
                {
                    string jsonSettings = File.ReadAllText(Path.Combine(path, settings.settingsFile));
                    settings = JsonSerializer.Deserialize<Settings>(jsonSettings);
                    string[] directories = Directory.GetDirectories(settings.lastPath);
                    Console.SetWindowSize(settings.windowWidth, settings.windowHeight);
                    Console.SetBufferSize(settings.bufferWidth, settings.bufferHeight);
                    return;
                }
                catch (Exception e)
                {
                    StandCursorAtCommandline();
                    Console.Write($"Ошибка при чтении настроек! Подробно в файле {errorsLogFile}. Настройки сброшены");
                    if (File.Exists(Path.Combine(path, errorsLogFile)))
                    {
                        var jsonString = JsonSerializer.Serialize(e.Message);
                        try
                        {
                            File.WriteAllText(Path.Combine(path, errorsLogFile), jsonString);
                        }
                        catch
                        {
                            Console.Write($"Ошибка записи в файл {errorsLogFile}");
                        }
                    }
                    Console.ReadKey();
                }
            }
            settings = new Settings();
        }

        //сохраняем настройки в JSON файле 
        static void SaveSettingFile(Settings settings)
        {
            string path = Directory.GetCurrentDirectory();
            string jsonSettings = JsonSerializer.Serialize(settings);
            try
            {
                File.WriteAllText(Path.Combine(path, settings.settingsFile), jsonSettings);
            }
            catch (Exception e)
            {
                StandCursorAtCommandline();
                Console.Write("Ошибка при записи файла настроек!");
                if (File.Exists(Path.Combine(path, errorsLogFile)))
                {
                    var jsonString = JsonSerializer.Serialize(e.Message);
                    try
                    {
                        File.WriteAllText(Path.Combine(path, errorsLogFile), jsonString);
                    }
                    catch
                    {
                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                    }
                }
                Console.ReadKey();
            }
        }

        //выводим информацию о файле
        static void FileInfo (string path)
        {
            var settings = new Settings();
            FileInfo fileInfo = new FileInfo(path);
            int cursorTop = Console.WindowHeight - (settings.infoWindowHeight + settings.commandLineHeight);

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, cursorTop);
            Console.Write($"File: {path}".PadRight(Console.WindowWidth / 2 - 3));

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, cursorTop + 1);
            Console.Write($"Last Access: {fileInfo.LastAccessTime} / ");
            Console.Write($"Last Write: {fileInfo.LastWriteTime}".PadRight(Console.WindowWidth / 2 - 3));

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, cursorTop + 2);
            Console.Write($"Creation: {fileInfo.CreationTime} / ");
            Console.Write($"Attributes: {fileInfo.Attributes}".PadRight(Console.WindowWidth / 2 - 3));
            StandCursorAtCommandline();
        }

        //выводим информацию о каталоге
        static void DirectoryInfo (string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            var settings = new Settings();
            int cursorTop = Console.WindowHeight - (settings.infoWindowHeight + settings.commandLineHeight);

            Console.SetCursorPosition(1, cursorTop);
            Console.Write($"Creation: {directoryInfo.CreationTime}".PadRight(Console.WindowWidth / 2 - 2));

            Console.SetCursorPosition(1, cursorTop + 1);
            Console.Write($"Last Access: {directoryInfo.LastAccessTime} / ");
            Console.Write($"Last Write: {directoryInfo.LastWriteTime}".PadRight(8));

            Console.SetCursorPosition(1, cursorTop + 2);
            Console.Write($"Contents: {directories.Length} Folders and {files.Length} Files".PadRight(Console.WindowWidth / 2 - 2));
            StandCursorAtCommandline();
        }

        //получаем размер файла или каталога
        static long GetSize(string path)
        {
            if (Directory.Exists(path))
            {
                string[] subDirectories;
                try
                {
                    subDirectories = Directory.GetDirectories(path);
                }
                catch (Exception e)
                {
                    string rootPath = Directory.GetCurrentDirectory();
                    if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                    {
                        var jsonString = JsonSerializer.Serialize(e.Message);
                        try
                        {
                            File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                        }
                        catch
                        {
                            Console.Write($"Ошибка записи в файл {errorsLogFile}");
                        }
                    }
                    return 0;
                }

                var subFiles = Directory.GetFiles(path);
                long size = 0;

                if (subDirectories.Length != 0)
                {
                    foreach (var file in subFiles)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        size += fileInfo.Length;
                    }
                    foreach (var directory in subDirectories)
                    {
                        size += GetSize(directory);
                    }
                }
                else
                {
                    foreach (var file in subFiles)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        size += fileInfo.Length;
                    }
                }
                return size;
            }
            else if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
            else
            {
                return 0;
            }
        }

        //получаем кол-во страниц (рекурсия)
        static int PagesNumber(int pageLines, int amountElements)
        {
            var count = 0;
            if (amountElements >= 0)
            {
                count++;
                amountElements = amountElements - pageLines;
                count = PagesNumber(pageLines, amountElements) + count;
            }
            return count;
        }

        //выводим файлы
        static void AttachedFiles(string path, int pageNumber, Settings settings)
        {
            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, 1);
            Console.Write(path.ToUpper().PadRight(Console.WindowWidth / 2 - 3));

            string[] files = Directory.GetFiles(path);
            var pages = PagesNumber(settings.pageLines, files.Length);
            if (pageNumber <= 0 || pageNumber > pages)
            {
                StandCursorAtCommandline();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (files.Length < settings.pageLines * pageNumber - 2)
            {
                for (int i = (settings.pageLines * pageNumber) - settings.pageLines; i < files.Length; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2 + 1, Console.CursorTop + 1);
                    Console.Write(files[i].Substring(path.Length).PadRight(Console.WindowWidth / 2 - 3));
                }
                for (int i = files.Length; i < Console.WindowHeight - 10; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2 + 1, Console.CursorTop + 1);
                    Console.Write(" ".PadRight(Console.WindowWidth / 2 - 3));
                }
            }
            else
            {
                for (int i = (settings.pageLines * pageNumber) - settings.pageLines; i < settings.pageLines * pageNumber - 2; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2 + 1, Console.CursorTop + 1);
                    Console.Write(files[i].Substring(path.Length).PadRight(Console.WindowWidth / 2 - 3));
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Console.WindowWidth / 4 * 3, Console.WindowHeight - 8);
            Console.Write($"Page {pageNumber} of {pages}");
            StandCursorAtCommandline();
        }

        //выводим каталоги
        static void AttachedDirectories(string path, int pageNumber, Settings settings)
        {
            Console.SetCursorPosition(1, 1);
            Console.Write(path.ToUpper().PadRight(Console.WindowWidth / 2 - 2));

            string[] directories = Directory.GetDirectories(path);
            var pages = PagesNumber(settings.pageLines, directories.Length);
            if (pageNumber <= 0 || pageNumber > pages)
            {
                StandCursorAtCommandline();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (directories.Length < settings.pageLines * pageNumber - 2)
            {
                for (int i = (settings.pageLines * pageNumber) - settings.pageLines; i < directories.Length; i++)
                {
                    Console.SetCursorPosition(1, Console.CursorTop + 1);
                    Console.Write(directories[i].Substring(path.Length).PadRight(Console.WindowWidth / 2 - 2));

                }
                for (int i = directories.Length; i < Console.WindowHeight - 10; i++)
                {
                    Console.SetCursorPosition(1, Console.CursorTop + 1);
                    Console.Write(" ".PadRight(Console.WindowWidth / 2 - 3));
                }
            }
            else
            {
                for (int i = (settings.pageLines * pageNumber) - settings.pageLines; i < settings.pageLines * pageNumber - 2; i++)
                {
                    Console.SetCursorPosition(1, Console.CursorTop + 1);
                    Console.Write(directories[i].Substring(path.Length).PadRight(Console.WindowWidth / 2 - 2));
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Console.WindowWidth / 4, Console.WindowHeight - 8);
            Console.Write($"Page {pageNumber} of {pages}");
            StandCursorAtCommandline();
        }

        //удаление файла
        static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                Console.Write("Удаление успешно");
            }
            catch (Exception e)
            {
                StandCursorAtCommandline();
                Console.Write($"Ошибка при удалении файла: {path}");
                string rootPath = Directory.GetCurrentDirectory();
                if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                {
                    var jsonString = JsonSerializer.Serialize(e.Message);
                    try
                    {
                        File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                    }
                    catch
                    {
                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                    }
                }
                Console.ReadKey();
            }
        }

        //удаление каталога
        static void DeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
                StandCursorAtCommandline();
                Console.Write("Удаление успешно");
            }
            catch (Exception e)
            {
                StandCursorAtCommandline();
                Console.Write($"Ошибка при удалении каталога: {path}");
                string rootPath = Directory.GetCurrentDirectory();
                if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                {
                    var jsonString = JsonSerializer.Serialize(e.Message);
                    try
                    {
                        File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                    }
                    catch
                    {
                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                    }
                }
                Console.ReadKey();
            }
        }

        //копирование файла
        static void CopyFile(string pathFrom, string pathTo)
        {
            try
            {
                File.Copy(pathFrom, pathTo);
                StandCursorAtCommandline();
                Console.Write("Копирование успешно");
            }
            catch (Exception e)
            {
                StandCursorAtCommandline();
                Console.Write($"При копировании произошла ошибка");
                string rootPath = Directory.GetCurrentDirectory();
                if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                {
                    var jsonString = JsonSerializer.Serialize(e.Message);
                    try
                    {
                        File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                    }
                    catch
                    {
                        Console.Write($"Ошибка записи в файл {errorsLogFile}");
                    }
                }
                Console.ReadKey();
            }
        }

        //копирование директории
        static void CopyDirectory(string pathFrom, string pathTo)
        {
            DirectoryInfo dir = new DirectoryInfo(pathFrom);
            DirectoryInfo[] dirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            Directory.CreateDirectory(pathTo);
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(pathTo, file.Name);
                try
                {
                    file.CopyTo(tempPath, false);
                }
                catch (Exception e)
                {
                    StandCursorAtCommandline();
                    Console.Write($"Ошибка при копировании файла {file.Name} (Нажмите любую клавишу)");
                    string rootPath = Directory.GetCurrentDirectory();
                    if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                    {
                        var jsonString = JsonSerializer.Serialize(e.Message);
                        try
                        {
                            File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                        }
                        catch
                        {
                            Console.Write($"Ошибка записи в файл {errorsLogFile}");
                        }
                    }
                    Console.ReadKey();
                }
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(pathTo, subdir.Name);
                try
                {
                    CopyDirectory(subdir.FullName, tempPath);
                }
                catch (Exception e)
                {
                    StandCursorAtCommandline();
                    Console.Write($"Ошибка при копировании директории {subdir.FullName} (Нажмите любую клавишу)");
                    string rootPath = Directory.GetCurrentDirectory();
                    if (File.Exists(Path.Combine(rootPath, errorsLogFile)))
                    {
                        var jsonString = JsonSerializer.Serialize(e.Message);
                        try
                        {
                            File.WriteAllText(Path.Combine(rootPath, errorsLogFile), jsonString);
                        }
                        catch
                        {
                            Console.Write($"Ошибка записи в файл {errorsLogFile}");
                        }
                    }
                    Console.ReadKey();
                }
            }
        }

        //вывод помощи
        static void PrintHelp(Settings settings)
        {
            var height = Console.WindowHeight - settings.infoWindowHeight + 1;

            Console.SetCursorPosition(1, height);
            Console.Write($"'cd ДИРЕКТОРИЯ' - переход в директорию");

            Console.CursorLeft = Console.WindowWidth / 2;
            Console.Write($"'page НОМЕР_СТРАНИЦЫ' - переход по страницам".PadRight(Console.WindowWidth / 2 - 2));

            Console.SetCursorPosition(1, --height);
            Console.Write($"'-p НОМЕР_СТРАНИЦЫ' - переход в директорию на страницу");

            Console.CursorLeft = Console.WindowWidth / 2;
            Console.Write($"'-d' - переход на нужную страницу директорий".PadRight(Console.WindowWidth / 2 - 2));

            Console.SetCursorPosition(1, --height);
            Console.Write($"'del ДИРЕКТОРИЯ' - удаление директории или файла");

            Console.CursorLeft = Console.WindowWidth / 2;
            Console.Write($"'-f' - переход на нужную страницу файлов".PadRight(Console.WindowWidth / 2 - 2));

            Console.SetCursorPosition(1, --height);
            Console.Write($"'info ПУТЬ_К_ФАЙЛУ' - информация о файле");

            Console.CursorLeft = Console.WindowWidth / 2;
            Console.Write($"'copy КОПИРУЕМАЯ_ДИРЕКТОРИЯ КОНЕЧНАЯ_ДИРЕКТОРИЯ' - копирование".PadRight(Console.WindowWidth / 2 - 2));
            StandCursorAtCommandline();
        }

        //разделение строки на подстроки-команды
        static List<string> ParseString(string userCommand)
        {
            var commands = new List<string>();
            string tempString = null;

            if (userCommand != null)
            {
                //проходим по строке от [0] индекса до первого пробела..
                for (int i = 0; i < userCommand.Length; i++)
                {
                    if (userCommand[i] == ' ')
                    {
                        //.. и добавляем в Лист полученную строку-команду.
                        for (int t = 0; t < i; t++)
                        {
                            tempString = tempString + userCommand[t];
                        }
                        commands.Add(tempString);

                        tempString = null;

                        //пропускаем пробел.
                        i++;

                        int tempIndex = i;

                        //если команда COPY, то разбираем строку на составляющие
                        if (commands[0] == "copy")
                        {
                            //пропускаем первое двоеточие начиная с [i + 2] элемента
                            for (int k = i + 2; k < userCommand.Length; k++)
                            {
                                if (userCommand[k] == ':')
                                {
                                    //.. добавляем в Лист первый полученный путь.
                                    for (int h = i; h < k - 2; h++)
                                    {
                                        tempString = tempString + userCommand[h];
                                    }
                                    commands.Add(tempString);
                                    tempString = null;
                                    //.. добавляем в Лист второй полученный путь.
                                    for (int l = k - 1; l < userCommand.Length; l++)
                                    {
                                        tempString = tempString + userCommand[l];
                                    }
                                    commands.Add(tempString);
                                }
                            }
                            return commands;
                        }

                        //если команда INFO или DEL, то разбираем строку на составляющие
                        if (commands[0] == "info" || commands[0] == "del")
                        {
                            for (int k = i; k < userCommand.Length; k++)
                            {
                                tempString = tempString + userCommand[k];
                            }
                            commands.Add(tempString); //.. добавляем в Лист оставшийся путь.
                            return commands;
                        }

                        //если команда CD, то разбираем строку на составляющие
                        if (commands[0] == "cd")
                        {
                            for (int k = i; k < userCommand.Length; k++)
                            {
                                // если находим в строке символ аргумента '-', то создаем строку-путь
                                // из символов от i до символа аргумента
                                if (userCommand[k] == '-')
                                {
                                    for (int j = i; j < k - 1; j++)
                                    {
                                        tempString = tempString + userCommand[j];
                                    }
                                    commands.Add(tempString); //добавляем в лист путь

                                    tempString = null;
                                    tempString = tempString + userCommand[k];
                                    tempString = tempString + userCommand[k + 1]; //добавляем аргумент
                                    commands.Add(tempString);

                                    tempString = null;
                                    tempString = tempString + userCommand[k + 3]; //добавляем номер страницы
                                    commands.Add(tempString);

                                    return commands;
                                }
                                //если не находим аргумент, то добавляем оставшиеся символы в строку-путь
                                if (k + 1 == userCommand.Length)
                                {
                                    for (int j = i; j < userCommand.Length; j++)
                                    {
                                        tempString = tempString + userCommand[j];
                                    }
                                    commands.Add(tempString);
                                    return commands;
                                }
                            }
                        }
                        // для остальных команд строку не надо разбирать на составляющие

                        for (int j = i; j < userCommand.Length; j++)  //продолжаем идти по строке начиная с i-го индекса,
                        {
                            //если после пути есть пробел, то добавляем путь в Лист и продолжаем идти по строке
                            if (userCommand[j] == ' ')
                            {
                                commands.Add(userCommand.Substring(tempIndex, j - tempIndex));
                                tempIndex = ++j; //пропускаем пробел
                            }
                            //если строка закончится на следующей итерации цикла, то добавляем полученный путь в Лист
                            if (j + 1 == userCommand.Length)
                            {
                                commands.Add(userCommand.Substring(tempIndex, (j + 1) - tempIndex));
                                break;
                            }
                        }
                        break;
                    }
                    if (i + 1 == userCommand.Length) //если строка закончится на следующей итерации цикла, то добавляем полученную команду в Лист
                    {
                        commands.Add(userCommand);
                    }
                }
            }
            return commands;

        }





    }


}
