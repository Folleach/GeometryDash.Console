using System;

namespace Common;

public static class Question
{
    public static string? Make(string question, Func<string?, bool> validate, string errorMessage)
    {
        while (true)
        {
            Console.Write($"{question}: ");
            var response = Console.ReadLine();
            if (validate(response))
                return response;
            Console.WriteLine(errorMessage);
        }
    }
}
