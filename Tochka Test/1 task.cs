using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var timeline = new List<(string date, ActionType action)>();
        var counter = 0;
        
        foreach (var guest in guests)
        {
            timeline.Add((guest.CheckIn, ActionType.CheckIn));
            timeline.Add((guest.CheckOut, ActionType.CheckOut));
        }
        
        timeline.Sort((a, b) => String.Compare(a.date, b.date, StringComparison.Ordinal));
        foreach (var time in timeline)
        {
            if (time.action == ActionType.CheckIn)
                counter++;
            else
                counter--;

            if (counter > maxCapacity)
                return false;
        }

        return true;
    }

    enum ActionType
    {
        CheckIn,
        CheckOut
    }

    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    static void Main()
    {
        var maxCapacity = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());


        var guests = new List<Guest>();


        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var guest = ParseGuest(line);
            guests.Add(guest);
        }

        var result = CheckCapacity(maxCapacity, guests);
        
        Console.WriteLine(result ? "True" : "False");
    }


    // Простой парсер JSON-строки для объекта Guest
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();
        
        // Извлекаем имя
        var nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;
        
        // Извлекаем дату заезда
        var checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;
        
        // Извлекаем дату выезда
        var checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;
        
        return guest;
    }
}