using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Serilog.Formatting.Compact; // Пакет для компактного JSON

namespace TaskManagerApp
{
    internal class Program
    {
        private static readonly List<string> Tasks = new List<string>();

        static void Main(string[] args)
        {
            // 1. Настройка структурированного логирования:
            // - В консоль: читаемый текст с подсветкой и свойствами
            // - В файл: чистый структурированный JSON (logs/structured-log-.json)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Application", "TaskManager")
                .Enrich.WithProperty("Environment", "Development")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(), // Структурированный JSON-формат
                    path: "logs/structured-log-.json",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Программа {ApplicationName} запущена.", "TaskManager");

                bool isRunning = true;
                while (isRunning)
                {
                    Console.Write("\nВведите команду (add, remove, list, exit): ");
                    string? command = Console.ReadLine()?.Trim().ToLower();

                    Log.Debug("Пользователь запросил команду {Command}", command);

                    switch (command)
                    {
                        case "add":
                            AddTask();
                            break;
                        case "remove":
                            RemoveTask();
                            break;
                        case "list":
                            ListTasks();
                            break;
                        case "exit":
                            Log.Information("Завершение работы программы.");
                            isRunning = false;
                            break;
                        default:
                            Log.Warning("Введена неизвестная команда {InvalidCommand}", command);
                            Console.WriteLine("Команда не распознана.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критический сбой в работе приложения");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void AddTask()
        {
            var sw = Stopwatch.StartNew();
            Log.Debug("Начало операции {Operation}", nameof(AddTask));

            Console.Write("Введите название задачи: ");
            string? taskName = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(taskName))
            {
                Tasks.Add(taskName);
                sw.Stop();
                // Структурированная запись с именованными полями TaskName, TaskCount, DurationMs
                Log.Information("Задача \"{TaskName}\" успешно добавлена. Всего задач: {TaskCount}. Время: {DurationMs}мс",
                    taskName, Tasks.Count, sw.ElapsedMilliseconds);
            }
            else
            {
                sw.Stop();
                Log.Warning("Попытка добавления пустой задачи. Время: {DurationMs}мс", sw.ElapsedMilliseconds);
            }

            Log.Debug("Конец операции {Operation}", nameof(AddTask));
        }

        private static void RemoveTask()
        {
            var sw = Stopwatch.StartNew();
            Log.Debug("Начало операции {Operation}", nameof(RemoveTask));

            Console.Write("Введите название задачи для удаления: ");
            string? taskName = Console.ReadLine()?.Trim();

            if (Tasks.Remove(taskName ?? string.Empty))
            {
                sw.Stop();
                Log.Information("Задача \"{TaskName}\" успешно удалена. Осталось: {TaskCount}. Время: {DurationMs}мс",
                    taskName, Tasks.Count, sw.ElapsedMilliseconds);
            }
            else
            {
                sw.Stop();
                // Структурированная ошибка с кодом ошибки и параметром
                Log.Error("Задача \"{TaskName}\" не найдена. Ошибка поиска {ErrorCode}. Время: {DurationMs}мс",
                    taskName, 404, sw.ElapsedMilliseconds);
            }

            Log.Debug("Конец операции {Operation}", nameof(RemoveTask));
        }

        private static void ListTasks()
        {
            Log.Debug("Начало операции {Operation}", nameof(ListTasks));
            Log.Information("Запрошен список задач. Текущее количество: {TotalCount}", Tasks.Count);

            if (Tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
            }
            else
            {
                for (int i = 0; i < Tasks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {Tasks[i]}");
                }
            }

            Log.Debug("Конец операции {Operation}", nameof(ListTasks));
        }
    }
}