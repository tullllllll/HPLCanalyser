using System;

namespace HPLC.Services;

public class NavigationService
{
    public Action<string> Navigate { get; set; }
}