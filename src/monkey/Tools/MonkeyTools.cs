using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using System;
using System.ComponentModel;
using System.Text.Json;
using Monkey.Services;
using static Monkey.ToolsInformation;

namespace Monkey
{    
    public static class MonkeyTools
    {
        [Function("GetMonkeys")]
        public static async Task<string> GetMonkeys([McpToolTrigger(GetMonkeysToolName, GetMonkeysToolDescription)] ToolInvocationContext context)
        {
            var monkeyService = new MonkeyService();
            var monkeys = await monkeyService.GetMonkeys();
            return JsonSerializer.Serialize(monkeys);
        }
        
        [Function("GetMonkey")]
        public static async Task<string> GetMonkey([McpToolTrigger(GetMonkeyToolName, GetMonkeyToolDescription)] ToolInvocationContext context,  
             [McpToolProperty(MonkeyNamePropertyName, PropertyType, MonketNamePropertyDescription )] string name)
        {
            var monkeyService = new MonkeyService();
            var monkey = await monkeyService.GetMonkey(name);
            return JsonSerializer.Serialize(monkey);
        }                
    }
}