using System;
using System.Collections.Generic;

namespace xp_apps.sources
{
    // TO BE CONTINUED ...
    public class Extensions
    {
        private static void MultiSelection()
        {
            var cities = new List<string> { "Seattle", "London", "Tokyo" };
            var selectedCities = new HashSet<string>();
            var currentSelection = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Which cities would you like to visit? Hit space to select");

                for (var i = 0; i < cities.Count; i++)
                {
                    Console.Write(selectedCities.Contains(cities[i]) ? "[x] " : "[ ] ");

                    if (i == currentSelection)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    Console.WriteLine(cities[i]);
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        currentSelection = currentSelection == 0 ? cities.Count - 1 : currentSelection - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        currentSelection = currentSelection == cities.Count - 1 ? 0 : currentSelection + 1;
                        break;
                    case ConsoleKey.Spacebar:
                        if (!selectedCities.Add(cities[currentSelection]))
                            selectedCities.Remove(cities[currentSelection]);
                        break;
                }
            } while (key != ConsoleKey.Enter);

            Console.Clear();
            Console.WriteLine("You have selected:");
            foreach (var city in selectedCities)
                Console.WriteLine(city);
        }

        private static void SingleSelecton()
        {
            var cities = new List<string> { "Seattle", "London", "Tokyo" };
            var currentSelection = 0;

            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Which city would you like to visit? Hit space to select");

                for (var i = 0; i < cities.Count; i++)
                {
                    if (i == currentSelection)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    Console.Write(i == currentSelection ? "[x] " : "[ ] ");
                    Console.WriteLine(cities[i]);
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        currentSelection = currentSelection == 0 ? cities.Count - 1 : currentSelection - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        currentSelection = currentSelection == cities.Count - 1 ? 0 : currentSelection + 1;
                        break;
                }
            } while (key != ConsoleKey.Enter && key != ConsoleKey.Spacebar);

            Console.Clear();
            Console.WriteLine("You have selected: " + cities[currentSelection]);
        }
    }
}