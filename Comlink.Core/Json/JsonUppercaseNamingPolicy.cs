using System;
using System.Text.Json;

namespace Comlink.Core.Json;

internal class JsonUpperCaseNamingPolicy: JsonNamingPolicy
{
    public override String ConvertName(String name) => name.ToUpperInvariant();
}