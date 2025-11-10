using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Console;

namespace VendingMachines.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string file = @"D:\IDE Projects\Visual Studio\C#\VendingMachines\VendingMachines.API\Controllers\DevicesController.cs";

            if (!File.Exists(file))
            {
                WriteLine($"Файл не найден: {file}");
                return;
            }

            try
            {
                var code = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();

                // Подсчёт операторов (n1, N1)
                var operatorNodes = root.DescendantNodes()
                    .Where(n => n is BinaryExpressionSyntax
                             || n is IfStatementSyntax
                             || n is InvocationExpressionSyntax
                             || n is AssignmentExpressionSyntax
                             || n is ReturnStatementSyntax)
                    .ToList();
                int N1 = operatorNodes.Count;
                int n1 = operatorNodes.Select(n => n.Kind().ToString()).Distinct().Count();

                // Подсчёт операндов (n2, N2)
                var operandNodes = root.DescendantNodes()
                    .Where(n => n is IdentifierNameSyntax || n is LiteralExpressionSyntax)
                    .ToList();
                int N2 = operandNodes.Count;
                int n2 = operandNodes.Select(n => n.ToString()).Distinct().Count();

                // Расчёт метрик Холстеда
                int n = n1 + n2;
                int N = N1 + N2;
                double V = n > 0 ? N * Math.Log2(n) : 0;
                double D = n2 > 0 ? (n1 / 2.0) * (N2 / (double)n2) : 0;
                double E = D * V;
                double B = V / 3000.0;

                WriteLine($"Файл: {file}");
                WriteLine($"Операторы (N1): {N1}, Уникальные (n1): {n1}");
                WriteLine($"Операнды (N2): {N2}, Уникальные (n2): {n2}");
                WriteLine($"\nМетрики Холстеда для {file}:");
                WriteLine($"n1 (уникальные операторы): {n1}");
                WriteLine($"n2 (уникальные операнды): {n2}");
                WriteLine($"N1 (общее количество операторов): {N1}");
                WriteLine($"N2 (общее количество операндов): {N2}");
                WriteLine($"Словарь (n): {n}");
                WriteLine($"Длина (N): {N}");
                WriteLine($"Объём (V): {V:F2}");
                WriteLine($"Сложность (D): {D:F2}");
                WriteLine($"Усилие (E): {E:F2}");
                WriteLine($"Ожидаемые ошибки (B): {B:F2}");

                // Подсчёт метрик Миллса
                WriteLine("\nВведите данные для модели Миллса:");
                Write("S (количество искусственных ошибок, введённых в код): ");
                string sInput = ReadLine();
                if (!int.TryParse(sInput, out int S) || S < 0)
                {
                    WriteLine("Ошибка: S должно быть неотрицательным числом. Используется S = 10.");
                    S = 10;
                }

                Write("s (количество найденных искусственных ошибок тестами): ");
                string sFoundInput = ReadLine();
                if (!int.TryParse(sFoundInput, out int s) || s < 0 || s > S)
                {
                    WriteLine($"Ошибка: s должно быть от 0 до {S}. Используется s = {S / 2}.");
                    s = S / 2;
                }

                Write("k (количество найденных реальных ошибок тестами): ");
                string kInput = ReadLine();
                if (!int.TryParse(kInput, out int k) || k < 0)
                {
                    WriteLine("Ошибка: k должно быть неотрицательным числом. Используется k = 0.");
                    k = 0;
                }

                // Расчёт Миллса
                double N_mills = s > 0 ? (S * (k + 1.0)) / (s + 1.0) : 0; // Гипергеометрическая формула
                double remainingBugs = N_mills - k;

                WriteLine($"\nМетрики Миллса для {file}:");
                WriteLine($"S (введённые искусственные ошибки): {S}");
                WriteLine($"s (найденные искусственные ошибки): {s}");
                WriteLine($"k (найденные реальные ошибки): {k}");
                WriteLine($"Оценка общего количества ошибок (N): {N_mills:F2}");
                WriteLine($"Оставшиеся ошибки: {remainingBugs:F2}");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при анализе {file}: {ex.Message}");
            }
        }
    }
}