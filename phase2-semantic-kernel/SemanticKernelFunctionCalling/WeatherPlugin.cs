using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;

namespace SemanticKernelFunctionCalling
{
    public class WeatherPlugin
    {
        [KernelFunction]
        [Description("Gets the current weather for a given city.")]
        public string GetWeather(
        [Description("The name of the city, e.g. Delhi")] string city)
        {
            return $"{city} mein abhi 32°C aur clear sky hai.";
        }

        [KernelFunction]
        [Description("Suggests clothing based on a weather description.")]
        public string SuggestClothing(
        [Description("A weather description, e.g. '32°C and clear sky'")] string weatherDescription)
        {
            return "Halke cotton clothes pehno, garmi hai.";
        }
    }
}
